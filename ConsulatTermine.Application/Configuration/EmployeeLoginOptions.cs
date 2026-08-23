using System.ComponentModel.DataAnnotations;

namespace ConsulatTermine.Application.Configuration;

/// <summary>
/// Grenzwerte der Mitarbeiteranmeldung.
/// </summary>
public sealed class EmployeeLoginOptions
{
    public const string SectionName = "EmployeeLogin";

    /// <summary>
    /// Fehlversuche, nach denen das Konto voruebergehend gesperrt wird.
    /// </summary>
    [Range(1, 100)]
    public int MaxFailedAttempts { get; set; } = 5;

    /// <summary>
    /// Dauer der Sperre nach Erreichen von <see cref="MaxFailedAttempts"/>.
    /// </summary>
    [Range(typeof(TimeSpan), "00:00:30", "24:00:00")]
    public TimeSpan LockoutDuration { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Leerlaufzeit, nach der die Anmeldung ungueltig wird.
    /// </summary>
    [Range(typeof(TimeSpan), "00:01:00", "24:00:00")]
    public TimeSpan SessionTimeout { get; set; } = TimeSpan.FromMinutes(120);

    /// <summary>
    /// Gueltigkeitsdauer eines Links zum Zuruecksetzen des Passworts.
    /// </summary>
    [Range(typeof(TimeSpan), "00:05:00", "24:00:00")]
    public TimeSpan PasswordResetTokenLifetime { get; set; } = TimeSpan.FromHours(1);
}
