using ConsulatTermine.Domain.Enums;

namespace ConsulatTermine.Application.Security;

/// <summary>
/// Serverseitige Autorisierung fuer Anwendungsfaelle des Mitarbeiterbereichs.
/// Jede geschuetzte Zustandsaenderung ruft diese Pruefung auf, unabhaengig davon,
/// ob die UI den zugehoerigen Bedienelement ueberhaupt anzeigt.
/// </summary>
public interface IEmployeeAuthorization
{
    /// <summary>
    /// Liefert den angemeldeten Mitarbeiter oder <c>null</c>, wenn niemand angemeldet ist.
    /// </summary>
    Task<CurrentEmployee?> GetCurrentEmployeeAsync();

    /// <summary>
    /// Verlangt einen angemeldeten Mitarbeiter beliebiger Rolle.
    /// </summary>
    /// <exception cref="Exceptions.NotAuthorizedException">Niemand ist angemeldet.</exception>
    Task<CurrentEmployee> RequireEmployeeAsync();

    /// <summary>
    /// Verlangt mindestens die angegebene Rolle. Die Rollen sind aufsteigend geordnet:
    /// <see cref="EmployeeRole.Employee"/> &lt; <see cref="EmployeeRole.ServiceChef"/>
    /// &lt; <see cref="EmployeeRole.Admin"/>.
    /// </summary>
    /// <exception cref="Exceptions.NotAuthorizedException">
    /// Niemand ist angemeldet oder die Rolle reicht nicht aus.
    /// </exception>
    Task<CurrentEmployee> RequireMinimumRoleAsync(EmployeeRole minimumRole);
}
