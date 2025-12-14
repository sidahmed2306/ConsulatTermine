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

        /// <summary>
        /// Prüft für alle in der Buchung enthaltenen Slots,
        /// ob ausreichend Kapazität vorhanden ist.
        /// 
        /// Wir gehen pro Service + Datum vor:
        /// - Service inkl. WorkingHours, DayOverrides, AssignedEmployees laden
        /// - alle bestehenden Appointments an diesem Datum für diesen Service holen
        /// - über AppointmentCalculator.GetAvailableSlots(...) die freien Plätze ermitteln
        /// - pro angefragtem Slot prüfen, ob "needed <= free"
        /// </summary>
        public async Task ValidateSlotCapacitiesAsync(CreateBookingRequestDto request)
        {
            var allPersons = GetAllPersons(request);

            // Alle Slots der Buchung zusammenfassen:
            // Key = (ServiceId, DateOnly), Value = Liste der SlotTimes (DateTime)
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

                // Service inkl. relevanter Daten laden
                var service = await _db.Services
                    .Include(s => s.WorkingHours)
                    .Include(s => s.DayOverrides)
                    .Include(s => s.AssignedEmployees)
                    .FirstOrDefaultAsync(s => s.Id == serviceId);

                if (service == null)
                    throw new Exception($"Service {serviceId} not found.");

                // Alle existierenden Termine für diesen Service + Tag laden
                var existingAppointments = await _db.Appointments
                    .Where(a => a.ServiceId == serviceId && a.Date.Date == date.Date)
                    .ToListAsync();

                // Freie Kapazitäten je Slot berechnen
                var freeSlotsDict = AppointmentCalculator.GetAvailableSlots(
                    service,
                    date,
                    existingAppointments);

                // Nun den Bedarf pro Slot zählen (wie viele Personen wollen genau diesen Slot)
                var requestedSlotsGrouped = group
                    .GroupBy(x => x.Time)
                    .ToList();

                foreach (var slotGroup in requestedSlotsGrouped)
                {
                    DateTime slotTime = slotGroup.Key;
                    TimeSpan startTimeOfDay = slotTime.TimeOfDay;

                    // passenden Eintrag aus dem Dictionary finden
                    var matchingKey = freeSlotsDict.Keys
                        .FirstOrDefault(k => k.Start == startTimeOfDay);

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

        /// <summary>
        /// Einzelnabfrage: ist dieser Slot für diesen Service noch mindestens 1x verfügbar?
        /// </summary>
        public async Task<bool> IsSlotAvailableAsync(int serviceId, DateTime slotTime)
        {
            var date = slotTime.Date;
            var timeOfDay = slotTime.TimeOfDay;

            var service = await _db.Services
                .Include(s => s.WorkingHours)
                .Include(s => s.DayOverrides)
                .Include(s => s.AssignedEmployees)
                .FirstOrDefaultAsync(s => s.Id == serviceId);

            if (service == null)
                throw new Exception($"Service {serviceId} not found.");

            var existingAppointments = await _db.Appointments
                .Where(a => a.ServiceId == serviceId && a.Date.Date == date)
                .ToListAsync();

            var freeSlotsDict = AppointmentCalculator.GetAvailableSlots(
                service,
                date,
                existingAppointments);

            var matchingKey = freeSlotsDict.Keys
                .FirstOrDefault(k => k.Start == timeOfDay);

            if (matchingKey.Start == default && matchingKey.End == default)
            {
                // Slot gehört nicht zu diesem Tag/Service (z. B. außerhalb der Öffnungszeiten)
                return false;
            }

            int free = freeSlotsDict[matchingKey];
            return free > 0;
        }

        // ------------------------------------------------------------
        // Helper
        // ------------------------------------------------------------
        private static List<BookingPersonDto> GetAllPersons(CreateBookingRequestDto request)
        {
            var list = new List<BookingPersonDto> { request.MainPerson };
            list.AddRange(request.AccompanyingPersons);
            return list;
        }
    }
}
