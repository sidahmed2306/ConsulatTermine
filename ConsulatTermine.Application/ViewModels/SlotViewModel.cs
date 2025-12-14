namespace ConsulatTermine.Application.ViewModels
{
    public class SlotViewModel
{
    public DateTime DateTime { get; set; }
    public int FreeSlots { get; set; }
    public int BookedSlots { get; set; }
    public string DisplayTime => DateTime.ToString("HH:mm");
    public string DisplayDate => DateTime.ToString("dd.MM.yyyy");
}

}
