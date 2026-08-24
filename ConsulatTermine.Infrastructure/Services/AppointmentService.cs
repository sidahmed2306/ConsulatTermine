using System.Globalization;
using ConsulatTermine.Application.DTOs;
using ConsulatTermine.Application.Exceptions;
using ConsulatTermine.Application.Interfaces;
using ConsulatTermine.Application.Localization;
using ConsulatTermine.Application.Resources;
using ConsulatTermine.Application.Services;
using ConsulatTermine.Domain.Entities;
using ConsulatTermine.Domain.Enums;
using ConsulatTermine.Infrastructure.Persistence;
using ConsulatTermine.Infrastructure.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ConsulatTermine.Infrastructure.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly IHubContext<DisplayHub, IDisplayClient> _displayHub;
    private readonly IHubContext<EmployeeHub, IEmployeeClient> _employeeHub;
    private readonly IWaitingRoomNotifier _waitingRoomNotifier;

    private readonly IEmailService _emailService;
    private readonly ILogger<AppointmentService> _logger;



    public AppointmentService(
  IDbContextFactory<ApplicationDbContext> contextFactory,
  IHubContext<DisplayHub, IDisplayClient> displayHub,
  IHubContext<EmployeeHub, IEmployeeClient> employeeHub,
  IEmailService emailService,
  IWaitingRoomNotifier waitingRoomNotifier,
  ILogger<AppointmentService> logger)
    {
        _contextFactory = contextFactory;
        _displayHub = displayHub;
        _employeeHub = employeeHub;
        _emailService = emailService;
        _logger = logger;
        _waitingRoomNotifier = waitingRoomNotifier;
    }



    // -------------------------------------------------------------
    // FREIE SLOTS ALS DTOs (für UI)
    // -------------------------------------------------------------
    public async Task<List<AvailableSlotDto>> GetAvailableSlotDtosAsync(int serviceId, DateTime appointmentDate)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        // Service laden (Kapazität über AssignedEmployees)
        var service = await db.Services
            .Include(s => s.AssignedEmployees)
            .FirstOrDefaultAsync(s => s.Id == serviceId);

        if (service == null)
        {
            throw new BusinessRuleViolationException(BusinessMessages.Get("ServiceNotFound"));
        }

        // Aktiven Plan laden
        var plan = await GetActivePlanAsync(serviceId);
        if (plan == null)
        {
            return new List<AvailableSlotDto>();
        }

        // Datum muss im Plan-Zeitraum liegen (sonst KEINE Slots)
        if (!IsInsidePlan(plan, appointmentDate))
        {
            return new List<AvailableSlotDto>();
        }

        // Plan-gebundene WorkingHours/Overrides laden
        var workingHours = await db.WorkingHours
            .Where(w => w.ServiceId == serviceId && w.WorkingSchedulePlanId == plan.Id)
            .ToListAsync();

        var overrides = await db.ServiceDayOverrides
            .Where(o => o.ServiceId == serviceId && o.WorkingSchedulePlanId == plan.Id)
            .ToListAsync();

        // Existierende Termine an dem Tag
        var appointments = await db.Appointments
            .Where(a => a.ServiceId == serviceId && a.Date.Date == appointmentDate.Date)
            .ToListAsync();

        // NEUE Signatur
        var slots = AppointmentCalculator.GetAvailableSlots(
            service,
            appointmentDate,
            workingHours,
            overrides,
            appointments
        );

        // Mapping ins DTO
        return slots
            .Select(kv => new AvailableSlotDto
            {
                SlotStart = appointmentDate.Date + kv.Key.Start,
                FreeCapacity = kv.Value
            })
            .OrderBy(x => x.SlotStart)
            .ToList();
    }

    /// <summary>
    /// Liefert die freien Slots aller Tage eines Monats in einem Zug.
    /// </summary>
    /// <remarks>
    /// Der Kalender der Buchung braucht immer den ganzen Monat. Eine Abfrage je
    /// Tag ergab dafuer rund dreissig Rundreisen zur Datenbank, waehrend derer
    /// die Oberflaeche unvollstaendig war. Stammdaten und Termine werden deshalb
    /// einmal geladen und die Tage im Speicher ausgewertet.
    /// </remarks>
    public async Task<Dictionary<DateOnly, List<AvailableSlotDto>>> GetAvailableSlotDtosForMonthAsync(
        int serviceId,
        DateOnly monthFirstDay)
    {
        var slotsByDay = new Dictionary<DateOnly, List<AvailableSlotDto>>();

        await using var db = await _contextFactory.CreateDbContextAsync();

        var service = await db.Services
            .Include(s => s.AssignedEmployees)
            .FirstOrDefaultAsync(s => s.Id == serviceId);

        if (service == null)
        {
            throw new BusinessRuleViolationException(BusinessMessages.Get("ServiceNotFound"));
        }

        var plan = await GetActivePlanAsync(serviceId);
        if (plan == null)
        {
            return slotsByDay;
        }

        var workingHours = await db.WorkingHours
            .Where(w => w.ServiceId == serviceId && w.WorkingSchedulePlanId == plan.Id)
            .ToListAsync();

        var overrides = await db.ServiceDayOverrides
            .Where(o => o.ServiceId == serviceId && o.WorkingSchedulePlanId == plan.Id)
            .ToListAsync();

        var firstDay = new DateOnly(monthFirstDay.Year, monthFirstDay.Month, 1);
        var monthStart = firstDay.ToDateTime(TimeOnly.MinValue);
        var monthEndExclusive = firstDay.AddMonths(1).ToDateTime(TimeOnly.MinValue);

        var appointments = await db.Appointments
            .Where(a => a.ServiceId == serviceId
                        && a.Date >= monthStart
                        && a.Date < monthEndExclusive)
            .ToListAsync();

        var dayCount = DateTime.DaysInMonth(firstDay.Year, firstDay.Month);

        for (var dayOfMonth = 1; dayOfMonth <= dayCount; dayOfMonth++)
        {
            var day = new DateOnly(firstDay.Year, firstDay.Month, dayOfMonth);
            var date = day.ToDateTime(TimeOnly.MinValue);

            // Ausserhalb des aktiven Plans gibt es keine Slots.
            if (!IsInsidePlan(plan, date))
            {
                slotsByDay[day] = new List<AvailableSlotDto>();
                continue;
            }

            var slots = AppointmentCalculator.GetAvailableSlots(
                service,
                date,
                workingHours,
                overrides,
                appointments);

            slotsByDay[day] = slots
                .Select(kv => new AvailableSlotDto
                {
                    SlotStart = date + kv.Key.Start,
                    FreeCapacity = kv.Value
                })
                .OrderBy(x => x.SlotStart)
                .ToList();
        }

        return slotsByDay;
    }

    // -------------------------------------------------------------
    // TERMIN BUCHEN
    // -------------------------------------------------------------
    public async Task<Appointment> BookAsync(
        int serviceId,
        DateTime slotStart,
        string fullName,
        string email)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var appointmentDate = slotStart.Date;

        // Freie Slots für diesen Tag holen (plan-basiert)
        var available = await GetAvailableSlotDtosAsync(serviceId, appointmentDate);

        // Passenden Slot finden
        var slotDto = available.SingleOrDefault(s => s.SlotStart == slotStart);
        if (slotDto == null)
        {
            throw new BusinessRuleViolationException(BusinessMessages.Get("SlotInvalid"));
        }

        if (!slotDto.IsAvailable)
        {
            throw new BusinessRuleViolationException(BusinessMessages.Get("SlotFullyBooked"));
        }

        var appointment = new Appointment
        {
            FullName = fullName,
            Email = email,
            Date = slotStart,
            ServiceId = serviceId,
            Status = AppointmentStatus.Booked,
            CreatedAt = DateTime.UtcNow
        };

        db.Appointments.Add(appointment);
        await db.SaveChangesAsync();

        return appointment;
    }
    // -------------------------------------------------------------
    // TERMIN STORNIEREN (THREAD-SAFE, EF-CORRECT)
    // -------------------------------------------------------------
    public async Task<bool> CancelAsync(int appointmentId)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        // 🔎 Termin inkl. Service laden
        var appointment = await db.Appointments
            .Include(a => a.Service)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment == null)
        {
            return false;
        }

        if (appointment.Status == AppointmentStatus.Completed)
        {
            return false;
        }

        if (appointment.Status == AppointmentStatus.Cancelled)
        {
            return true; // idempotent
        }

        // ❌ Termin stornieren
        appointment.Status = AppointmentStatus.Cancelled;
        await db.SaveChangesAsync();

        // ---------------------------------------------------------
        // 🔎 ALLE Termine dieser Buchung laden (GLEICHER THREAD)
        // ---------------------------------------------------------
        var allAppointments = await db.Appointments
            .Include(a => a.Service)
            .Where(a => a.BookingReference == appointment.BookingReference)
            .ToListAsync();

        var mainPerson = allAppointments
            .FirstOrDefault(a => a.IsMainPerson && !string.IsNullOrWhiteSpace(a.Email));

        if (mainPerson == null)
        {
            return true;
        }

        var hasActiveAppointments = allAppointments
            .Any(a => a.Status == AppointmentStatus.Booked);

        // ---------------------------------------------------------
        // 📧 E-MAIL (kein Task.Run, kein Parallelismus)
        // ---------------------------------------------------------
        try
        {
            if (hasActiveAppointments)
            {
                // 🟨 TEIL-ABSAGE
                // Die Sprache stammt aus dem Termin und nicht aus der Sitzung des
                // Mitarbeiters: der Empfaenger ist der Buerger, der gebucht hat.
                await _emailService.SendPartialCancellationAsync(
                    mainPerson.Email!,
                    appointment.FullName,
                    appointment.Service!.NameFor(appointment.Language),
                    appointment.Date,
                    appointment.Language);
            }
            else
            {
                // 🟥 VOLL-ABSAGE
                await _emailService.SendCancellationConfirmationAsync(
                    mainPerson.Email!,
                    mainPerson.FullName,
                    appointment.BookingReference,
                    appointment.Language);
            }
        }
        catch (Exception ex)
        {
            // Der Versand der Absage-Mail darf die Stornierung nicht verhindern:
            // der Termin ist bereits abgesagt. Der Fehler wird protokolliert, damit
            // er im Betrieb sichtbar ist, statt stillschweigend zu verschwinden.
            ServiceLog.CancellationMailFailed(_logger, ex, appointmentId);
        }

        return true;
    }



    public async Task<List<Appointment>> GetByBookingReferenceAsync(string bookingReference)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        if (string.IsNullOrWhiteSpace(bookingReference))
        {
            return new List<Appointment>();
        }

        return await db.Appointments
            .Include(a => a.Service)
            .Where(a => a.BookingReference == bookingReference)
            .OrderBy(a => a.Date)
            .ToListAsync();
    }


    // -------------------------------------------------------------
    // CHECK-IN (Empfang)
    // -------------------------------------------------------------
    public async Task<bool> CheckInAsync(int appointmentId)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var appointment = await db.Appointments.FindAsync(appointmentId);
        if (appointment == null)
        {
            return false;
        }

        if (appointment.Status != AppointmentStatus.Booked)
        {
            return false;
        }

        appointment.Status = AppointmentStatus.CheckedIn;
        appointment.CheckedInAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        await _employeeHub.Clients.All.StatusUpdated(appointment.Id, appointment.Status);

        return true;
    }

    // -------------------------------------------------------------
    // BEARBEITUNG STARTEN (Mitarbeiter nimmt Bürger dran)
    // -------------------------------------------------------------
    public async Task<bool> StartProcessingAsync(int appointmentId, int employeeId)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        // 🔒 ATOMARES UPDATE
        var affected = await db.Database.ExecuteSqlInterpolatedAsync($@"
    UPDATE Appointments
    SET 
        Status = {(int)AppointmentStatus.InProgress},
        CurrentEmployeeId = {employeeId},
        IsVisibleInWaitingRoom = 1
    WHERE 
        Id = {appointmentId}
        AND Status = {(int)AppointmentStatus.Booked}
");

        if (affected == 0)
        {
            return false;
        }

        // 🔴 DAS HAT GEFEHLT
        db.ChangeTracker.Clear();

        return true;

    }









    // -------------------------------------------------------------
    // TERMIN ABSCHLIESSEN
    // -------------------------------------------------------------
    public async Task<bool> CompleteAsync(int appointmentId, int employeeId)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var appointment = await db.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment == null)
        {
            return false;
        }

        // 🔐 Sicherheitscheck: nur der Mitarbeiter, der ihn gestartet hat
        if ((appointment.Status != AppointmentStatus.InProgress &&
         appointment.Status != AppointmentStatus.AtDesk) ||
        appointment.CurrentEmployeeId != employeeId)
        {
            return false;
        }


        // ✅ FINALER STATUSWECHSEL
        appointment.Status = AppointmentStatus.Completed;
        appointment.CompletedAt = DateTime.UtcNow;

        // 🧹 Arbeitsplatz / Mitarbeiter freigeben
        appointment.CurrentEmployeeId = null;

        await db.SaveChangesAsync();
        return true;
    }


    public async Task<bool> HideFromWaitingRoomAsync(int appointmentId, int employeeId)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var appointment = await db.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment == null)
        {
            return false;
        }

        // Sicherheitscheck: nur eigener Termin
        if (appointment.CurrentEmployeeId != employeeId)
        {
            return false;
        }

        // Nur sinnvoll bei aktiver Bearbeitung
        if (appointment.Status != AppointmentStatus.InProgress)
        {
            return false;
        }

        // ✅ Fachlicher Zustand: Bürger ist am Schalter
        appointment.Status = AppointmentStatus.AtDesk;

        // ✅ Nicht mehr im Wartezimmer anzeigen
        appointment.IsVisibleInWaitingRoom = false;

        await db.SaveChangesAsync();

        _waitingRoomNotifier.Notify();
        return true;
    }



    // -------------------------------------------------------------
    // GRUPPENBUCHUNG (1–5 Personen, mehrere Slots möglich)
    // -------------------------------------------------------------
    public async Task<List<Appointment>> BookGroupAsync(GroupBookingDto dto)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var result = new List<Appointment>();

        // Service laden (Kapazität über AssignedEmployees)
        var service = await db.Services
            .Include(s => s.AssignedEmployees)
            .FirstOrDefaultAsync(s => s.Id == dto.ServiceId);

        if (service == null)
        {
            throw new BusinessRuleViolationException(BusinessMessages.Get("ServiceNotFound"));
        }

        // Gruppengröße prüfen
        if (dto.TotalPersons < 1 || dto.TotalPersons > 5)
        {
            throw new BusinessRuleViolationException(BusinessMessages.Get("GroupSizeInvalid"));
        }

        if (dto.Persons.Count != dto.TotalPersons)
        {
            throw new BusinessRuleViolationException(BusinessMessages.Get("PersonCountMismatch"));
        }

        var allSlotStarts = dto.Persons.Select(p => p.SlotStart).ToList();

        // Alle Slots müssen am selben Tag liegen
        var appointmentDate = allSlotStarts.First().Date;
        if (allSlotStarts.Any(s => s.Date != appointmentDate.Date))
        {
            throw new BusinessRuleViolationException(BusinessMessages.Get("SlotsSameDay"));
        }

        // Aktiven Plan laden
        var plan = await GetActivePlanAsync(dto.ServiceId);
        if (plan == null)
        {
            throw new BusinessRuleViolationException(BusinessMessages.Get("NoSchedulePlan"));
        }

        // Datum muss im Plan-Zeitraum liegen
        if (!IsInsidePlan(plan, appointmentDate))
        {
            throw new BusinessRuleViolationException(BusinessMessages.Get("DateOutsidePlan"));
        }

        // Plan-gebundene WorkingHours/Overrides laden
        var workingHours = await db.WorkingHours
            .Where(w => w.ServiceId == dto.ServiceId && w.WorkingSchedulePlanId == plan.Id)
            .ToListAsync();

        var overrides = await db.ServiceDayOverrides
            .Where(o => o.ServiceId == dto.ServiceId && o.WorkingSchedulePlanId == plan.Id)
            .ToListAsync();

        // Existierende Termine für diesen Tag
        var existing = await db.Appointments
            .Where(a => a.ServiceId == dto.ServiceId && a.Date.Date == appointmentDate.Date)
            .ToListAsync();

        // Slots berechnen (NEUE Signatur)
        var freeSlots = AppointmentCalculator.GetAvailableSlots(
            service,
            appointmentDate,
            workingHours,
            overrides,
            existing
        );

        // Validierung: jeder Slot hat genug Kapazität
        foreach (var slotGroup in dto.Persons.GroupBy(p => p.SlotStart))
        {
            var slotStart = slotGroup.Key.TimeOfDay;

            var slot = freeSlots.Keys.FirstOrDefault(k => k.Start == slotStart);
            if (slot.Start == default)
            {
                throw new BusinessRuleViolationException(BusinessMessages.Format("SlotInvalidAt", slotStart.ToString("HH:mm", CultureInfo.CurrentCulture)));
            }

            int free = freeSlots[slot];
            int needed = slotGroup.Count();

            if (needed > free)
            {
                throw new BusinessRuleViolationException(BusinessMessages.Format("SlotCapacityShort", slotStart.ToString("HH:mm", CultureInfo.CurrentCulture), free, needed));
            }
        }

        // Speichern in Transaktion
        using var trx = await db.Database.BeginTransactionAsync();

        try
        {
            foreach (var person in dto.Persons)
            {
                var appointment = new Appointment
                {
                    FullName = person.FullName,
                    Email = person.Email,
                    Date = person.SlotStart,
                    ServiceId = dto.ServiceId,
                    Status = AppointmentStatus.Booked,
                    CreatedAt = DateTime.UtcNow
                };

                db.Appointments.Add(appointment);
                result.Add(appointment);
            }

            await db.SaveChangesAsync();
            await trx.CommitAsync();
        }
        catch
        {
            await trx.RollbackAsync();
            throw;
        }

        return result;
    }

    public async Task<List<Appointment>> GetAppointmentsForServiceOnDateAsync(
int serviceId,
DateTime appointmentDate)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var targetDate = appointmentDate.Date;
        return await db.Appointments
            .AsNoTracking() // 🔴 WICHTIG
            .Where(a =>
                a.ServiceId == serviceId &&
                a.Date.Date == targetDate)
            .OrderBy(a => a.Date)
            .ToListAsync();

    }

    // -------------------------------------------------------------
    // Helpers: Active Plan + Range Check
    // -------------------------------------------------------------
    /// <summary>
    /// Liest den aktiven Arbeitszeitplan eines Service. Reine Leseabfrage ohne Bezug zu
    /// offenen Änderungen des Aufrufers, daher mit eigenem, kurzlebigem Kontext.
    /// </summary>
    private async Task<WorkingSchedulePlan?> GetActivePlanAsync(int serviceId)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        return await db.WorkingSchedulePlans
            .AsNoTracking()
            .Where(p => p.ServiceId == serviceId && p.IsActive)
            .OrderByDescending(p => p.ValidFromDate)
            .FirstOrDefaultAsync();
    }

    private static bool IsInsidePlan(WorkingSchedulePlan plan, DateTime appointmentDate)
    {
        var from = plan.ValidFromDate.ToDateTime(TimeOnly.MinValue);
        var to = plan.ValidToDate.ToDateTime(TimeOnly.MaxValue);
        return appointmentDate >= from && appointmentDate <= to;
    }

    public async Task<List<Appointment>> GetActiveWaitingRoomAppointmentsAsync()
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var today = DateTime.Today;

        return await db.Appointments
            .Include(a => a.Service)
            .Where(a =>
                a.Status == AppointmentStatus.InProgress &&
                a.IsVisibleInWaitingRoom &&
                a.Date.Date == today)
            .OrderBy(a => a.Date)
            .ToListAsync();
    }


}
