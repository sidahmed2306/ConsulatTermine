using ConsulatTermine.Application.DTOs;
using ConsulatTermine.Application.Interfaces;
using ConsulatTermine.Domain.Entities;
using ConsulatTermine.Domain.Enums;
using ConsulatTermine.Infrastructure.Persistence;
using Infrastructure.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;


namespace ConsulatTermine.Infrastructure.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _context;

        private readonly IHubContext<DisplayHub, IDisplayClient> _displayHub;
        private readonly IHubContext<EmployeeHub, IEmployeeClient> _employeeHub;



        public AppointmentService(
            ApplicationDbContext context,
            IHubContext<DisplayHub, IDisplayClient> displayHub,
            IHubContext<EmployeeHub, IEmployeeClient> employeeHub)
        {
            _context = context;
            _displayHub = displayHub;
            _employeeHub = employeeHub;
        }

        // -------------------------------------------------------------
        // FREIE SLOTS ALS DTOs (für UI)
        // -------------------------------------------------------------
        public async Task<List<AvailableSlotDto>> GetAvailableSlotDtosAsync(
            int serviceId,
            DateTime date)
        {
            // Service inkl. aller relevanten Daten laden
            var service = await _context.Services
                .Include(s => s.WorkingHours)
                .Include(s => s.DayOverrides)
                .Include(s => s.AssignedEmployees)
                    .ThenInclude(a => a.Employee)
                .FirstOrDefaultAsync(s => s.Id == serviceId);

            if (service == null)
                throw new Exception("Service not found.");

            // Alle Termine dieses Tages für diesen Service
            var appointments = await _context.Appointments
                .Where(a => a.ServiceId == serviceId &&
                            a.Date.Date == date.Date)
                .ToListAsync();

            // Dictionary: (Start, End) -> freie Plätze
            var dict = AppointmentCalculator.GetAvailableSlots(service, date, appointments);

            // In DTOs umwandeln
            var result = new List<AvailableSlotDto>();

            foreach (var kv in dict)
            {
                var slot = kv.Key;
                int free = kv.Value;

                result.Add(new AvailableSlotDto
                {
                    SlotStart = date.Date + slot.Start,
                    FreeCapacity = free
                });
            }

            return result
                .OrderBy(r => r.SlotStart)
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

            // Freie Slots für diesen Tag holen
            var available = await GetAvailableSlotDtosAsync(serviceId, date);

            // Passenden Slot finden
            var slotDto = available
                .SingleOrDefault(s => s.SlotStart == slotStart);

            if (slotDto == null)
                throw new Exception("Invalid slot.");

            if (!slotDto.IsAvailable)
                throw new Exception("Slot is fully booked.");

            // Termin erzeugen
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
        // TERMIN STORNIEREN
        // -------------------------------------------------------------
        public async Task<bool> CancelAsync(int appointmentId)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null)
                return false;

            if (appointment.Status == AppointmentStatus.Completed)
                return false; // fertig bearbeitete Termine nicht mehr stornieren

            appointment.Status = AppointmentStatus.Cancelled;
            await _context.SaveChangesAsync();

            return true;
        }

        // -------------------------------------------------------------
        // TAGESLISTE FÜR EINEN SERVICE
        // -------------------------------------------------------------
        public async Task<List<Appointment>> GetAppointmentsByServiceAndDayAsync(
            int serviceId,
            DateTime date)
        {
            return await _context.Appointments
                .Where(a => a.ServiceId == serviceId &&
                            a.Date.Date == date.Date)
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
                .Where(a => a.ServiceId == serviceId &&
                            a.Status == AppointmentStatus.CheckedIn)
                .OrderBy(a => a.CheckedInAt ?? a.Date)
                .FirstOrDefaultAsync();
        }

        // -------------------------------------------------------------
        // BEARBEITUNG STARTEN (Mitarbeiter nimmt Bürger dran)
        // -------------------------------------------------------------
        public async Task<bool> StartProcessingAsync(int appointmentId)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null)
                return false;

            if (appointment.Status != AppointmentStatus.CheckedIn)
                return false; // nur direkt aus der Warteschlange

            appointment.Status = AppointmentStatus.InProgress;

            await _context.SaveChangesAsync();
            await _displayHub.Clients.All.CitizenCalled(
            appointment.Id,
            appointment.FullName,    // oder Ticketnummer, später UI-Entscheidung
            appointment.Service?.Name ?? "",
            "" // counterName kommt später, wenn Mitarbeiter-Workplaces gebaut sind
);

            await _employeeHub.Clients.All.StatusUpdated(appointment.Id, appointment.Status);

            return true;
        }

        // -------------------------------------------------------------
        // TERMIN ABSCHLIESSEN
        // -------------------------------------------------------------
        public async Task<bool> CompleteAsync(int appointmentId)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null)
                return false;

            if (appointment.Status != AppointmentStatus.InProgress)
                return false;

            appointment.Status = AppointmentStatus.Completed;
            appointment.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _employeeHub.Clients.All.StatusUpdated(appointment.Id, appointment.Status);

            return true;
        }

        // -------------------------------------------------------------
// GRUPPENBUCHUNG (1–5 Personen, mehrere Slots möglich)
// -------------------------------------------------------------
public async Task<List<Appointment>> BookGroupAsync(GroupBookingDto dto)
{
    var result = new List<Appointment>();

    // Service laden (inkl. Kapazitätsdaten)
    var service = await _context.Services
        .Include(s => s.WorkingHours)
        .Include(s => s.DayOverrides)
        .Include(s => s.AssignedEmployees)
        .FirstOrDefaultAsync(s => s.Id == dto.ServiceId);

    if (service == null)
        throw new Exception("Service not found.");

    // Gruppengröße prüfen
    if (dto.TotalPersons < 1 || dto.TotalPersons > 5)
        throw new Exception("Gruppengröße muss zwischen 1 und 5 liegen.");

    if (dto.Persons.Count != dto.TotalPersons)
        throw new Exception("Anzahl der Personen stimmt nicht mit TotalPersons überein.");

    // Alle Slots aus GroupBookingDto extrahieren
    var allSlotStarts = dto.Persons.Select(p => p.SlotStart).ToList();

    // Datumsvalidierung: alle Slots müssen am gleichen Tag liegen
    var date = allSlotStarts.First().Date;
    if (allSlotStarts.Any(s => s.Date != date))
        throw new Exception("Alle Slots müssen am selben Tag liegen.");

    // Alle existierenden Termine für diesen Tag laden
    var existing = await _context.Appointments
        .Where(a => a.ServiceId == dto.ServiceId && a.Date.Date == date.Date)
        .ToListAsync();

    // freie Kapazität pro Slot berechnen
    var freeSlots = AppointmentCalculator.GetAvailableSlots(service, date, existing);
    // freeSlots: Dictionary<(start,end), capacity>

    // 1. Validierung: jeder Slot hat genug Kapazität
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

    // 2. Speichern in Transaktion
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

    }
}
