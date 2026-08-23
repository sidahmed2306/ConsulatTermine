using ConsulatTermine.Application.DTOs.Booking;
using ConsulatTermine.Application.Exceptions;
using ConsulatTermine.Application.Interfaces.Booking;
using ConsulatTermine.Application.Services;
using ConsulatTermine.Domain.Enums;
using ConsulatTermine.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConsulatTermine.Infrastructure.Services.Booking;

public class SlotAvailabilityService : ISlotAvailabilityService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public SlotAvailabilityService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task ValidateSlotCapacitiesAsync(CreateBookingRequestDto request)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
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
            var service = await db.Services
                .Include(s => s.AssignedEmployees)
                .FirstOrDefaultAsync(s => s.Id == serviceId);

            if (service == null)
            {
                throw new BusinessRuleViolationException("Der Service wurde nicht gefunden.");
            }

            // 2) Aktiven Plan laden
            var plan = await db.WorkingSchedulePlans
                .Where(p => p.ServiceId == serviceId && p.IsActive)
                .OrderByDescending(p => p.ValidFromDate)
                .FirstOrDefaultAsync();

            if (plan == null)
            {
                throw new BusinessRuleViolationException("Für diesen Service ist derzeit kein Arbeitszeitplan hinterlegt.");
            }

            // 3) Datum muss im Plan-Zeitraum liegen
            var planFrom = plan.ValidFromDate.ToDateTime(TimeOnly.MinValue);
            var planTo = plan.ValidToDate.ToDateTime(TimeOnly.MaxValue);

            if (date < planFrom || date > planTo)
            {
                throw new BusinessRuleViolationException($"Für den {date:dd.MM.yyyy} können keine Termine vergeben werden.");
            }

            // 4) Plan-bezogene WorkingHours / Overrides laden
            var workingHours = await db.WorkingHours
                .Where(w =>
                    w.ServiceId == serviceId &&
                    w.WorkingSchedulePlanId == plan.Id)
                .ToListAsync();

            var overrides = await db.ServiceDayOverrides
                .Where(o =>
                    o.ServiceId == serviceId &&
                    o.WorkingSchedulePlanId == plan.Id)
                .ToListAsync();

            // 5) Existierende Termine für diesen Tag laden
            var existingAppointments = await db.Appointments
.Where(a =>
    a.ServiceId == serviceId &&
    a.Date.Date == date.Date &&
    a.Status == AppointmentStatus.Booked)
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

            var now = DateTime.Now;
            var minSlotTime = now.AddMinutes(30);

            foreach (var slotGroup in requestedSlotsGrouped)
            {
                DateTime slotTime = slotGroup.Key;
                var localSlot = slotTime.Kind == DateTimeKind.Utc ? slotTime.ToLocalTime() : slotTime;

                // Regel wie in der UI: Slot muss mindestens „heute + 30 Min“ sein (nicht in der Vergangenheit / zu nah an „jetzt“)
                if (localSlot < minSlotTime)
                {
                    throw new BusinessRuleViolationException(
                        "Der gewählte Termin liegt in der Vergangenheit oder in weniger als 30 Minuten. " +
                        "Bitte wählen Sie einen anderen Slot.");
                }

                // Zeit auf Slot-Granularität runden (z. B. 30 Min), damit kleine Abweichungen nicht zu „not a valid slot“ führen
                var timeOfDay = localSlot.TimeOfDay;
                var stepMinutes = service.SlotDurationMinutes;
                var totalMinutes = (int)timeOfDay.TotalMinutes;
                var roundedMinutes = (int)Math.Round((double)totalMinutes / stepMinutes) * stepMinutes;
                var timeOfDayRounded = TimeSpan.FromMinutes(roundedMinutes);

                // Slot-Grenzen aus der DB können minimale Abweichungen haben (z. B. 09:00:00.001) – beim Matching ebenfalls runden, damit der erste Slot (09:00) trifft
                var matchingKey = freeSlotsDict.Keys
                    .FirstOrDefault(k =>
                    {
                        var startRounded = RoundTimeToSlotMinutes(k.Start, stepMinutes);
                        var endRounded = RoundTimeToSlotMinutes(k.End, stepMinutes);
                        return timeOfDayRounded >= startRounded && timeOfDayRounded < endRounded;
                    });

                if (matchingKey.Start == default && matchingKey.End == default)
                {
                    throw new BusinessRuleViolationException(
                        $"Requested slot {slotTime:yyyy-MM-dd HH:mm} is not a valid slot for service {serviceId}.");
                }

                int free = freeSlotsDict[matchingKey];
                int needed = slotGroup.Count();

                if (needed > free)
                {
                    throw new BusinessRuleViolationException(
                        $"Not enough capacity for service {serviceId} at {slotTime:yyyy-MM-dd HH:mm}. " +
                        $"Requested: {needed}, Free: {free}");
                }
            }
        }
    }

    public async Task<bool> IsSlotAvailableAsync(int serviceId, DateTime slotTime)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var localSlot = slotTime.Kind == DateTimeKind.Utc ? slotTime.ToLocalTime() : slotTime;
        var date = localSlot.Date;
        var timeOfDay = localSlot.TimeOfDay;

        // 1) Service laden
        var service = await db.Services
            .Include(s => s.AssignedEmployees)
            .FirstOrDefaultAsync(s => s.Id == serviceId);

        if (service == null)
        {
            throw new BusinessRuleViolationException("Der Service wurde nicht gefunden.");
        }

        // 2) Aktiven Plan laden
        var plan = await db.WorkingSchedulePlans
            .Where(p => p.ServiceId == serviceId && p.IsActive)
            .OrderByDescending(p => p.ValidFromDate)
            .FirstOrDefaultAsync();

        if (plan == null)
        {
            throw new BusinessRuleViolationException("Für diesen Service ist derzeit kein Arbeitszeitplan hinterlegt.");
        }

        // 3) Range Check
        var planFrom = plan.ValidFromDate.ToDateTime(TimeOnly.MinValue);
        var planTo = plan.ValidToDate.ToDateTime(TimeOnly.MaxValue);

        if (date < planFrom || date > planTo)
        {
            return false;
        }

        // 4) Plan-bezogene WorkingHours / Overrides laden
        var workingHours = await db.WorkingHours
            .Where(w =>
                w.ServiceId == serviceId &&
                w.WorkingSchedulePlanId == plan.Id)
            .ToListAsync();

        var overrides = await db.ServiceDayOverrides
            .Where(o =>
                o.ServiceId == serviceId &&
                o.WorkingSchedulePlanId == plan.Id)
            .ToListAsync();

        // 5) Existierende Appointments laden
        var existingAppointments = await db.Appointments
.Where(a =>
    a.ServiceId == serviceId &&
    a.Date.Date == date &&
    a.Status == AppointmentStatus.Booked)
.ToListAsync();


        // 6) Slots berechnen (NEUE Signatur)
        var freeSlotsDict = AppointmentCalculator.GetAvailableSlots(
            service,
            date,
            workingHours,
            overrides,
            existingAppointments);

        var stepMinutes = service.SlotDurationMinutes;
        var totalMinutes = (int)timeOfDay.TotalMinutes;
        var timeOfDayRounded = TimeSpan.FromMinutes((int)Math.Round((double)totalMinutes / stepMinutes) * stepMinutes);

        var matchingKey = freeSlotsDict.Keys
            .FirstOrDefault(k =>
            {
                var startRounded = RoundTimeToSlotMinutes(k.Start, stepMinutes);
                var endRounded = RoundTimeToSlotMinutes(k.End, stepMinutes);
                return timeOfDayRounded >= startRounded && timeOfDayRounded < endRounded;
            });

        if (matchingKey.Start == default && matchingKey.End == default)
        {
            return false;
        }

        return freeSlotsDict[matchingKey] > 0;
    }

    private static TimeSpan RoundTimeToSlotMinutes(TimeSpan time, int stepMinutes)
    {
        var totalMinutes = (int)time.TotalMinutes;
        var rounded = (int)Math.Round((double)totalMinutes / stepMinutes) * stepMinutes;
        if (rounded < 0)
        {
            rounded = 0;
        }

        return TimeSpan.FromMinutes(rounded);
    }

    private static List<BookingPersonDto> GetAllPersons(CreateBookingRequestDto request)
    {
        var list = new List<BookingPersonDto> { request.MainPerson };
        list.AddRange(request.AccompanyingPersons);
        return list;
    }

}
