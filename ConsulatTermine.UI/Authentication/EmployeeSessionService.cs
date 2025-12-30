using Blazored.SessionStorage;
using ConsulatTermine.Domain.Enums;

namespace ConsulatTermine.UI.Authentication;

public class EmployeeSessionService
{
    private readonly ISessionStorageService _session;

    // ⏱ Timeout (aktuell 1 Minute – später z. B. 60 / 120)
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(120);

    // 🔔 State-Change-Event (für MainLayout / NavMenu)
    public event Action? OnChange;

    public bool IsAuthenticated { get; private set; }

    public EmployeeRole? CurrentRole { get; private set; }

    public EmployeeSessionService(ISessionStorageService session)
    {
        _session = session;
    }

    // ----------------------------------------------------
    // LOGIN
    // ----------------------------------------------------
    public async Task SignInAsync(
        int employeeId,
        string employeeCode,
        EmployeeRole role
    )
    {
        await _session.SetItemAsync(EmployeeSessionKeys.EmployeeId, employeeId);
        await _session.SetItemAsync(EmployeeSessionKeys.EmployeeCode, employeeCode);
        await _session.SetItemAsync(EmployeeSessionKeys.EmployeeRole, (int)role);

        await TouchAsync();

        IsAuthenticated = true;
        CurrentRole = role;

        NotifyStateChanged();
    }

    // ----------------------------------------------------
    // AUTH-CHECK + TIMEOUT
    // ----------------------------------------------------
    public async Task<(bool IsAuthenticated, int EmployeeId)> EnsureAuthenticatedAsync()
    {
        var employeeId =
            await _session.GetItemAsync<int>(EmployeeSessionKeys.EmployeeId);

        if (employeeId <= 0)
        {
            ClearState();
            return (false, 0);
        }

        var ticks =
            await _session.GetItemAsync<long>(EmployeeSessionKeys.LastActivityUtcTicks);

        if (ticks <= 0)
        {
            await SignOutAsync();
            return (false, 0);
        }

        var last = new DateTime(ticks, DateTimeKind.Utc);
        if (DateTime.UtcNow - last > Timeout)
        {
            await SignOutAsync();
            return (false, 0);
        }

        // Rolle laden (nur wenn noch nicht im Speicher)
        if (CurrentRole == null)
        {
            var roleValue =
                await _session.GetItemAsync<int>(EmployeeSessionKeys.EmployeeRole);

            if (roleValue > 0)
                CurrentRole = (EmployeeRole)roleValue;
        }

        await TouchAsync();

        IsAuthenticated = true;
        return (true, employeeId);
    }

    // ----------------------------------------------------
    // ACTIVITY
    // ----------------------------------------------------
    public async Task TouchAsync()
    {
        await _session.SetItemAsync(
            EmployeeSessionKeys.LastActivityUtcTicks,
            DateTime.UtcNow.Ticks
        );
    }

    // ----------------------------------------------------
    // LOGOUT
    // ----------------------------------------------------
    public async Task SignOutAsync()
    {
        await _session.RemoveItemAsync(EmployeeSessionKeys.EmployeeId);
        await _session.RemoveItemAsync(EmployeeSessionKeys.EmployeeCode);
        await _session.RemoveItemAsync(EmployeeSessionKeys.EmployeeRole);
        await _session.RemoveItemAsync(EmployeeSessionKeys.LastActivityUtcTicks);
        await _session.RemoveItemAsync(EmployeeSessionKeys.ActiveServiceId);

        ClearState();
        NotifyStateChanged();
    }

    // ----------------------------------------------------
    // SERVICE-KONTEXT
    // ----------------------------------------------------
    public async Task SetActiveServiceAsync(int serviceId)
    {
        await _session.SetItemAsync(
            EmployeeSessionKeys.ActiveServiceId,
            serviceId
        );

        await TouchAsync();
    }

    public async Task<int?> GetActiveServiceAsync()
    {
        var serviceId =
            await _session.GetItemAsync<int>(EmployeeSessionKeys.ActiveServiceId);

        return serviceId > 0 ? serviceId : null;
    }

    // ----------------------------------------------------
    // INTERNAL
    // ----------------------------------------------------
    private void ClearState()
    {
        IsAuthenticated = false;
        CurrentRole = null;
    }

    private void NotifyStateChanged()
        => OnChange?.Invoke();
}
