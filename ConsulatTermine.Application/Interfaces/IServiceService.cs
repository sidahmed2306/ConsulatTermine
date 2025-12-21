using ConsulatTermine.Application.DTOs;
using ConsulatTermine.Application.ViewModels;
using ConsulatTermine.Domain.Entities;

namespace ConsulatTermine.Application.Interfaces;

public interface IServiceService
{
    Task<Service> CreateServiceAsync(ServiceDto dto);
    Task<Service> UpdateServiceAsync(int id, ServiceDto dto);
    Task<bool> DeleteServiceAsync(int id);
    Task<Service?> GetServiceByIdAsync(int id);
    Task<List<Service>> GetAllServicesAsync();
     Task<ServiceDto> GetByIdAsync(int id);
    Task<List<SlotViewModel>> GetAvailableSlotsForServiceAsync(int serviceId, DateOnly date);
    Task<List<AvailableSlotDto>> GetAvailableSlotDtosAsync(
    int serviceId,
    DateTime date);

}
