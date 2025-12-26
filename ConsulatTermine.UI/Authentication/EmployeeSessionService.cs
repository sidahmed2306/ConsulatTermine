using Blazored.SessionStorage;

namespace ConsulatTermine.UI.Authentication;

public class EmployeeSessionService
{
    private readonly ISessionStorageService _session;

    // ⏱ Timeout (aktuell 1 Minute, später z. B. 60 / 120)
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(1);

    // 🔔 State-Change-Event (WICHTIG)
    public event Action? OnChange;

    public bool IsAuthenticated { get; private set; }

    public EmployeeSessionService(ISessionStorageService session)
    {
        _session = session;
    }

    // ----------------------------------------------------
    // LOGIN
    // ----------------------------------------------------
    public async Task SignInAsync(int employeeId, string employeeCode)
    {
        await _session.SetItemAsync(EmployeeSessionKeys.EmployeeId, employeeId);
        await _session.SetItemAsync(EmployeeSessionKeys.EmployeeCode, employeeCode);

        await TouchAsync();

        IsAuthenticated = true;
        NotifyStateChanged();
    }

    // ----------------------------------------------------
    // AUTH-CHECK + TIMEOUT
    // ----------------------------------------------------
    public async Task<(bool IsAuthenticated, int EmployeeId)> EnsureAuthenticatedAsync()
    {
        var employeeId = await _session.GetItemAsync<int>(EmployeeSessionKeys.EmployeeId);
        if (employeeId <= 0)
        {
            IsAuthenticated = false;
            return (false, 0);
        }

        var ticks = await _session.GetItemAsync<long>(EmployeeSessionKeys.LastActivityUtcTicks);
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
        await _session.RemoveItemAsync(EmployeeSessionKeys.LastActivityUtcTicks);
        await _session.RemoveItemAsync(EmployeeSessionKeys.ActiveServiceId);

        IsAuthenticated = false;
        NotifyStateChanged();
    }

    // ----------------------------------------------------
    // SERVICE-KONTEXT
    // ----------------------------------------------------
    public async Task SetActiveServiceAsync(int serviceId)
    {
        await _session.SetItemAsync(EmployeeSessionKeys.ActiveServiceId, serviceId);
        await TouchAsync();
    }

    public async Task<int?> GetActiveServiceAsync()
    {
        var serviceId = await _session.GetItemAsync<int>(EmployeeSessionKeys.ActiveServiceId);
        return serviceId > 0 ? serviceId : null;
    }

    // ----------------------------------------------------
    // INTERNAL
    // ----------------------------------------------------
    private void NotifyStateChanged()
        => OnChange?.Invoke();
}
