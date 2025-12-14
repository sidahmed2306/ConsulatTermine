namespace ConsulatTermine.Application.DTOs.Booking;

public class BookingPersonDto
{
    /// <summary>
    /// Laufende Nummer der Person, wird im Backend gesetzt.
    /// 1 = Hauptbucher, 2..n = Begleiter.
    /// </summary>
    public int PersonIndex { get; set; }

    /// <summary>
    /// Vollständiger Name der Person (Frontend setzt das).
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// E-Mail: nur beim Hauptbucher Pflicht.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Liste der Dienste und zugehörigen Slots,
    /// die diese Person ausgewählt hat.
    /// 1 Eintrag pro Service.
    /// </summary>
    public List<BookingServiceSlotDto> ServiceSlots { get; set; } = new();
}
