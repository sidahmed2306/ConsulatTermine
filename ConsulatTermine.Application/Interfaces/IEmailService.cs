using ConsulatTermine.Application.DTOs.Booking;

namespace ConsulatTermine.Application.Interfaces;

/// <summary>
/// Versand des Schriftverkehrs an Buergerinnen, Buerger und Mitarbeiter.
/// </summary>
/// <remarks>
/// Jede Methode nimmt die Sprache des Empfaengers entgegen. Sie wird ausdruecklich
/// uebergeben und nicht aus der laufenden Sitzung abgeleitet, weil der Ausloesende
/// und der Empfaenger unterschiedliche Sprachen verwenden koennen.
/// Eine unbekannte Angabe faellt auf die Standardsprache zurueck.
/// </remarks>
public interface IEmailService
{
    /// <summary>
    /// Sendet die Terminbestaetigung an die Hauptperson.
    /// </summary>
    /// <param name="toEmail">Empfaenger, immer die Hauptperson der Buchung.</param>
    /// <param name="fullName">Vollstaendiger Name der Hauptperson.</param>
    /// <param name="bookingReference">Gemeinsame Referenz aller Termine der Buchung.</param>
    /// <param name="cancelToken">Einmalwert fuer den Absage-Link.</param>
    /// <param name="appointments">Alle Termine der Buchung fuer die Uebersicht.</param>
    /// <param name="language">Sprache der Buchung als Kulturname, zum Beispiel <c>ar-DZ</c>.</param>
    Task SendBookingConfirmationAsync(
        string toEmail,
        string fullName,
        string bookingReference,
        string cancelToken,
        IReadOnlyList<BookingEmailAppointmentDto> appointments,
        string language);

    /// <summary>
    /// Bestaetigt, dass alle Termine einer Buchung abgesagt wurden.
    /// </summary>
    Task SendCancellationConfirmationAsync(
        string toEmail,
        string fullName,
        string bookingReference,
        string language);

    /// <summary>
    /// Bestaetigt die Absage eines einzelnen Termins, waehrend weitere bestehen bleiben.
    /// </summary>
    Task SendPartialCancellationAsync(
        string toEmail,
        string fullName,
        string serviceName,
        DateTime appointmentDate,
        string language);

    /// <summary>
    /// Uebermittelt einem neuen Mitarbeiter Kennung und vorlaeufiges Passwort.
    /// </summary>
    Task SendEmployeeWelcomeEmailAsync(
        string toEmail,
        string fullName,
        string employeeCode,
        string temporaryPassword,
        string changePasswordLink,
        string language);

    /// <summary>
    /// Bestaetigt einem Mitarbeiter die Aenderung seines Passworts.
    /// </summary>
    Task SendEmployeePasswordChangedConfirmationEmailAsync(
        string toEmail,
        string fullName,
        string loginLink,
        string language);

    /// <summary>
    /// Sendet einem Mitarbeiter den Link zum Zuruecksetzen seines Passworts.
    /// </summary>
    Task SendEmployeePasswordResetEmailAsync(
        string toEmail,
        string fullName,
        string resetLink,
        string language);
}
