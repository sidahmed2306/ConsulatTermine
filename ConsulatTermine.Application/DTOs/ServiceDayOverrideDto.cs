namespace ConsulatTermine.Application.DTOs
{
    public class ServiceDayOverrideDto
    {
        public int ServiceId { get; set; }
        public DateTime? Date { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public bool IsClosed { get; set; } = false;
        public int? CapacityPerSlotOverride { get; set; }
    }
}
