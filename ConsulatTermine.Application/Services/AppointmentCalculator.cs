using ConsulatTermine.Application.DTOs;
using ConsulatTermine.Domain.Entities;
using ConsulatTermine.Domain.Enums;

namespace ConsulatTermine.Application.Services;

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
            {
                return slots;
            }

            if (!dateOverride.StartTime.HasValue || !dateOverride.EndTime.HasValue)
            {
                return slots;
            }

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
            {
                return slots;
            }

            if (!weeklyOverride.StartTime.HasValue || !weeklyOverride.EndTime.HasValue)
            {
                return slots;
            }

            return BuildSlots(
                weeklyOverride.StartTime.Value,
                weeklyOverride.EndTime.Value,
                service.SlotDurationMinutes
            );
        }

        // 3️⃣ Reguläre Öffnungszeiten
        var work = workingHours.FirstOrDefault(w => w.Day == date.DayOfWeek);
        if (work == null)
        {
            return slots;
        }

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
        {
            return slots;
        }

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

        if (slots.Count == 0)
        {
            return result;
        }

        int capacity = GetEffectiveCapacity(service);

        foreach (var slot in slots)
        {
            int booked = existingAppointments.Count(a =>
    a.ServiceId == service.Id &&
    a.Date.Date == date.Date &&
    a.Date.TimeOfDay >= slot.Start &&
    a.Date.TimeOfDay < slot.End &&
    a.Status == AppointmentStatus.Booked);


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
            {
                return false;
            }
        }

        return true;
    }

    // ============================================================
    // 4) AUSWAEHLBARE SLOTS DER BUCHUNGSOBERFLAECHE
    // ============================================================

    /// <summary>
    /// Verdichtet die freien Slots eines Tages zu den Eintraegen, die in der
    /// Buchung tatsaechlich auswaehlbar sind: vertraeglich mit den bereits
    /// gewaehlten Terminen der anderen Services, nicht mehr zu kurzfristig und
    /// auf das Anzeigeraster zusammengefasst.
    /// </summary>
    /// <remarks>
    /// Kalender und Slot-Liste verwenden bewusst dieselbe Berechnung. Ein Tag
    /// gilt genau dann als buchbar, wenn diese Methode fuer ihn mindestens einen
    /// Eintrag liefert. Damit kann ein anklickbarer Tag nicht mehr leer sein.
    /// </remarks>
    /// <param name="availableSlots">Freie Slots des Tages aus der Kapazitaetsrechnung.</param>
    /// <param name="day">Tag, zu dem die Slots gehoeren.</param>
    /// <param name="slotDurationMinutes">Dauer eines Termins des betrachteten Service.</param>
    /// <param name="otherSelectedSlots">Bereits gewaehlte Termine der uebrigen Services.</param>
    /// <param name="bufferBetweenServices">Mindestabstand zwischen zwei Terminen derselben Buchung.</param>
    /// <param name="gridIntervalMinutes">Raster, in dem die Oberflaeche Slots zusammenfasst.</param>
    /// <param name="now">Aktueller Zeitpunkt; bestimmt die Vorlaufzeit am laufenden Tag.</param>
    public static List<AvailableSlotDto> GetSelectableSlots(
        IEnumerable<AvailableSlotDto> availableSlots,
        DateOnly day,
        int slotDurationMinutes,
        IReadOnlyCollection<(DateTime Start, int DurationMinutes)> otherSelectedSlots,
        TimeSpan bufferBetweenServices,
        int gridIntervalMinutes,
        DateTime now)
    {
        ArgumentNullException.ThrowIfNull(availableSlots);
        ArgumentNullException.ThrowIfNull(otherSelectedSlots);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(gridIntervalMinutes);

        // Am laufenden Tag faellt weg, was ohne Vorlauf nicht mehr erreichbar ist.
        var earliestStart = day == DateOnly.FromDateTime(now)
            ? now.AddMinutes(gridIntervalMinutes)
            : DateTime.MinValue;

        return availableSlots
            .Where(slot => slot.FreeCapacity > 0)
            .Where(slot => slot.SlotStart > earliestStart)
            .Where(slot => IsSlotTimeCompatible(
                slot.SlotStart,
                slotDurationMinutes,
                otherSelectedSlots,
                bufferBetweenServices))
            .GroupBy(slot => RoundDownToGrid(slot.SlotStart, gridIntervalMinutes))
            .Select(group => new AvailableSlotDto
            {
                SlotStart = group.Key,
                FreeCapacity = group.Min(slot => slot.FreeCapacity)
            })
            .Where(slot => slot.FreeCapacity > 0)
            .OrderBy(slot => slot.SlotStart)
            .ToList();
    }

    private static DateTime RoundDownToGrid(DateTime slotStart, int gridIntervalMinutes)
    {
        var minutesSinceMidnight = (int)(slotStart - slotStart.Date).TotalMinutes;
        var rounded = minutesSinceMidnight / gridIntervalMinutes * gridIntervalMinutes;
        return slotStart.Date.AddMinutes(rounded);
    }
}
