public class WorkingHoursDto
{
    public int ServiceId { get; set; }
    public HashSet<DayOfWeek> SelectedDays { get; set; } = new();
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
}
