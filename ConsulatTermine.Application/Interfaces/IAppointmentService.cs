using ConsulatTermine.Application.DTOs;
using ConsulatTermine.Domain.Entities;

namespace ConsulatTermine.Application.Interfaces;

public interface IAppointmentService
{
    // --- Buchung & freie Slots ---
    Task<List<AvailableSlotDto>> GetAvailableSlotDtosAsync(int serviceId, DateTime date);
    Task<Appointment> BookAsync(int serviceId, DateTime slotStart, string fullName, string email);
    Task<bool> CancelAsync(int appointmentId);
    Task<List<Appointment>> GetAppointmentsByServiceAndDayAsync(int serviceId, DateTime date);

    // --- Ablauf im Konsulat ---
    Task<bool> CheckInAsync(int appointmentId);          // Warten in Warteschlange
    Task<Appointment?> GetNextAsync(int serviceId);       // Nächster wartender Bürger
    Task<bool> StartProcessingAsync(int appointmentId);   // Mitarbeiter bearbeitet diesen Bürger
    Task<bool> CompleteAsync(int appointmentId);          // Fertig verarbeitet
}
