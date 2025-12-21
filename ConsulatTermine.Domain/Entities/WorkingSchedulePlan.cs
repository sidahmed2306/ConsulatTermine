namespace ConsulatTermine.Domain.Entities;

public class WorkingSchedulePlan
{
    public int Id { get; set; }                 // PK

    public int ServiceId { get; set; }          // FK -> Service.Id
    public Service Service { get; set; } = null!;

    public DateOnly ValidFromDate { get; set; } // date
    public DateOnly ValidToDate { get; set; }   // date (inkl.)

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
