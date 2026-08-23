using ConsulatTermine.Domain.Enums; // falls bei dir anders: Namespace anpassen
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.SignalR;

// Clients für Mitarbeiter-Arbeitsplätze (NEXT-Button, Status-Anzeige)
public interface IEmployeeClient
{
    Task StatusUpdated(int appointmentId, AppointmentStatus newStatus);
}

// Der Hub selbst – ebenfalls nur Transportkanal
public class EmployeeHub : Hub<IEmployeeClient>
{
}
