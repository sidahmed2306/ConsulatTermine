using ConsulatTermine.Application.DTOs.Booking;

namespace ConsulatTermine.Application.Interfaces.Booking;

public interface IBookingService
{
    /// <summary>
    /// Erstellt eine vollständige Buchung (inkl. Hauptbucher, Begleitpersonen und Slots).
    /// </summary>
    Task<string> CreateBookingAsync(CreateBookingRequestDto request);
}
