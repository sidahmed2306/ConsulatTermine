namespace ConsulatTermine.Application.Interfaces
{
    /// <summary>
    /// Service für den Versand von E-Mails.
    /// Wird z. B. für Terminbestätigungen verwendet.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sendet eine Terminbestätigung an die Hauptperson.
        /// </summary>
        /// <param name="toEmail">Empfänger (Hauptperson)</param>
        /// <param name="fullName">Vollständiger Name</param>
        /// <param name="bookingReference">Buchungsnummer</param>
        Task SendBookingConfirmationAsync(
            string toEmail,
            string fullName,
            string bookingReference,
            string cancelToken);

            Task SendCancellationConfirmationAsync(
    string toEmail,
    string fullName,
    string bookingReference);

    Task SendPartialCancellationAsync(
    string toEmail,
    string fullName,
    string serviceName,
    DateTime date);

    Task SendEmployeeWelcomeEmailAsync(
    string toEmail,
    string fullName,
    string employeeCode,
    string temporaryPassword,
    string changePasswordLink);

Task SendEmployeePasswordChangedConfirmationEmailAsync(
    string toEmail,
    string fullName,
    string loginLink);


    }

    
}
