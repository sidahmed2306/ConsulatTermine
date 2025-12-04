namespace ConsulatTermine.Domain.Entities;

public class Holiday
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime Date { get; set; }
}
