using ConsulatTermine.Application.DTOs.Booking;

namespace ConsulatTermine.Application.Interfaces.Booking;

public interface IBookingValidationService
{
    /// <summary>
    /// Prüft die gesamte Buchungsanfrage auf formale und logische Konsistenz.
    /// </summary>
    Task ValidateBookingRequestAsync(CreateBookingRequestDto request);
}
