using ConsulatTermine.Domain.Entities;

public static class AppointmentCalculator
{
    /// <summary>
    /// Berechnet alle theoretischen Zeit-Slots für ein bestimmtes Datum
    /// anhand von WorkingHours, Overrides und SlotDuration.
    /// </summary>
    public static List<(TimeSpan Start, TimeSpan End)> GetDailySlots(Service service, DateTime date)
    {
        var slots = new List<(TimeSpan Start, TimeSpan End)>();

        var overrideDay = service.DayOverrides?
            .FirstOrDefault(d => d.Date.Date == date.Date);

        TimeSpan start;
        TimeSpan end;

        if (overrideDay != null)
        {
            if (overrideDay.IsClosed)
                return slots;

            if (!overrideDay.StartTime.HasValue || !overrideDay.EndTime.HasValue)
                return slots;

            start = overrideDay.StartTime.Value;
            end   = overrideDay.EndTime.Value;
        }
        else
        {
            var work = service.WorkingHours?
                .FirstOrDefault(w => w.Day == date.DayOfWeek);

            if (work == null)
                return slots;

            start = work.StartTime;
            end   = work.EndTime;
        }

        if (start >= end)
            return slots;

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
    /// Effektive Kapazität = Anzahl der Mitarbeitenden
    /// </summary>
    public static int GetEffectiveCapacity(Service service)
    {
        return service.AssignedEmployees?.Count ?? 0;
    }

    /// <summary>
    /// Berechnet verfügbare Slots (Zeitfenster + freie Mitarbeiter)
    /// </summary>
    public static Dictionary<(TimeSpan Start, TimeSpan End), int> GetAvailableSlots(
        Service service,
        DateTime date,
        List<Appointment> existingAppointments)
    {
        var slots = GetDailySlots(service, date);
        var result = new Dictionary<(TimeSpan Start, TimeSpan End), int>();

        if (slots.Count == 0)
            return result;

        int effectiveCapacity = GetEffectiveCapacity(service);

        if (effectiveCapacity == 0)
        {
            foreach (var slot in slots)
                result[slot] = 0;

            return result;
        }

        foreach (var slot in slots)
        {
            int booked = existingAppointments.Count(a =>
                a.ServiceId == service.Id &&
                a.Date.Date == date.Date &&
                a.Date.TimeOfDay >= slot.Start &&
                a.Date.TimeOfDay < slot.End);

            result[slot] = Math.Max(0, effectiveCapacity - booked);
        }

        return result;
    }
}
