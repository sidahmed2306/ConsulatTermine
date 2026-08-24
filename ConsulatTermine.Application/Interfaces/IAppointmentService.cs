using ConsulatTermine.Application.DTOs;
using ConsulatTermine.Domain.Entities;

namespace ConsulatTermine.Application.Interfaces;

public interface IAppointmentService
{
    // --- Buchung & freie Slots ---
    Task<List<AvailableSlotDto>> GetAvailableSlotDtosAsync(int serviceId, DateTime appointmentDate);

    /// <summary>
    /// Liefert die freien Slots aller Tage eines Monats in einem Zug, je Tag eine Liste.
    /// </summary>
    /// <param name="serviceId">Service, dessen Verfuegbarkeit gefragt ist.</param>
    /// <param name="monthFirstDay">Beliebiger Tag des gewuenschten Monats; ausgewertet wird der ganze Monat.</param>
    Task<Dictionary<DateOnly, List<AvailableSlotDto>>> GetAvailableSlotDtosForMonthAsync(
        int serviceId,
        DateOnly monthFirstDay);
    Task<Appointment> BookAsync(int serviceId, DateTime slotStart, string fullName, string email);
    Task<bool> CancelAsync(int appointmentId);

    // --- Ablauf im Konsulat ---
    Task<bool> CheckInAsync(int appointmentId);
    Task<bool> StartProcessingAsync(int appointmentId, int employeeId);
    Task<bool> CompleteAsync(int appointmentId, int employeeId);
    // Fertig verarbeitet

    Task<List<Appointment>> BookGroupAsync(GroupBookingDto dto);

    Task<List<Appointment>> GetByBookingReferenceAsync(string bookingReference);



    Task<List<Appointment>> GetAppointmentsForServiceOnDateAsync(
        int serviceId,
        DateTime appointmentDate);

    Task<bool> HideFromWaitingRoomAsync(int appointmentId, int employeeId);


    Task<List<Appointment>> GetActiveWaitingRoomAppointmentsAsync();


}
