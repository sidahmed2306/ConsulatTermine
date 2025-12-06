using ConsulatTermine.Domain.Entities;

public interface IEmployeeAssignmentService
{
    Task<List<EmployeeServiceAssignment>> GetAllAssignmentsAsync();

    Task<List<EmployeeServiceAssignment>> GetAssignmentsByEmployeeAsync(int employeeId);

    Task<bool> AddAssignmentAsync(int employeeId, int serviceId);

    Task<bool> RemoveAssignmentAsync(int employeeId, int serviceId);
}
