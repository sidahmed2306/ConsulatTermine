using ConsulatTermine.Application.DTOs.Booking;

namespace ConsulatTermine.Application.Interfaces.Booking;

public interface ISlotAvailabilityService
{
    /// <summary>
    /// Prüft, ob alle benötigten Slots ausreichend Plätze haben.
    /// </summary>
    Task ValidateSlotCapacitiesAsync(CreateBookingRequestDto request);

    /// <summary>
    /// Liefert zurück, ob ein bestimmter Slot für einen Service verfügbar ist.
    /// </summary>
    Task<bool> IsSlotAvailableAsync(int serviceId, DateTime slotTime);
}
