using ConsulatTermine.Application.DTOs.Booking;
using ConsulatTermine.Application.Interfaces.Booking;
using ConsulatTermine.Domain.Entities;
using ConsulatTermine.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConsulatTermine.Infrastructure.Services.Booking
{
    public class SlotAvailabilityService : ISlotAvailabilityService
    {
        private readonly ApplicationDbContext _db;

        public SlotAvailabilityService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task ValidateSlotCapacitiesAsync(CreateBookingRequestDto request)
        {
            var allPersons = GetAllPersons(request);

            var grouped = allPersons
                .SelectMany(p => p.ServiceSlots.Select(s => new
                {
                    ServiceId = s.ServiceId,
                    Date = s.SlotTime.Date,
                    Time = s.SlotTime
                }))
                .GroupBy(x => new { x.ServiceId, x.Date });

            foreach (var group in grouped)
            {
                int serviceId = group.Key.ServiceId;
                DateTime date = group.Key.Date;

                // 1) Service laden (nur Basis + Employees für Kapazität)
                var service = await _db.Services
                    .Include(s => s.AssignedEmployees)
                    .FirstOrDefaultAsync(s => s.Id == serviceId);

                if (service == null)
                    throw new Exception($"Service {serviceId} not found.");

                // 2) Aktiven Plan laden
                var plan = await _db.WorkingSchedulePlans
                    .Where(p => p.ServiceId == serviceId && p.IsActive)
                    .OrderByDescending(p => p.ValidFromDate)
                    .FirstOrDefaultAsync();

                if (plan == null)
                    throw new Exception($"No active WorkingSchedulePlan found for service {serviceId}.");

                // 3) Datum muss im Plan-Zeitraum liegen
                var planFrom = plan.ValidFromDate.ToDateTime(TimeOnly.MinValue);
                var planTo = plan.ValidToDate.ToDateTime(TimeOnly.MaxValue);

                if (date < planFrom || date > planTo)
                    throw new Exception($"Requested date {date:yyyy-MM-dd} is outside active plan range for service {serviceId}.");

                // 4) Plan-bezogene WorkingHours / Overrides laden
                var workingHours = await _db.WorkingHours
                    .Where(w =>
                        w.ServiceId == serviceId &&
                        w.WorkingSchedulePlanId == plan.Id)
                    .ToListAsync();

                var overrides = await _db.ServiceDayOverrides
                    .Where(o =>
                        o.ServiceId == serviceId &&
                        o.WorkingSchedulePlanId == plan.Id)
                    .ToListAsync();

                // 5) Existierende Termine für diesen Tag laden
                var existingAppointments = await _db.Appointments
                    .Where(a => a.ServiceId == serviceId && a.Date.Date == date.Date)
                    .ToListAsync();

                // 6) Verfügbare Slots berechnen (NEUE Signatur)
                var freeSlotsDict = AppointmentCalculator.GetAvailableSlots(
                    service,
                    date,
                    workingHours,
                    overrides,
                    existingAppointments);

                // 7) Bedarf pro Slot zählen
                var requestedSlotsGrouped = group
                    .GroupBy(x => x.Time)
                    .ToList();

                foreach (var slotGroup in requestedSlotsGrouped)
                {
                    DateTime slotTime = slotGroup.Key;
                    var timeOfDay = slotTime.TimeOfDay;

                    var matchingKey = freeSlotsDict.Keys
                        .FirstOrDefault(k => k.Start == timeOfDay);

                    if (matchingKey.Start == default && matchingKey.End == default)
                    {
                        throw new Exception(
                            $"Requested slot {slotTime:yyyy-MM-dd HH:mm} is not a valid slot for service {serviceId}.");
                    }

                    int free = freeSlotsDict[matchingKey];
                    int needed = slotGroup.Count();

                    if (needed > free)
                    {
                        throw new Exception(
                            $"Not enough capacity for service {serviceId} at {slotTime:yyyy-MM-dd HH:mm}. " +
                            $"Requested: {needed}, Free: {free}");
                    }
                }
            }
        }

        public async Task<bool> IsSlotAvailableAsync(int serviceId, DateTime slotTime)
        {
            var date = slotTime.Date;
            var timeOfDay = slotTime.TimeOfDay;

            // 1) Service laden
            var service = await _db.Services
                .Include(s => s.AssignedEmployees)
                .FirstOrDefaultAsync(s => s.Id == serviceId);

            if (service == null)
                throw new Exception($"Service {serviceId} not found.");

            // 2) Aktiven Plan laden
            var plan = await _db.WorkingSchedulePlans
                .Where(p => p.ServiceId == serviceId && p.IsActive)
                .OrderByDescending(p => p.ValidFromDate)
                .FirstOrDefaultAsync();

            if (plan == null)
                throw new Exception($"No active WorkingSchedulePlan found for service {serviceId}.");

            // 3) Range Check
            var planFrom = plan.ValidFromDate.ToDateTime(TimeOnly.MinValue);
            var planTo = plan.ValidToDate.ToDateTime(TimeOnly.MaxValue);

            if (date < planFrom || date > planTo)
                return false;

            // 4) Plan-bezogene WorkingHours / Overrides laden
            var workingHours = await _db.WorkingHours
                .Where(w =>
                    w.ServiceId == serviceId &&
                    w.WorkingSchedulePlanId == plan.Id)
                .ToListAsync();

            var overrides = await _db.ServiceDayOverrides
                .Where(o =>
                    o.ServiceId == serviceId &&
                    o.WorkingSchedulePlanId == plan.Id)
                .ToListAsync();

            // 5) Existierende Appointments laden
            var existingAppointments = await _db.Appointments
                .Where(a => a.ServiceId == serviceId && a.Date.Date == date)
                .ToListAsync();

            // 6) Slots berechnen (NEUE Signatur)
            var freeSlotsDict = AppointmentCalculator.GetAvailableSlots(
                service,
                date,
                workingHours,
                overrides,
                existingAppointments);

            var matchingKey = freeSlotsDict.Keys
                .FirstOrDefault(k => k.Start == timeOfDay);

            if (matchingKey.Start == default && matchingKey.End == default)
                return false;

            return freeSlotsDict[matchingKey] > 0;
        }

        private static List<BookingPersonDto> GetAllPersons(CreateBookingRequestDto request)
        {
            var list = new List<BookingPersonDto> { request.MainPerson };
            list.AddRange(request.AccompanyingPersons);
            return list;
        }
    }
}
