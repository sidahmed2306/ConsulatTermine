using ConsulatTermine.Domain.Entities;

public static class AppointmentCalculator
{
    // ============================================================
    // 1) TÄGLICHE SLOT-BERECHNUNG (BLEIBT WIE BEI DIR)
    // ============================================================
    public static List<(TimeSpan Start, TimeSpan End)> GetDailySlots(
        Service service,
        DateTime date,
        List<WorkingHours> workingHours,
        List<ServiceDayOverride> overrides)
    {
        var slots = new List<(TimeSpan Start, TimeSpan End)>();

        // 1️⃣ Datums-spezifische Override (höchste Priorität)
        var dateOverride = overrides
            .Where(o => !o.IsWeeklyOverride)
            .FirstOrDefault(o => o.Date.Date == date.Date);

        if (dateOverride != null)
        {
            if (dateOverride.IsClosed)
                return slots;

            if (!dateOverride.StartTime.HasValue || !dateOverride.EndTime.HasValue)
                return slots;

            return BuildSlots(
                dateOverride.StartTime.Value,
                dateOverride.EndTime.Value,
                service.SlotDurationMinutes
            );
        }

        // 2️⃣ Wöchentliche Override
        var weeklyOverride = overrides
            .Where(o => o.IsWeeklyOverride)
            .FirstOrDefault(o => o.WeeklyDay == date.DayOfWeek);

        if (weeklyOverride != null)
        {
            if (weeklyOverride.IsClosed)
                return slots;

            if (!weeklyOverride.StartTime.HasValue || !weeklyOverride.EndTime.HasValue)
                return slots;

            return BuildSlots(
                weeklyOverride.StartTime.Value,
                weeklyOverride.EndTime.Value,
                service.SlotDurationMinutes
            );
        }

        // 3️⃣ Reguläre Öffnungszeiten
        var work = workingHours.FirstOrDefault(w => w.Day == date.DayOfWeek);
        if (work == null)
            return slots;

        return BuildSlots(
            work.StartTime,
            work.EndTime,
            service.SlotDurationMinutes
        );
    }

    private static List<(TimeSpan Start, TimeSpan End)> BuildSlots(
        TimeSpan start,
        TimeSpan end,
        int slotDurationMinutes)
    {
        var slots = new List<(TimeSpan Start, TimeSpan End)>();

        if (start >= end)
            return slots;

        var duration = TimeSpan.FromMinutes(slotDurationMinutes);
        var current = start;

        while (current + duration <= end)
        {
            slots.Add((current, current + duration));
            current += duration;
        }

        return slots;
    }

    // ============================================================
    // 2) KAPAZITÄT (BLEIBT WIE BEI DIR)
    // ============================================================
    private static int GetEffectiveCapacity(Service service)
    {
        return service.AssignedEmployees?.Count ?? 0;
    }

    public static Dictionary<(TimeSpan Start, TimeSpan End), int> GetAvailableSlots(
        Service service,
        DateTime date,
        List<WorkingHours> workingHours,
        List<ServiceDayOverride> overrides,
        List<Appointment> existingAppointments)
    {
        var slots = GetDailySlots(service, date, workingHours, overrides);
        var result = new Dictionary<(TimeSpan Start, TimeSpan End), int>();

        if (!slots.Any())
            return result;

        int capacity = GetEffectiveCapacity(service);

        foreach (var slot in slots)
        {
            int booked = existingAppointments.Count(a =>
                a.ServiceId == service.Id &&
                a.Date.Date == date.Date &&
                a.Date.TimeOfDay >= slot.Start &&
                a.Date.TimeOfDay < slot.End);

            result[slot] = Math.Max(0, capacity - booked);
        }

        return result;
    }

    // ============================================================
    // 3) ⭐ NEU ⭐ ZEIT-KOMPATIBILITÄT ZWISCHEN SERVICES
    // ============================================================
    public static bool IsSlotTimeCompatible(
        DateTime candidateStart,
        int candidateDurationMinutes,
        IEnumerable<(DateTime Start, int DurationMinutes)> otherSelectedSlots,
        TimeSpan buffer)
    {
        var candidateEnd = candidateStart.AddMinutes(candidateDurationMinutes);

        foreach (var other in otherSelectedSlots)
        {
            var otherStart = other.Start;
            var otherEnd = other.Start.AddMinutes(other.DurationMinutes);

            // Kandidat liegt VOR dem anderen Slot
            bool endsBefore =
                candidateEnd.Add(buffer) <= otherStart;

            // Kandidat liegt NACH dem anderen Slot
            bool startsAfter =
                candidateStart >= otherEnd.Add(buffer);

            // Wenn weder vorher noch nachher → Kollision
            if (!endsBefore && !startsAfter)
                return false;
        }

        return true;
    }
}