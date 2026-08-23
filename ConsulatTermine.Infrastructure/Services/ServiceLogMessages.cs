using Microsoft.Extensions.Logging;

namespace ConsulatTermine.Infrastructure.Services;

/// <summary>
/// Quellcodegenerierte Logmeldungen der Infrastruktur.
/// Der Generator erzeugt je Meldung eine typisierte Methode ohne Boxing und ohne
/// Formatierung, solange der Loglevel nicht aktiv ist.
///
/// Keine Meldung enthaelt Passwoerter, Tokens, Connection Strings oder unnoetige
/// personenbezogene Daten (harness/design.md Abschnitt 10). Bei der Anmeldung wird
/// bewusst nur die Mitarbeiter-Id protokolliert, nie die eingegebene Kennung.
/// </summary>
internal static partial class ServiceLog
{
    // ---------------------------------------------------------------- Anmeldung

    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Information,
        Message = "Anmeldung fehlgeschlagen: unbekannte oder inaktive Kennung.")]
    public static partial void LoginRejectedUnknownAccount(ILogger logger);

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Warning,
        Message = "Anmeldung abgelehnt: Konto {EmployeeId} ist bis {LockoutEnd} gesperrt.")]
    public static partial void LoginRejectedLockedOut(ILogger logger, int employeeId, DateTime lockoutEnd);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Information,
        Message = "Mitarbeiter {EmployeeId} hat sich angemeldet.")]
    public static partial void LoginSucceeded(ILogger logger, int employeeId);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Warning,
        Message = "Konto {EmployeeId} nach zu vielen Fehlversuchen bis {LockoutEnd} gesperrt.")]
    public static partial void AccountLockedOut(ILogger logger, int employeeId, DateTime? lockoutEnd);

    [LoggerMessage(
        EventId = 1004,
        Level = LogLevel.Information,
        Message = "Fehlversuch {Attempt} von {MaxAttempts} fuer Mitarbeiter {EmployeeId}.")]
    public static partial void LoginAttemptFailed(ILogger logger, int attempt, int maxAttempts, int employeeId);

    // ------------------------------------------------------------ Passwortpflege

    [LoggerMessage(
        EventId = 1010,
        Level = LogLevel.Information,
        Message = "Mitarbeiter {EmployeeId} hat das Passwort geaendert.")]
    public static partial void PasswordChanged(ILogger logger, int employeeId);

    [LoggerMessage(
        EventId = 1011,
        Level = LogLevel.Information,
        Message = "Passwort-Reset fuer unbekannte Adresse angefordert.")]
    public static partial void PasswordResetRequestedForUnknownAddress(ILogger logger);

    [LoggerMessage(
        EventId = 1012,
        Level = LogLevel.Information,
        Message = "Passwort-Reset fuer Mitarbeiter {EmployeeId} angefordert.")]
    public static partial void PasswordResetRequested(ILogger logger, int employeeId);

    [LoggerMessage(
        EventId = 1013,
        Level = LogLevel.Warning,
        Message = "Passwort-Reset mit ungueltigem oder abgelaufenem Token abgelehnt.")]
    public static partial void PasswordResetTokenRejected(ILogger logger);

    [LoggerMessage(
        EventId = 1014,
        Level = LogLevel.Information,
        Message = "Passwort von Mitarbeiter {EmployeeId} wurde zurueckgesetzt.")]
    public static partial void PasswordReset(ILogger logger, int employeeId);

    // ------------------------------------------------------ Mitarbeiterverwaltung

    [LoggerMessage(
        EventId = 1020,
        Level = LogLevel.Information,
        Message = "Mitarbeiter {EmployeeId} mit Kennung {EmployeeCode} angelegt.")]
    public static partial void EmployeeCreated(ILogger logger, int employeeId, string employeeCode);

    [LoggerMessage(
        EventId = 1021,
        Level = LogLevel.Information,
        Message = "Mitarbeiter {EmployeeId} geaendert.")]
    public static partial void EmployeeUpdated(ILogger logger, int employeeId);

    [LoggerMessage(
        EventId = 1022,
        Level = LogLevel.Information,
        Message = "Mitarbeiter {EmployeeId} geloescht.")]
    public static partial void EmployeeDeleted(ILogger logger, int employeeId);

    [LoggerMessage(
        EventId = 1023,
        Level = LogLevel.Warning,
        Message = "Kein Administrator vorhanden. Konto {EmployeeCode} wurde angelegt; das "
                  + "Initialpasswort wurde per E-Mail versendet und muss beim ersten Login "
                  + "geaendert werden.")]
    public static partial void InitialAdminCreated(ILogger logger, string employeeCode);

    // ------------------------------------------------------------------- Versand

    [LoggerMessage(
        EventId = 1030,
        Level = LogLevel.Warning,
        Message = "Kein SMTP-Server konfiguriert. Es wird keine E-Mail versendet.")]
    public static partial void SmtpNotConfigured(ILogger logger);

    [LoggerMessage(
        EventId = 1031,
        Level = LogLevel.Error,
        Message = "Absage-E-Mail zu Termin {AppointmentId} konnte nicht versendet werden.")]
    public static partial void CancellationMailFailed(ILogger logger, Exception exception, int appointmentId);

    [LoggerMessage(
        EventId = 1032,
        Level = LogLevel.Error,
        Message = "Bestaetigungs-E-Mail zur Buchung {BookingReference} konnte nicht versendet werden.")]
    public static partial void ConfirmationMailFailed(ILogger logger, Exception exception, string bookingReference);
}
