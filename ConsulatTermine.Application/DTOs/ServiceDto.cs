namespace ConsulatTermine.Application.DTOs;

public class ServiceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public string? NameEnglish { get; set; }

    public string? NameArabic { get; set; }

    public string Description { get; set; } = string.Empty;

    public string? DescriptionEnglish { get; set; }

    public string? DescriptionArabic { get; set; }

    public int? CapacityPerSlot { get; set; }

    public int SlotDurationMinutes { get; set; }
    public string Floor { get; set; } = string.Empty;

}
