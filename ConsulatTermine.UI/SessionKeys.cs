namespace ConsulatTermine.UI;

/// <summary>
/// Schluessel des Browser-Session-Speichers.
/// Dort liegt ausschliesslich Bedienzustand: der Fortschritt des Buchungs-Wizards und
/// der gewaehlte Arbeitsplatz eines Mitarbeiters. Niemals Identitaet oder Berechtigungen —
/// der Benutzer kann diese Werte im Browser frei aendern.
/// </summary>
public static class SessionKeys
{
    // Öffentlicher Buchungs-Wizard
    public const string PersonCount = "personCount";
    public const string ServiceAssignments = "serviceAssignments";
    public const string SelectedServiceSlots = "selectedServiceSlots";
    public const string BookingReference = "bookingReference";
    public const string MainPersonEmail = "mainPersonEmail";

    // Arbeitsplatz des Mitarbeiters
    public const string ActiveServiceId = "ActiveServiceId";
    public const string OfficeName = "OfficeName";
    public const string RoomName = "RoomName";
    public const string ServiceName = "ServiceName";
}
