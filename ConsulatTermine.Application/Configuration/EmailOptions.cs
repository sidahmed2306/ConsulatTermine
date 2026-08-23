using System.ComponentModel.DataAnnotations;

namespace ConsulatTermine.Application.Configuration;

/// <summary>
/// Zugangsdaten und Adressen des SMTP-Postausgangs.
/// Benutzername und Passwort stammen aus dem Secret Store der jeweiligen Umgebung,
/// niemals aus einer eingecheckten Konfigurationsdatei.
/// </summary>
public sealed class EmailOptions
{
    public const string SectionName = "Email";

    [Required(AllowEmptyStrings = false)]
    public string SmtpServer { get; set; } = string.Empty;

    [Range(1, 65535)]
    public int Port { get; set; } = 587;

    public bool UseSsl { get; set; } = true;

    [Required(AllowEmptyStrings = false)]
    public string Username { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Password { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [EmailAddress]
    public string FromEmail { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string FromName { get; set; } = string.Empty;
}
