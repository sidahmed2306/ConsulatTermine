

namespace ConsulatTermine.Domain.Entities;

public class Service
{
    public int Id { get; set; }

    /// <summary>
    /// Bezeichnung in der Amtssprache des Standorts. Pflichtangabe und zugleich
    /// Rueckfallebene, wenn fuer eine Sprache keine Uebersetzung gepflegt ist.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Englische Bezeichnung. Leer, solange die Verwaltung sie nicht gepflegt hat.
    /// </summary>
    public string? NameEnglish { get; set; }

    /// <summary>
    /// Arabische Bezeichnung. Leer, solange die Verwaltung sie nicht gepflegt hat.
    /// </summary>
    public string? NameArabic { get; set; }

    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Englische Beschreibung. Leer, solange die Verwaltung sie nicht gepflegt hat.
    /// </summary>
    public string? DescriptionEnglish { get; set; }

    /// <summary>
    /// Arabische Beschreibung. Leer, solange die Verwaltung sie nicht gepflegt hat.
    /// </summary>
    public string? DescriptionArabic { get; set; }

    // Kapazität pro Zeitslot
    public int? CapacityPerSlot { get; set; }

    // Dauer eines Slots in Minuten (10, 15, 20 etc.)
    public int SlotDurationMinutes { get; set; }

    // Normal definierte Arbeitszeiten
    public List<WorkingHours> WorkingHours { get; set; } = new();

    public string Floor { get; set; } = string.Empty;

    // Override für Feiertage / besondere Tage
    public List<ServiceDayOverride> DayOverrides { get; set; } = new();

    // Mitarbeiter die diesen Service bedienen können
    public List<EmployeeServiceAssignment> AssignedEmployees { get; set; } = new();
}
