namespace ConsulatTermine.Application.DTOs.Booking;

public class BookingServiceSlotDto
{
    /// <summary>
    /// Der Service (z. B. Pass, Visa).
    /// </summary>
    public int ServiceId { get; set; }

    /// <summary>
    /// Startzeit des ausgewählten Slots.
    /// Beispiel: 2025-12-12 09:25:00
    /// </summary>
    public DateTime SlotTime { get; set; }
}
