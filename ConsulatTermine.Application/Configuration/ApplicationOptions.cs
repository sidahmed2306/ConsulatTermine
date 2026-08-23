using System.ComponentModel.DataAnnotations;

namespace ConsulatTermine.Application.Configuration;

/// <summary>
/// Allgemeine Anwendungseinstellungen. Werden beim Start validiert, damit fehlende
/// Konfiguration zu einem klaren Startfehler fuehrt und nicht zu spaetem Fehlverhalten.
/// </summary>
public sealed class ApplicationOptions
{
    public const string SectionName = "Application";

    /// <summary>
    /// Oeffentlich erreichbare Basisadresse der Anwendung. Grundlage aller Links in
    /// ausgehenden E-Mails, zum Beispiel des Absage-Links.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    [Url]
    public string BaseUrl { get; set; } = string.Empty;
}
