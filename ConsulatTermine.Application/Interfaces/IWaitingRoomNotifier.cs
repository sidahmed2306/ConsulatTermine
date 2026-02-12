using ConsulatTermine.Domain.Enums;

namespace ConsulatTermine.Application.Interfaces;

public interface IWaitingRoomNotifier
{
    // 🔔 Zentrales Event: Grund + optional AppointmentId
    event Action<WaitingRoomChangeReason, int?> OnChanged;

    // 🔔 Haupt-Notify (neu)
    void Notify(WaitingRoomChangeReason reason, int? appointmentId = null);

    // 🔁 Fallback für alten Code (ohne Reason)
    void Notify();

    // 🧭 Arbeitsplatz-Verwaltung
    void SetWorkplace(int appointmentId, string office, string room);
    bool TryGetWorkplace(int appointmentId, out string office, out string room);
    void RemoveWorkplace(int appointmentId);
}
