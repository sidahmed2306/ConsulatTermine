using ConsulatTermine.Application.DTOs;
using ConsulatTermine.Domain.Entities;

namespace ConsulatTermine.Application.Interfaces;

public interface IEmployeeService
{
    Task<Employee> CreateEmployeeAsync(EmployeeDto dto);
    Task<Employee> UpdateEmployeeAsync(int id, EmployeeDto dto);
    Task<Employee?> GetEmployeeByIdAsync(int id);
    Task<List<Employee>> GetAllEmployeesAsync();
    Task AssignServiceAsync(int employeeId, int serviceId);
    Task<bool> DeleteEmployeeAsync(int id);


}
