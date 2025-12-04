namespace ConsulatTermine.Application.DTOs;

public class ServiceDto
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int CapacityPerSlot { get; set; }

    public int SlotDurationMinutes { get; set; }
}