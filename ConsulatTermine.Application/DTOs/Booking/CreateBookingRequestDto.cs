using ConsulatTermine.Application.Localization;

namespace ConsulatTermine.Application.DTOs.Booking;

public class CreateBookingRequestDto
{
    /// <summary>
    /// Hauptbucher (Pflicht).
    /// </summary>
    public BookingPersonDto MainPerson { get; set; } = new();

    /// <summary>
    /// Alle Begleitpersonen.
    /// </summary>
    public List<BookingPersonDto> AccompanyingPersons { get; set; } = new();

    /// <summary>
    /// Zeitzone, optional – nützlich für internationale Konsulate.
    /// </summary>
    public string TimeZone { get; set; } = "Europe/Berlin";

    /// <summary>
    /// Sprache, in der gebucht wurde, als Kulturname wie <c>ar-DZ</c>. Bestimmt die
    /// Sprache der Bestaetigung und jeder spaeteren Absage. Eine unbekannte oder
    /// fehlende Angabe ergibt die Standardsprache.
    /// </summary>
    public string Language { get; set; } = SupportedLanguages.DefaultCultureCode;

    /// <summary>
    /// Wird im Backend gesetzt, Frontend nicht erforderlich.
    /// </summary>
    public string? BookingReference { get; set; }
}
