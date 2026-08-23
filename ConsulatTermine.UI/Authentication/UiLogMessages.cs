namespace ConsulatTermine.UI.Authentication;

/// <summary>
/// Quellcodegenerierte Logmeldungen der Praesentationsschicht.
/// Protokolliert wird ausschliesslich die Mitarbeiter-Id, nie die eingegebene Kennung
/// oder das Passwort.
/// </summary>
internal static partial class UiLog
{
    [LoggerMessage(
        EventId = 2000,
        Level = LogLevel.Information,
        Message = "Anmeldung von Mitarbeiter {EmployeeId} erfolgreich.")]
    public static partial void LoginSucceeded(ILogger logger, int employeeId);

    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Information,
        Message = "Application:InitialAdminEmail ist nicht gesetzt. Es wird kein Administrator angelegt.")]
    public static partial void InitialAdminEmailMissing(ILogger logger);
}
