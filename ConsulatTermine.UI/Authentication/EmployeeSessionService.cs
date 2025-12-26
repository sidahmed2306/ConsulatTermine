using Blazored.SessionStorage;

namespace ConsulatTermine.UI.Authentication;

public class EmployeeSessionService
{
    private readonly ISessionStorageService _session;

    // 60 Minuten Timeout (kannst du später auf 120 setzen)
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(1);

    public EmployeeSessionService(ISessionStorageService session)
    {
        _session = session;
    }

    /// <summary>
    /// Wird beim erfolgreichen Login aufgerufen.
    /// </summary>
    public async Task SignInAsync(int employeeId, string employeeCode)
    {
        await _session.SetItemAsync(EmployeeSessionKeys.EmployeeId, employeeId);
        await _session.SetItemAsync(EmployeeSessionKeys.EmployeeCode, employeeCode);
        await TouchAsync();
    }

    /// <summary>
    /// Prüft: eingeloggt + nicht abgelaufen. Falls ok → aktualisiert LastActivity.
    /// </summary>
    public async Task<(bool IsAuthenticated, int EmployeeId)> EnsureAuthenticatedAsync()
    {
        var employeeId = await _session.GetItemAsync<int>(EmployeeSessionKeys.EmployeeId);

        if (employeeId <= 0)
            return (false, 0);

        // Timeout prüfen
        var ticks = await _session.GetItemAsync<long>(EmployeeSessionKeys.LastActivityUtcTicks);

        if (ticks <= 0)
        {
            // Kein Activity-Stempel → als abgelaufen behandeln (sicher)
            await SignOutAsync();
            return (false, 0);
        }

        var last = new DateTime(ticks, DateTimeKind.Utc);
        if (DateTime.UtcNow - last > Timeout)
        {
            await SignOutAsync();
            return (false, 0);
        }

        // Aktivität erneuern
        await TouchAsync();

        return (true, employeeId);
    }

    public async Task TouchAsync()
    {
        await _session.SetItemAsync(EmployeeSessionKeys.LastActivityUtcTicks, DateTime.UtcNow.Ticks);
    }

    public async Task SignOutAsync()
    {
        await _session.RemoveItemAsync(EmployeeSessionKeys.EmployeeId);
        await _session.RemoveItemAsync(EmployeeSessionKeys.EmployeeCode);
        await _session.RemoveItemAsync(EmployeeSessionKeys.LastActivityUtcTicks);
    }

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

}
