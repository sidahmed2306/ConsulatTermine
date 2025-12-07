using System.Threading.Tasks;
using ConsulatTermine.Application.DTOs;

namespace ConsulatTermine.Application.Interfaces
{
    public interface IWorkingScheduleService
    {
        Task<bool> GenerateScheduleAsync(WorkingScheduleRequestDto request);
    }
}
