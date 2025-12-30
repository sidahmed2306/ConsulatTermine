using ConsulatTermine.Domain.Enums;

namespace ConsulatTermine.Application.DTOs;

public class EmployeeDto
{
public string FirstName { get; set; } = string.Empty;

public string LastName { get; set; } = string.Empty;

public string Email { get; set; } = string.Empty;

public DateTime? DateOfBirth { get; set; }
public EmployeeRole Role { get; set; } = EmployeeRole.Employee;

}