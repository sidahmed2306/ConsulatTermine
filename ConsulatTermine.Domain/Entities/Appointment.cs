using ConsulatTermine.Domain.Enums;

namespace ConsulatTermine.Domain.Entities;

public class Appointment
{
    public int Id { get; set; }

    /// <summary>
    /// Vollständiger Name der Person, die zu diesem Termin kommt.
    /// Beim Hauptbucher entspricht das dem Hauptkontakt,
    /// bei Begleitpersonen deren eigenem Namen.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// E-Mail der Person. Für Begleitpersonen optional, 
    /// für den Hauptbucher i. d. R. erforderlich.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Startzeit des Termins (inkl. Datum).
    /// Beispiel: 2025-12-12 08:45:00
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Service, zu dem dieser Termin gehört (Pass, Visa, …).
    /// </summary>
    public int ServiceId { get; set; }
    public Service? Service { get; set; }

    /// <summary>
    /// Status im Ablauf (gebucht, eingecheckt, in Bearbeitung, abgeschlossen, storniert).
    /// </summary>
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CheckedInAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // ---------------------------------------------------------------------
    // NEU: Gruppierung & Personen-Infos für Mehrpersonen-/Mehrservice-Buchungen
    // ---------------------------------------------------------------------

    /// <summary>
    /// Buchungsreferenz, um mehrere Termine (mehrere Services / Personen)
    /// einer gemeinsamen Online-Buchung zuzuordnen.
    ///
    /// Alle Termine, die in einem Vorgang gebucht werden,
    /// bekommen denselben Wert (z. B. ein GUID-String).
    /// Für alte Daten kann dieser Wert leer bleiben.
    /// </summary>
    public string BookingReference { get; set; } = string.Empty;

    /// <summary>
    /// Laufende Nummer der Person innerhalb dieser Buchung.
    /// 1 = Hauptbucher, 2..n = Begleitpersonen.
    /// </summary>
    public int PersonIndex { get; set; } = 1;

    /// <summary>
    /// Kennzeichnet, ob diese Person der Hauptkontakt ist
    /// (Empfänger der Bestätigungs-E-Mails usw.).
    /// </summary>
    public bool IsMainPerson { get; set; } = false;

    /// <summary>
/// Telefonnummer der Person (aus Formular).
/// </summary>
public string PhoneNumber { get; set; } = string.Empty;

/// <summary>
/// Geburtsdatum der Person (aus Formular).
/// </summary>
public DateTime? DateOfBirth { get; set; }

/// <summary>
/// Sicherheits-Token für die Terminabsage über E-Mail-Link.
/// </summary>
public string? CancelToken { get; set; }

/// <summary>
/// Ablaufdatum des Cancel-Links.
/// Nach diesem Zeitpunkt ist eine Absage über den Link nicht mehr möglich.
/// </summary>
public DateTime? CancelTokenExpiresAt { get; set; }


}
