namespace ConsulatTermine.Application.DTOs;

public class AvailableSlotDto
{
    public DateTime SlotStart { get; set; }   
    public int FreeCapacity { get; set; }     
    public bool IsAvailable => FreeCapacity > 0;
}
