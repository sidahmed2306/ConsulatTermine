namespace ConsulatTermine.Application.DTOs.Booking;

/// <summary>
/// Ein Termin, wie er in der Bestaetigungsmail erscheint.
/// </summary>
public class BookingEmailAppointmentDto
{
    public string PersonFullName { get; set; } = string.Empty;

    /// <summary>
    /// Bezeichnung des Service in der Amtssprache des Standorts. Pflichtangabe und
    /// Rueckfallebene fuer nicht gepflegte Uebersetzungen.
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// Englische Bezeichnung des Service, sofern gepflegt.
    /// </summary>
    public string? ServiceNameEnglish { get; set; }

    /// <summary>
    /// Arabische Bezeichnung des Service, sofern gepflegt.
    /// </summary>
    public string? ServiceNameArabic { get; set; }

    public DateTime DateTime { get; set; }
}
