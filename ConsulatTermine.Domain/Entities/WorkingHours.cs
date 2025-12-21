namespace ConsulatTermine.Domain.Entities;

public class WorkingHours
{
    public int Id { get; set; }

    public int ServiceId { get; set; }
    public Service? Service { get; set; }
    
     public int WorkingSchedulePlanId { get; set; }
    public WorkingSchedulePlan WorkingSchedulePlan { get; set; } = null!;

    public DayOfWeek Day { get; set; }

    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}
