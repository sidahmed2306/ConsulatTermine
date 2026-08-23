using ConsulatTermine.Domain.Enums;

namespace ConsulatTermine.Application.DTOs;

/// <summary>
/// Ergebnis eines Anmeldeversuchs. Enthaelt bei Erfolg alles, was fuer den Aufbau der
/// Anmelde-Cookies noetig ist, damit kein zweiter Datenbankzugriff erforderlich wird.
/// </summary>
public sealed class EmployeeLoginResultDto
{
    public bool Success { get; set; }

    /// <summary>
    /// Anzeigetext bei Misserfolg. Bewusst unspezifisch, siehe <c>IEmployeeAuthService</c>.
    /// </summary>
    public string? ErrorMessage { get; set; }

    public int? EmployeeId { get; set; }

    public string? EmployeeCode { get; set; }

    public EmployeeRole? Role { get; set; }

    /// <summary>
    /// Der Mitarbeiter muss vor der weiteren Nutzung ein eigenes Passwort setzen.
    /// </summary>
    public bool MustChangePassword { get; set; }
}
