namespace ConsulatTermine.Application.Interfaces.Booking;

public interface IBookingReferenceGenerator
{
    /// <summary>
    /// Erstellt eine eindeutige Referenz für eine Buchung.
    /// </summary>
    string GenerateReference();
}
