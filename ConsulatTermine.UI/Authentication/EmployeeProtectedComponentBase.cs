using Microsoft.AspNetCore.Components;
using ConsulatTermine.UI.Authentication;

namespace ConsulatTermine.UI.Authentication;

/// <summary>
/// Basisklasse für ALLE geschützten Employee-Seiten.
/// Erzwingt Login + Session-Timeout.
/// </summary>
public abstract class EmployeeProtectedComponentBase : ComponentBase
{
    [Inject] protected EmployeeSessionService EmployeeSession { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;

    protected int CurrentEmployeeId { get; private set; }

    private bool _checked;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || _checked)
            return;

        _checked = true;

        var (isAuthenticated, employeeId) =
            await EmployeeSession.EnsureAuthenticatedAsync();

        if (!isAuthenticated)
        {
            Navigation.NavigateTo("/employee/login", replace: true);
            return;
        }

        CurrentEmployeeId = employeeId;
        StateHasChanged();
    }
}
