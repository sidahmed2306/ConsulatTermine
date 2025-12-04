
namespace ConsulatTermine.Domain.Entities;

public class Employee
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    // Verbindung zu IdentityUser
    public string? IdentityUserId { get; set; }

    // Mitarbeiter kann mehrere Services bedienen
    public List<EmployeeServiceAssignment> AssignedServices { get; set; } = new();
}
