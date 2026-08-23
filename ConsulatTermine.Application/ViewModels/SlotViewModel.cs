using System.Globalization;

namespace ConsulatTermine.Application.ViewModels;

/// <summary>
/// Ein Terminfenster in der Ansicht der Terminauswahl.
/// </summary>
public sealed class SlotViewModel
{
    public DateTime DateTime { get; set; }

    public int FreeSlots { get; set; }

    public int BookedSlots { get; set; }

    /// <summary>
    /// Uhrzeit in fester Schreibweise. Bewusst invariant: die Anwendung laeuft auch
    /// unter ar-DZ, wo kulturabhaengige Formatierung andere Ziffernzeichen erzeugt und
    /// der Wert dann nicht mehr zum gespeicherten Termin passt.
    /// </summary>
    public string DisplayTime => DateTime.ToString("HH:mm", CultureInfo.InvariantCulture);

    public string DisplayDate => DateTime.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
}
