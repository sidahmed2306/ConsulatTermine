using ConsulatTermine.Domain.Entities;

public static class AppointmentCalculator
{
    /// <summary>
    /// Berechnet alle theoretischen Slots (Zeiträume) für ein bestimmtes Datum.
    /// Berücksichtigt WorkingHours + DayOverrides + SlotDuration.
    /// </summary>
    public static List<(TimeSpan Start, TimeSpan End)> GetDailySlots(Service service, DateTime date)
    {
        var slots = new List<(TimeSpan Start, TimeSpan End)>();

        // --- 1) Prüfen, ob für dieses Datum ein Override existiert ---
        var overrideDay = service.DayOverrides
            ?.FirstOrDefault(d => d.Date.Date == date.Date);

        TimeSpan start;
        TimeSpan end;

        if (overrideDay != null)
        {
            // Wenn geschlossen markiert → keine Slots
            if (overrideDay.IsClosed)
            {
                return slots;
            }

            // Zeiten angegeben?
            if (!overrideDay.StartTime.HasValue || !overrideDay.EndTime.HasValue)
            {
                // Ungültiger Override (keine Zeit), behandeln als geschlossen
                return slots;
            }

            start = overrideDay.StartTime.Value;
            end   = overrideDay.EndTime.Value;
        }
        else
        {
            // --- 2) Keine Ausnahme → normale WorkingHours verwenden ---
            var work = service.WorkingHours?
                .FirstOrDefault(w => w.Day == date.DayOfWeek);

            if (work == null)
                return slots;

            start = work.StartTime;
            end   = work.EndTime;
        }

        // Validierung: Start < Ende
        if (start >= end)
            return slots;

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
    /// (Override-Kapazität wird ggf. separat behandelt.)
    /// </summary>
    public static int GetEffectiveCapacity(Service service)
    {
        int employeeCount = service.AssignedEmployees?.Count ?? 0;
        if (employeeCount == 0)
            return 0;

        return employeeCount * service.CapacityPerSlot;
    }

    /// <summary>
    /// Berechnet verfügbare Slots und verbleibende Plätze für ein Datum.
    /// Berücksichtigt Overrides, Slots und bestehende Buchungen.
    /// </summary>
  public static Dictionary<(TimeSpan Start, TimeSpan End), int> GetAvailableSlots(
    Service service,
    DateTime date,
    List<Appointment> existingAppointments)
{
    var slots = GetDailySlots(service, date);
    var result = new Dictionary<(TimeSpan Start, TimeSpan End), int>();

    // Wenn es keine Slots gibt, direkt zurück
    if (slots == null || slots.Count == 0)
        return result;

    // Anzahl Mitarbeiter für diesen Service
    int employeeCount = service.AssignedEmployees?.Count ?? 0;
    if (employeeCount == 0)
    {
        // Keine Mitarbeiter → 0 freie Plätze in allen Slots
        foreach (var slot in slots)
            result[slot] = 0;

        return result;
    }

    // Basis-Kapazität pro Slot aus dem Service
    int baseCapacityPerSlot = service.CapacityPerSlot;

    // Override für diesen Tag (falls vorhanden)
    var overrideDay = service.DayOverrides?
        .FirstOrDefault(d => d.Date.Date == date.Date);

    int capacityPerSlot = baseCapacityPerSlot;

    // Wenn Override-Kapazität gesetzt und > 0 → diese verwenden
    if (overrideDay?.CapacityPerSlotOverride is int overrideCap && overrideCap > 0)
    {
        capacityPerSlot = overrideCap;
    }

    // Effektive Kapazität pro Slot (Kapazität * Anzahl Mitarbeiter)
    int effectiveCapacity = capacityPerSlot * employeeCount;

    foreach (var slot in slots)
    {
        int booked = existingAppointments.Count(a =>
            a.Date.Date == date.Date &&
            a.Date.TimeOfDay >= slot.Start &&
            a.Date.TimeOfDay < slot.End &&
            a.ServiceId == service.Id);

        int free = Math.Max(0, effectiveCapacity - booked);
        result[slot] = free;
    }

    return result;
}


}
