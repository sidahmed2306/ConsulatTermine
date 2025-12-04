using ConsulatTermine.Domain.Enums;

namespace ConsulatTermine.Domain.Entities;

public class Appointment
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public int ServiceId { get; set; }
    public Service? Service { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Optional für spätere Analyse:
    public DateTime? CheckedInAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
