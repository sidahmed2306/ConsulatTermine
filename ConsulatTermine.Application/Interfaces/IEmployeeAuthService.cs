using ConsulatTermine.Application.DTOs;

namespace ConsulatTermine.Application.Interfaces;

/// <summary>
/// Anmeldung und Passwortverwaltung des internen Mitarbeiterbereichs.
/// </summary>
public interface IEmployeeAuthService
{
    /// <summary>
    /// Prueft Kennung und Passwort.
    /// Schlaegt die Pruefung fehl, ist die Fehlermeldung bewusst unspezifisch: sie
    /// unterscheidet nicht zwischen unbekannter Kennung, falschem Passwort, gesperrtem
    /// und deaktiviertem Konto.
    /// </summary>
    Task<EmployeeLoginResultDto> LoginAsync(
        string employeeCode,
        string password,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Setzt ein neues Passwort und entwertet dabei einen offenen Reset-Link.
    /// </summary>
    Task<bool> ChangePasswordAsync(
        int employeeId,
        string newPassword,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Versendet einen Link zum Zuruecksetzen des Passworts.
    /// Gibt immer <c>true</c> zurueck, damit sich ueber diese Funktion nicht ermitteln
    /// laesst, welche E-Mail-Adressen im System bekannt sind.
    /// </summary>
    Task<bool> RequestPasswordResetAsync(
        string email,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Setzt das Passwort mit dem Token aus der E-Mail zurueck.
    /// </summary>
    /// <returns><c>false</c>, wenn das Token unbekannt oder abgelaufen ist.</returns>
    Task<bool> ResetPasswordWithTokenAsync(
        string token,
        string newPassword,
        CancellationToken cancellationToken = default);
}
