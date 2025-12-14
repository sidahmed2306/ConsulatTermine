using ConsulatTermine.Application.Interfaces.Booking;

namespace ConsulatTermine.Infrastructure.Services.Booking
{
    public class BookingReferenceGenerator : IBookingReferenceGenerator
    {
        public string GenerateReference()
        {
            // Beispiel: CONSUL-2025-ABC123
            string prefix = "CONSUL";
            string year = DateTime.UtcNow.Year.ToString();

            // kurze eindeutige ID
            string unique = Guid.NewGuid()
                .ToString("N")
                .Substring(0, 6)
                .ToUpper();

            return $"{prefix}-{year}-{unique}";
        }
    }
}
