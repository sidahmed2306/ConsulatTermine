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
            string bookingReference);
    }
}
