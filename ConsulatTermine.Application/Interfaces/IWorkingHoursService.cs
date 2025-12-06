using ConsulatTermine.Domain.Entities;

public interface IWorkingHoursService
{
    Task<List<WorkingHours>> GetAllAsync();
    Task<List<WorkingHours>> GetByServiceAsync(int serviceId);
    Task<WorkingHours?> GetByIdAsync(int id);

    Task<WorkingHours> CreateAsync(WorkingHours model);
    Task<WorkingHours> UpdateAsync(int id, WorkingHours model);
    Task<bool> DeleteAsync(int id);
}
