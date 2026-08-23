namespace ConsulatTermine.Application.DTOs;

public class WaitingRoomCallDto
{
    public int AppointmentId { get; set; }
    public string FullName { get; set; } = "";
    public DateTime Date { get; set; }

    public string ServiceName { get; set; } = "";
    public string? Floor { get; set; }

    public string? OfficeName { get; set; }
    public string? RoomName { get; set; }
}
