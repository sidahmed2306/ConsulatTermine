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

/// <summary>
/// Optionale Verknüpfung mit ASP.NET Identity
/// </summary>
public string? IdentityUserId { get; set; }

// -------------------------------------------------
// Meta
// -------------------------------------------------
public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

// -------------------------------------------------
// Service-Zuordnungen
// -------------------------------------------------
public List<EmployeeServiceAssignment> AssignedServices { get; set; } = new();

/// <summary>
/// Temporäres Initial‑Passwort (Hash oder Klartext nur für Setup‑Phase)
/// Wird nach erstem Login ungültig
/// </summary>
public string? TemporaryPassword { get; set; }

/// <summary>
/// Rolle des Mitarbeiters (Employee, ServiceChef, Admin)
/// </summary>
public EmployeeRole Role { get; set; } = EmployeeRole.Employee;

}