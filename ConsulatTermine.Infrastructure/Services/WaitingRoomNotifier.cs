using System.Collections.Concurrent;
using ConsulatTermine.Application.Interfaces;
using ConsulatTermine.Domain.Enums;

namespace ConsulatTermine.Infrastructure.SignalR;

public class WaitingRoomNotifier : IWaitingRoomNotifier
{
    private readonly ConcurrentDictionary<int, (string Office, string Room)> _workplaces
        = new();

    // 🔔 Event mit Reason + AppointmentId
    public event Action<WaitingRoomChangeReason, int?>? OnChanged;

    // 🔔 Haupt-Notify
    public void Notify(WaitingRoomChangeReason reason, int? appointmentId = null)
    {
        OnChanged?.Invoke(reason, appointmentId);
    }

    // 🔁 Fallback für alten Code
    public void Notify()
    {
        Notify(WaitingRoomChangeReason.StatusOnly, null);
    }

    // 🧭 Arbeitsplatz setzen
    public void SetWorkplace(int appointmentId, string office, string room)
    {
        _workplaces[appointmentId] = (office, room);
    }

    // 🧭 Arbeitsplatz lesen
    public bool TryGetWorkplace(int appointmentId, out string office, out string room)
    {
        if (_workplaces.TryGetValue(appointmentId, out var value))
        {
            office = value.Office;
            room = value.Room;
            return true;
        }

        office = string.Empty;
        room = string.Empty;
        return false;
    }

    // 🧭 Arbeitsplatz entfernen
    public void RemoveWorkplace(int appointmentId)
    {
        _workplaces.TryRemove(appointmentId, out _);
    }
}
