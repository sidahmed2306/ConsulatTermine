namespace ConsulatTermine.Application.DTOs.Booking;

public class BookingEmailAppointmentDto
{
    public string PersonFullName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public DateTime DateTime { get; set; }
}
