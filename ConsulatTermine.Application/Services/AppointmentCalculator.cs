using ConsulatTermine.Domain.Entities;

namespace ConsulatTermine.Application.Services;

public static class AppointmentCalculator
{
    /// <summary>
    /// Berechnet alle theoretischen Slots (Zeiträume) für ein bestimmtes Datum.
    /// WorkingHours + DayOverrides + SlotDuration.
    /// </summary>
    public static List<(TimeSpan Start, TimeSpan End)> GetDailySlots(Service service, DateTime date)
    {
        var slots = new List<(TimeSpan Start, TimeSpan End)>();

        // Override prüfen
        var overrideDay = service.DayOverrides
            .FirstOrDefault(d => d.Date.Date == date.Date);

        TimeSpan start;
        TimeSpan end;

        if (overrideDay != null)
        {
            if (overrideDay.IsClosed)
                return slots;

            start = overrideDay.StartTime ?? TimeSpan.Zero;
            end   = overrideDay.EndTime   ?? TimeSpan.Zero;
        }
        else
        {
            // Regulärer Tag
            var work = service.WorkingHours
                .FirstOrDefault(w => w.Day == date.DayOfWeek);

            if (work == null)
                return slots;

            start = work.StartTime;
            end   = work.EndTime;
        }

        // Slot-Erzeugung
        var duration = TimeSpan.FromMinutes(service.SlotDurationMinutes);
        var current = start;

        while (current + duration <= end)
        {
            slots.Add((current, current + duration));
            current += duration;
        }

        return slots;
    }


    /// <summary>
    /// Berechnet die maximale Kapazität pro Slot basierend auf:
    /// CapacityPerSlot * Anzahl Mitarbeiter
    /// </summary>
    public static int GetEffectiveCapacity(Service service)
    {
        int employeeCount = service.AssignedEmployees?.Count ?? 0;

        // Falls keine Mitarbeiter eingeteilt sind → keine Termine möglich
        if (employeeCount == 0)
            return 0;

        return employeeCount * service.CapacityPerSlot;
    }


    /// <summary>
    /// Ermittelt für jeden Slot, wie viele Plätze noch frei sind.
    /// </summary>
    public static Dictionary<(TimeSpan Start, TimeSpan End), int> GetAvailableSlots(
        Service service,
        DateTime date,
        List<Appointment> existingAppointments)
    {
        var slots = GetDailySlots(service, date);
        var effectiveCapacity = GetEffectiveCapacity(service);

        var result = new Dictionary<(TimeSpan Start, TimeSpan End), int>();

        foreach (var slot in slots)
        {
            // Anzahl der bereits gebuchten Termine in diesem Slot
            int booked = existingAppointments.Count(a =>
                a.Date.Date == date.Date &&
                a.Date.TimeOfDay >= slot.Start &&
                a.Date.TimeOfDay < slot.End &&
                a.ServiceId == service.Id);

            int free = Math.Max(0, effectiveCapacity - booked);

            result.Add(slot, free);
        }

        return result;
    }
}
