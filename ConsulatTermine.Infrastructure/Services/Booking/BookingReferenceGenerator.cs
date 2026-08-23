using System.Globalization;
using ConsulatTermine.Application.Interfaces.Booking;

namespace ConsulatTermine.Infrastructure.Services.Booking;

/// <summary>
/// Erzeugt die Buchungsreferenz im Format CONSUL-2026-ABC123.
/// </summary>
public sealed class BookingReferenceGenerator : IBookingReferenceGenerator
{
    private const string Prefix = "CONSUL";

    private const int UniquePartLength = 6;

    public string GenerateReference()
    {
        // Durchgehend invariante Formatierung: die Anwendung laeuft auch unter ar-DZ,
        // wo kulturabhaengige Formatierung andere Ziffernzeichen erzeugen wuerde. Die
        // Referenz muss in E-Mail, Datenbank und Absage-Link exakt uebereinstimmen.
        var year = DateTime.UtcNow.Year.ToString(CultureInfo.InvariantCulture);

        var unique = Guid.NewGuid()
            .ToString("N", CultureInfo.InvariantCulture)[..UniquePartLength]
            .ToUpperInvariant();

        return $"{Prefix}-{year}-{unique}";
    }
}
