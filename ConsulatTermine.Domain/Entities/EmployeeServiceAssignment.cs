namespace ConsulatTermine.Domain.Entities;

public class EmployeeServiceAssignment
{
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public int ServiceId { get; set; }
    public Service? Service { get; set; }
}
