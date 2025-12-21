using ConsulatTermine.Application.DTOs.WorkingSchedulePlan;

namespace ConsulatTermine.Application.Interfaces;

public interface IWorkingSchedulePlanService
{
    Task<WorkingSchedulePlanDto> SaveAsync(WorkingSchedulePlanDto dto);

    Task<List<WorkingSchedulePlanDto>> GetByServiceAsync(int serviceId);

    Task<WorkingSchedulePlanDto?> GetByIdAsync(int id);

    Task<bool> DeleteAsync(int id);

    Task<bool> SetActiveAsync(int id);
}
