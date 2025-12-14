using ConsulatTermine.Application.DTOs.Booking;

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
    /// Wird im Backend gesetzt, Frontend nicht erforderlich.
    /// </summary>
    public string? BookingReference { get; set; }
}
