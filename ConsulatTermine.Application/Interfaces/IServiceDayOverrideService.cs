using ConsulatTermine.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsulatTermine.Application.Interfaces
{
    public interface IServiceDayOverrideService
    {
        Task<List<ServiceDayOverride>> GetAllAsync();
        Task<List<ServiceDayOverride>> GetByServiceAsync(int serviceId);
        Task<ServiceDayOverride?> GetByIdAsync(int id);

        Task<ServiceDayOverride> CreateAsync(ServiceDayOverride model);
        Task<ServiceDayOverride> UpdateAsync(int id, ServiceDayOverride model);
        Task<bool> DeleteAsync(int id);
    }
}
