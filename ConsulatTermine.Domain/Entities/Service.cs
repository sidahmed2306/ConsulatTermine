

namespace ConsulatTermine.Domain.Entities;

public class Service
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    // Kapazität pro Zeitslot
    public int? CapacityPerSlot { get; set; }

    // Dauer eines Slots in Minuten (10, 15, 20 etc.)
    public int SlotDurationMinutes { get; set; }

    // Normal definierte Arbeitszeiten
    public List<WorkingHours> WorkingHours { get; set; } = new();

    // Override für Feiertage / besondere Tage
    public List<ServiceDayOverride> DayOverrides { get; set; } = new();

    // Mitarbeiter die diesen Service bedienen können
    public List<EmployeeServiceAssignment> AssignedEmployees { get; set; } = new();
}
