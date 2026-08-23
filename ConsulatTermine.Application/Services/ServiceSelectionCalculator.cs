namespace ConsulatTermine.Application.Services;

/// <summary>
/// Regeln des ersten Buchungsschritts: wie viele Personen einen Termin brauchen und
/// wie viele Termine je Service auf sie verteilt werden duerfen.
///
/// Fachliche Grundregeln:
/// 1. Jede Person braucht mindestens einen Termin. Die Summe aller Service-Zuordnungen
///    muss daher mindestens der Personenzahl entsprechen.
/// 2. Ein einzelner Service kann hoechstens einmal je Person belegt werden.
/// 3. Insgesamt sind hoechstens <see cref="MaxServicesPerPerson"/> Termine je Person moeglich.
///
/// Die Berechnung ist frei von Persistenz und UI und damit vollstaendig testbar.
/// </summary>
public static class ServiceSelectionCalculator
{
    /// <summary>Obergrenze der Personen, die gemeinsam gebucht werden koennen.</summary>
    public const int MaxPersons = 5;

    /// <summary>Obergrenze der Termine, die eine einzelne Person erhalten kann.</summary>
    public const int MaxServicesPerPerson = 3;

    /// <summary>Summe aller einem Service zugeordneten Termine.</summary>
    public static int TotalAssigned(IReadOnlyDictionary<int, int> assignments)
    {
        ArgumentNullException.ThrowIfNull(assignments);

        return assignments.Values.Sum();
    }

    /// <summary>Obergrenze aller Termine bei der angegebenen Personenzahl.</summary>
    public static int MaxTotalAssignments(int personCount)
    {
        EnsureValidPersonCount(personCount);

        return personCount * MaxServicesPerPerson;
    }

    /// <summary>
    /// Anzahl der Termine, die noch fehlen, bis jede Person mindestens einen Termin hat.
    /// Null bedeutet: die Auswahl erfuellt die Mindestanforderung.
    /// </summary>
    public static int MissingAssignments(IReadOnlyDictionary<int, int> assignments, int personCount)
    {
        EnsureValidPersonCount(personCount);

        return Math.Max(0, personCount - TotalAssigned(assignments));
    }

    /// <summary>
    /// Gibt an, ob dem angegebenen Service ein weiterer Termin hinzugefuegt werden darf.
    /// </summary>
    public static bool CanAddService(
        IReadOnlyDictionary<int, int> assignments,
        int serviceId,
        int personCount)
    {
        EnsureValidPersonCount(personCount);

        if (TotalAssigned(assignments) >= MaxTotalAssignments(personCount))
        {
            return false;
        }

        return AssignedCount(assignments, serviceId) < personCount;
    }

    /// <summary>
    /// Gibt an, ob dem angegebenen Service ein Termin entzogen werden darf.
    /// </summary>
    public static bool CanRemoveService(IReadOnlyDictionary<int, int> assignments, int serviceId)
        => AssignedCount(assignments, serviceId) > 0;

    /// <summary>
    /// Gibt an, ob die Auswahl vollstaendig ist und der naechste Schritt freigegeben werden darf.
    /// </summary>
    public static bool IsSelectionComplete(IReadOnlyDictionary<int, int> assignments, int personCount)
    {
        EnsureValidPersonCount(personCount);

        var total = TotalAssigned(assignments);

        return total >= personCount && total <= MaxTotalAssignments(personCount);
    }

    /// <summary>Anzahl der Termine, die dem angegebenen Service zugeordnet sind.</summary>
    public static int AssignedCount(IReadOnlyDictionary<int, int> assignments, int serviceId)
    {
        ArgumentNullException.ThrowIfNull(assignments);

        return assignments.TryGetValue(serviceId, out var count) ? count : 0;
    }

    /// <summary>
    /// Bringt eine Auswahl auf einen zulaessigen Stand und liefert das Ergebnis als neue Zuordnung.
    ///
    /// Notwendig, weil sich die Personenzahl nachtraeglich verringern laesst: eine vorher
    /// gueltige Auswahl kann dadurch mehr Termine je Service oder insgesamt enthalten, als
    /// die neue Personenzahl erlaubt. Ueberzaehlige Termine werden zuerst bei den Services
    /// mit den meisten Zuordnungen abgebaut, damit die Auswahl moeglichst breit bleibt.
    /// </summary>
    public static Dictionary<int, int> Normalize(
        IReadOnlyDictionary<int, int> assignments,
        int personCount)
    {
        ArgumentNullException.ThrowIfNull(assignments);
        EnsureValidPersonCount(personCount);

        var normalized = new Dictionary<int, int>();

        foreach (var (serviceId, count) in assignments)
        {
            var capped = Math.Min(count, personCount);

            if (capped > 0)
            {
                normalized[serviceId] = capped;
            }
        }

        var maxTotal = MaxTotalAssignments(personCount);

        while (TotalAssigned(normalized) > maxTotal)
        {
            var serviceId = normalized.OrderByDescending(entry => entry.Value).First().Key;

            normalized[serviceId]--;

            if (normalized[serviceId] <= 0)
            {
                normalized.Remove(serviceId);
            }
        }

        return normalized;
    }

    private static void EnsureValidPersonCount(int personCount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(personCount, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(personCount, MaxPersons);
    }
}
