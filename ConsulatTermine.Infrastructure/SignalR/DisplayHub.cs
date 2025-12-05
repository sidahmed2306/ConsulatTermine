using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.SignalR
{
    // Alle Display-Clients (öffentlicher Monitor) müssen diese Methoden empfangen können
    public interface IDisplayClient
    {
        Task CitizenCalled(int appointmentId, string ticketNumber, string serviceName, string counterName);
    }

    // Der Hub selbst – nimmt keine Logik auf, nur Transportkanal
    public class DisplayHub : Hub<IDisplayClient>
    {
    }
}
