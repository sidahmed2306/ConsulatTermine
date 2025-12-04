using ConsulatTermine.Application.DTOs;
using ConsulatTermine.Domain.Entities;

namespace ConsulatTermine.Application.Interfaces;

public interface IServiceService
{
    Task<Service> CreateServiceAsync(ServiceDto dto);
    Task<Service> UpdateServiceAsync(int id, ServiceDto dto);
    Task<bool> DeleteServiceAsync(int id);
    Task<Service?> GetServiceByIdAsync(int id);
    Task<List<Service>> GetAllServicesAsync();
}
