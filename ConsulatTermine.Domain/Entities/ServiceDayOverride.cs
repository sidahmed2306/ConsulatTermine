namespace ConsulatTermine.Domain.Entities;

public class ServiceDayOverride
{
    public int Id { get; set; }

    public int ServiceId { get; set; }
    public Service? Service { get; set; }

    public DateTime Date { get; set; }

    public bool IsClosed { get; set; } = false;

    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }

    public int? CapacityPerSlot { get; set; }
}
