using ConsulatTermine.Application.DTOs;
using ConsulatTermine.Domain.Entities;

namespace ConsulatTermine.Application.Interfaces;

/// <summary>
/// Verwaltung der Mitarbeiterstammdaten.
/// Alle Methoden ausser <see cref="EnsureInitialAdminAsync"/> pruefen die Berechtigung
/// des aufrufenden Benutzers serverseitig.
/// </summary>
public interface IEmployeeService
{
    /// <summary>Erfordert mindestens die Rolle ServiceChef.</summary>
    Task<Employee> CreateEmployeeAsync(EmployeeDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Erfordert mindestens die Rolle ServiceChef. Das Aendern der Rolle ist Administratoren
    /// vorbehalten.
    /// </summary>
    Task<Employee> UpdateEmployeeAsync(int id, EmployeeDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Erfordert eine Anmeldung. Ein einfacher Mitarbeiter darf nur den eigenen Datensatz lesen.
    /// </summary>
    Task<Employee?> GetEmployeeByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Erfordert mindestens die Rolle ServiceChef.</summary>
    Task<List<Employee>> GetAllEmployeesAsync(CancellationToken cancellationToken = default);

    /// <summary>Erfordert die Rolle Admin.</summary>
    Task<bool> DeleteEmployeeAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Legt beim Anwendungsstart einen Administrator an, falls noch keiner existiert.
    /// Wird ohne angemeldeten Benutzer aufgerufen und daher nicht autorisiert.
    /// </summary>
    Task EnsureInitialAdminAsync(string adminEmail, CancellationToken cancellationToken = default);
}
