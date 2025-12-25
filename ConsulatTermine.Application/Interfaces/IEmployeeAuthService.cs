using ConsulatTermine.Application.DTOs;

namespace ConsulatTermine.Application.Interfaces;

public interface IEmployeeAuthService
{
    Task<EmployeeLoginResultDto> LoginAsync(string employeeCode, string password);

    Task<bool> ChangePasswordAsync(int employeeId, string newPassword);
}