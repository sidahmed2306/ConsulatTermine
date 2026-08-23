using System.Globalization;
using System.Security.Claims;
using ConsulatTermine.Application.Exceptions;
using ConsulatTermine.Application.Security;
using ConsulatTermine.Domain.Enums;
using Microsoft.AspNetCore.Components.Authorization;

namespace ConsulatTermine.UI.Authentication;

/// <summary>
/// Ermittelt den angemeldeten Mitarbeiter aus dem serverseitigen Authentifizierungs-Cookie.
/// Quelle ist ausschliesslich der <see cref="ClaimsPrincipal"/>; es werden keine Werte aus
/// Browser-Speicher oder Benutzereingaben uebernommen.
/// </summary>
public sealed class ClaimsEmployeeAuthorization : IEmployeeAuthorization
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public ClaimsEmployeeAuthorization(AuthenticationStateProvider authenticationStateProvider)
    {
        _authenticationStateProvider = authenticationStateProvider;
    }

    public async Task<CurrentEmployee?> GetCurrentEmployeeAsync()
    {
        var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
        return FromPrincipal(state.User);
    }

    public async Task<CurrentEmployee> RequireEmployeeAsync()
    {
        return await GetCurrentEmployeeAsync()
            ?? throw new NotAuthorizedException("Fuer diese Aktion ist eine Anmeldung erforderlich.");
    }

    public async Task<CurrentEmployee> RequireMinimumRoleAsync(EmployeeRole minimumRole)
    {
        var current = await RequireEmployeeAsync();

        if (current.Role < minimumRole)
        {
            throw new NotAuthorizedException();
        }

        return current;
    }

    /// <summary>
    /// Liest die Mitarbeiterdaten aus den Claims. Gibt <c>null</c> zurueck, sobald ein
    /// erforderlicher Claim fehlt oder unlesbar ist — ein halb gefuellter Principal gilt
    /// als nicht angemeldet.
    /// </summary>
    internal static CurrentEmployee? FromPrincipal(ClaimsPrincipal? principal)
    {
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var idValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idValue, CultureInfo.InvariantCulture, out var employeeId))
        {
            return null;
        }

        var employeeCode = principal.FindFirstValue(ClaimTypes.Name);
        if (string.IsNullOrWhiteSpace(employeeCode))
        {
            return null;
        }

        var roleValue = principal.FindFirstValue(ClaimTypes.Role);
        if (!Enum.TryParse<EmployeeRole>(roleValue, ignoreCase: false, out var role))
        {
            return null;
        }

        return new CurrentEmployee(employeeId, employeeCode, role);
    }
}
