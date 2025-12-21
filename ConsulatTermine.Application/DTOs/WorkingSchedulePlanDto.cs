namespace ConsulatTermine.Application.DTOs.WorkingSchedulePlan;

public class WorkingSchedulePlanDto
{
    public int Id { get; set; }

    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;

    public DateOnly ValidFromDate { get; set; }
    public DateOnly ValidToDate { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
