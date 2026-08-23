using ConsulatTermine.Application.DTOs;

namespace ConsulatTermine.Application.Interfaces;

public interface IEmployeeAuthService
{
    Task<EmployeeLoginResultDto> LoginAsync(string employeeCode, string password);

    Task<bool> ChangePasswordAsync(int employeeId, string newPassword);

    /// <summary>
    /// Sendet einen Passwort-Reset-Link an die E-Mail-Adresse.
    /// Gibt true zurück, wenn eine E-Mail gesendet wurde (auch bei unbekannter E-Mail aus Sicherheitsgründen).
    /// </summary>
    Task<bool> RequestPasswordResetAsync(string email);

    /// <summary>
    /// Setzt das Passwort mit dem gültigen Token aus der E-Mail.
    /// Gibt true zurück, wenn erfolgreich.
    /// </summary>
    Task<bool> ResetPasswordWithTokenAsync(string token, string newPassword);
}
