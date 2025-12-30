using ConsulatTermine.Application.DTOs;
using ConsulatTermine.Application.Interfaces;
using ConsulatTermine.Domain.Entities;
using ConsulatTermine.Domain.Enums;
using ConsulatTermine.Infrastructure.Persistence;
using Infrastructure.SignalR;
using ConsulatTermine.Infrastructure.SignalR;
using Microsoft.AspNetCore.SignalR;

using Microsoft.EntityFrameworkCore;

namespace ConsulatTermine.Infrastructure.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<DisplayHub, IDisplayClient> _displayHub;
        private readonly IHubContext<EmployeeHub, IEmployeeClient> _employeeHub;

        private readonly IEmailService _emailService;
        private readonly IHubContext<WaitingRoomHub, IWaitingRoomClient> _waitingRoomHub;


       public AppointmentService(
    ApplicationDbContext context,
    IHubContext<DisplayHub, IDisplayClient> displayHub,
    IHubContext<EmployeeHub, IEmployeeClient> employeeHub,
    IHubContext<WaitingRoomHub, IWaitingRoomClient> waitingRoomHub,
    IEmailService emailService)
{
    _context = context;
    _displayHub = displayHub;
    _employeeHub = employeeHub;
    _waitingRoomHub = waitingRoomHub;
    _emailService = emailService;
}


        // -------------------------------------------------------------
        // FREIE SLOTS ALS DTOs (für UI)
        // -------------------------------------------------------------
        public async Task<List<AvailableSlotDto>> GetAvailableSlotDtosAsync(int serviceId, DateTime date)
        {
            // Service laden (Kapazität über AssignedEmployees)
            var service = await _context.Services
                .Include(s => s.AssignedEmployees)
                .FirstOrDefaultAsync(s => s.Id == serviceId);

            if (service == null)
                throw new Exception("Service not found.");

            // Aktiven Plan laden
            var plan = await GetActivePlanAsync(serviceId);
            if (plan == null)
                return new List<AvailableSlotDto>();

            // Datum muss im Plan-Zeitraum liegen (sonst KEINE Slots)
            if (!IsInsidePlan(plan, date))
                return new List<AvailableSlotDto>();

            // Plan-gebundene WorkingHours/Overrides laden
            var workingHours = await _context.WorkingHours
                .Where(w => w.ServiceId == serviceId && w.WorkingSchedulePlanId == plan.Id)
                .ToListAsync();

            var overrides = await _context.ServiceDayOverrides
                .Where(o => o.ServiceId == serviceId && o.WorkingSchedulePlanId == plan.Id)
                .ToListAsync();

            // Existierende Termine an dem Tag
            var appointments = await _context.Appointments
                .Where(a => a.ServiceId == serviceId && a.Date.Date == date.Date)
                .ToListAsync();

            // NEUE Signatur
            var slots = AppointmentCalculator.GetAvailableSlots(
                service,
                date,
                workingHours,
                overrides,
                appointments
            );

            // Mapping ins DTO
            return slots
                .Select(kv => new AvailableSlotDto
                {
                    SlotStart = date.Date + kv.Key.Start,
                    FreeCapacity = kv.Value
                })
                .OrderBy(x => x.SlotStart)
                .ToList();
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
            var date = slotStart.Date;

            // Freie Slots für diesen Tag holen (plan-basiert)
            var available = await GetAvailableSlotDtosAsync(serviceId, date);

            // Passenden Slot finden
            var slotDto = available.SingleOrDefault(s => s.SlotStart == slotStart);
            if (slotDto == null)
                throw new Exception("Invalid slot.");

            if (!slotDto.IsAvailable)
                throw new Exception("Slot is fully booked.");

            var appointment = new Appointment
            {
                FullName = fullName,
                Email = email,
                Date = slotStart,
                ServiceId = serviceId,
                Status = AppointmentStatus.Booked,
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return appointment;
        }
// -------------------------------------------------------------
// TERMIN STORNIEREN (THREAD-SAFE, EF-CORRECT)
// -------------------------------------------------------------
public async Task<bool> CancelAsync(int appointmentId)
{
    // 🔎 Termin inkl. Service laden
    var appointment = await _context.Appointments
        .Include(a => a.Service)
        .FirstOrDefaultAsync(a => a.Id == appointmentId);

    if (appointment == null)
        return false;

    if (appointment.Status == AppointmentStatus.Completed)
        return false;

    if (appointment.Status == AppointmentStatus.Cancelled)
        return true; // idempotent

    // ❌ Termin stornieren
    appointment.Status = AppointmentStatus.Cancelled;
    await _context.SaveChangesAsync();

    // ---------------------------------------------------------
    // 🔎 ALLE Termine dieser Buchung laden (GLEICHER THREAD)
    // ---------------------------------------------------------
    var allAppointments = await _context.Appointments
        .Include(a => a.Service)
        .Where(a => a.BookingReference == appointment.BookingReference)
        .ToListAsync();

    var mainPerson = allAppointments
        .FirstOrDefault(a => a.IsMainPerson && !string.IsNullOrWhiteSpace(a.Email));

    if (mainPerson == null)
        return true;

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
            await _emailService.SendPartialCancellationAsync(
                mainPerson.Email!,
                appointment.FullName,
                appointment.Service!.Name,
                appointment.Date);
        }
        else
        {
            // 🟥 VOLL-ABSAGE
            await _emailService.SendCancellationConfirmationAsync(
                mainPerson.Email!,
                mainPerson.FullName,
                appointment.BookingReference);
        }
    }
    catch (Exception ex)
    {
        // Mail-Fehler dürfen Cancel NICHT verhindern
        Console.WriteLine("EMAIL ERROR (ignored):");
        Console.WriteLine(ex);
    }

    return true;
}



        // -------------------------------------------------------------
        // TAGESLISTE FÜR EINEN SERVICE
        // -------------------------------------------------------------
        public async Task<List<Appointment>> GetAppointmentsByServiceAndDayAsync(int serviceId, DateTime date)
        {
            return await _context.Appointments
                .Where(a => a.ServiceId == serviceId && a.Date.Date == date.Date)
                .OrderBy(a => a.Date)
                .ToListAsync();
        }


        public async Task<List<Appointment>> GetByBookingReferenceAsync(string bookingReference)
{
    if (string.IsNullOrWhiteSpace(bookingReference))
        return new List<Appointment>();

    return await _context.Appointments
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
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null)
                return false;

            if (appointment.Status != AppointmentStatus.Booked)
                return false;

            appointment.Status = AppointmentStatus.CheckedIn;
            appointment.CheckedInAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _employeeHub.Clients.All.StatusUpdated(appointment.Id, appointment.Status);

            return true;
        }

        // -------------------------------------------------------------
        // NÄCHSTER WARTENDER BÜRGER (für Mitarbeiter-UI)
        // -------------------------------------------------------------
        public async Task<Appointment?> GetNextAsync(int serviceId)
        {
            return await _context.Appointments
                .Where(a => a.ServiceId == serviceId && a.Status == AppointmentStatus.CheckedIn)
                .OrderBy(a => a.CheckedInAt ?? a.Date)
                .FirstOrDefaultAsync();
        }

        // -------------------------------------------------------------
        // BEARBEITUNG STARTEN (Mitarbeiter nimmt Bürger dran)
        // -------------------------------------------------------------
public async Task<bool> StartProcessingAsync(int appointmentId, int employeeId)
{
    var appointment = await _context.Appointments
        .Include(a => a.Service)
        .FirstOrDefaultAsync(a => a.Id == appointmentId);

    if (appointment == null)
        return false;

    // Version 1: Alle Booked-Termine gelten als wartend
    if (appointment.Status != AppointmentStatus.Booked)
        return false;

    appointment.Status = AppointmentStatus.InProgress;
    appointment.CurrentEmployeeId = employeeId;
    appointment.IsVisibleInWaitingRoom = true;

    await _context.SaveChangesAsync();
await _waitingRoomHub.Clients.All.CallStarted();

    // Bestehende Echtzeit-Updates (falls vorhanden)
    await _employeeHub.Clients.All.StatusUpdated(
        appointment.Id,
        appointment.Status
    );

    return true;
}

public async Task RecallAsync(int appointmentId, int employeeId)
{
    // Kein Statuswechsel, kein DB-Zugriff nötig
    await _waitingRoomHub.Clients.All.Recall();
}






        // -------------------------------------------------------------
        // TERMIN ABSCHLIESSEN
        // -------------------------------------------------------------
public async Task<bool> CompleteAsync(int appointmentId, int employeeId)
{
    var appointment = await _context.Appointments
        .FirstOrDefaultAsync(a => a.Id == appointmentId);

    if (appointment == null)
        return false;

    // Nur der eigene InProgress-Termin darf beendet werden
    if (appointment.Status != AppointmentStatus.InProgress)
        return false;

    if (appointment.CurrentEmployeeId != employeeId)
        return false;

    appointment.Status = AppointmentStatus.Completed;
    appointment.CompletedAt = DateTime.UtcNow;
    appointment.CurrentEmployeeId = null;

    await _context.SaveChangesAsync();

    await _employeeHub.Clients.All.StatusUpdated(
        appointment.Id,
        appointment.Status
    );

    return true;
}

public async Task<bool> HideFromWaitingRoomAsync(int appointmentId, int employeeId)
{
    var appointment = await _context.Appointments
        .FirstOrDefaultAsync(a => a.Id == appointmentId);

    if (appointment == null)
        return false;

    // Nur der Mitarbeiter, der den Termin hat, darf ihn ausblenden
    if (appointment.Status != AppointmentStatus.InProgress)
        return false;

    if (appointment.CurrentEmployeeId != employeeId)
        return false;

   appointment.IsVisibleInWaitingRoom = false;

await _context.SaveChangesAsync();

// ❌ aus TV entfernen
await _waitingRoomHub.Clients.All.Hide();

return true;

}


        // -------------------------------------------------------------
        // GRUPPENBUCHUNG (1–5 Personen, mehrere Slots möglich)
        // -------------------------------------------------------------
        public async Task<List<Appointment>> BookGroupAsync(GroupBookingDto dto)
        {
            var result = new List<Appointment>();

            // Service laden (Kapazität über AssignedEmployees)
            var service = await _context.Services
                .Include(s => s.AssignedEmployees)
                .FirstOrDefaultAsync(s => s.Id == dto.ServiceId);

            if (service == null)
                throw new Exception("Service not found.");

            // Gruppengröße prüfen
            if (dto.TotalPersons < 1 || dto.TotalPersons > 5)
                throw new Exception("Gruppengröße muss zwischen 1 und 5 liegen.");

            if (dto.Persons.Count != dto.TotalPersons)
                throw new Exception("Anzahl der Personen stimmt nicht mit TotalPersons überein.");

            var allSlotStarts = dto.Persons.Select(p => p.SlotStart).ToList();

            // Alle Slots müssen am selben Tag liegen
            var date = allSlotStarts.First().Date;
            if (allSlotStarts.Any(s => s.Date != date.Date))
                throw new Exception("Alle Slots müssen am selben Tag liegen.");

            // Aktiven Plan laden
            var plan = await GetActivePlanAsync(dto.ServiceId);
            if (plan == null)
                throw new Exception($"Kein aktiver Dienstplan für Service {dto.ServiceId} gefunden.");

            // Datum muss im Plan-Zeitraum liegen
            if (!IsInsidePlan(plan, date))
                throw new Exception("Datum liegt außerhalb des aktiven Plan-Zeitraums.");

            // Plan-gebundene WorkingHours/Overrides laden
            var workingHours = await _context.WorkingHours
                .Where(w => w.ServiceId == dto.ServiceId && w.WorkingSchedulePlanId == plan.Id)
                .ToListAsync();

            var overrides = await _context.ServiceDayOverrides
                .Where(o => o.ServiceId == dto.ServiceId && o.WorkingSchedulePlanId == plan.Id)
                .ToListAsync();

            // Existierende Termine für diesen Tag
            var existing = await _context.Appointments
                .Where(a => a.ServiceId == dto.ServiceId && a.Date.Date == date.Date)
                .ToListAsync();

            // Slots berechnen (NEUE Signatur)
            var freeSlots = AppointmentCalculator.GetAvailableSlots(
                service,
                date,
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
                    throw new Exception($"Ungültiger Slot: {slotStart}");

                int free = freeSlots[slot];
                int needed = slotGroup.Count();

                if (needed > free)
                    throw new Exception($"Slot {slotStart} hat nicht genug freie Plätze. Frei: {free}, benötigt: {needed}");
            }

            // Speichern in Transaktion
            using var trx = await _context.Database.BeginTransactionAsync();

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

                    _context.Appointments.Add(appointment);
                    result.Add(appointment);
                }

                await _context.SaveChangesAsync();
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
    DateTime date)
{
    var targetDate = date.Date;

    return await _context.Appointments
        .Where(a =>
            a.ServiceId == serviceId &&
            a.Date.Date == targetDate)
        .OrderBy(a => a.Date) // Uhrzeit
        .ToListAsync();
}

public async Task<Appointment?> GetNextAppointmentForServiceOnDateAsync(
    int serviceId,
    DateTime date)
{
    var targetDate = date.Date;

    return await _context.Appointments
        .Where(a =>
            a.ServiceId == serviceId &&
            a.Date.Date == targetDate)
        .OrderBy(a => a.Date)
        .FirstOrDefaultAsync();
}

        // -------------------------------------------------------------
        // Helpers: Active Plan + Range Check
        // -------------------------------------------------------------
        private async Task<WorkingSchedulePlan?> GetActivePlanAsync(int serviceId)
        {
            return await _context.WorkingSchedulePlans
                .Where(p => p.ServiceId == serviceId && p.IsActive)
                .OrderByDescending(p => p.ValidFromDate)
                .FirstOrDefaultAsync();
        }

        private static bool IsInsidePlan(WorkingSchedulePlan plan, DateTime date)
        {
            var from = plan.ValidFromDate.ToDateTime(TimeOnly.MinValue);
            var to = plan.ValidToDate.ToDateTime(TimeOnly.MaxValue);
            return date >= from && date <= to;
        }
    }
}
