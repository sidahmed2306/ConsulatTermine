namespace ConsulatTermine.Application.Interfaces
{
    public interface IAppointmentCalendarQueryService
    {
        /// <summary>
        /// Liefert für einen Service und einen Monat,
        /// welche Tage grundsätzlich buchbar sind.
        /// </summary>
        Task<Dictionary<DateOnly, bool>> GetBookableDaysAsync(
            int serviceId,
            int year,
            int month);
    }
}
