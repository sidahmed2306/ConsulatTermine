namespace ConsulatTermine.Application.DTOs;

public class EmployeeLoginResultDto
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }

    public int? EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }

    public bool MustChangePassword { get; set; }
}