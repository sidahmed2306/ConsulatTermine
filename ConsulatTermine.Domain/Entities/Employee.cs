using ConsulatTermine.Domain.Enums;

namespace ConsulatTermine.Domain.Entities;

public class Employee
{
    public int Id { get; set; }

    // -------------------------------------------------
    // Fachliche Identität
    // -------------------------------------------------

    /// <summary>
    /// Interne Mitarbeiter-Kennung (z. B. CDZ-001)
    /// Wird systemseitig generiert.
    /// </summary>
    public string EmployeeCode { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Dienstliche E-Mail-Adresse
    /// </summary>
    public string Email { get; set; } = string.Empty;

    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Aktiv / Deaktiviert (z. B. bei Ausscheiden)
    /// </summary>
    public bool IsActive { get; set; } = true;

    // -------------------------------------------------
    // Login / Sicherheit (fachlich vorbereitet)
    // -------------------------------------------------

    /// <summary>
    /// Flag für erzwungene Passwortänderung beim Erst-Login
    /// </summary>
    public bool MustChangePassword { get; set; } = true;

    // -------------------------------------------------
    // Meta
    // -------------------------------------------------
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // -------------------------------------------------
    // Service-Zuordnungen
    // -------------------------------------------------
    public List<EmployeeServiceAssignment> AssignedServices { get; set; } = new();

    /// <summary>
    /// Passwort im Format von <c>Microsoft.AspNetCore.Identity.PasswordHasher</c>
    /// (PBKDF2 mit Salt und Iterationszahl). Niemals Klartext, niemals loggen.
    /// <c>null</c>, solange noch kein Passwort gesetzt wurde.
    /// </summary>
    public string? PasswordHash { get; set; }

    /// <summary>
    /// Rolle des Mitarbeiters (Employee, ServiceChef, Admin)
    /// </summary>
    public EmployeeRole Role { get; set; } = EmployeeRole.Employee;

    /// <summary>
    /// SHA-256-Hash des Tokens zum Zuruecksetzen des Passworts.
    /// Gespeichert wird nur der Hash: Wer die Datenbank liest, kann daraus keinen
    /// gueltigen Link bauen. Der Klartext existiert ausschliesslich in der versendeten E-Mail.
    /// </summary>
    public string? PasswordResetTokenHash { get; set; }

    /// <summary>
    /// Ablaufdatum des Passwort-Reset-Links.
    /// Nach diesem Zeitpunkt ist eine Zurücksetzung nicht mehr möglich.
    /// </summary>
    public DateTime? PasswordResetTokenExpiresAt { get; set; }

    /// <summary>
    /// Aufeinanderfolgende Fehlversuche bei der Anmeldung. Wird bei Erfolg zurueckgesetzt.
    /// </summary>
    public int FailedLoginAttempts { get; set; }

    /// <summary>
    /// Zeitpunkt, bis zu dem die Anmeldung nach zu vielen Fehlversuchen gesperrt ist.
    /// </summary>
    public DateTime? LockoutEndsAt { get; set; }
}
