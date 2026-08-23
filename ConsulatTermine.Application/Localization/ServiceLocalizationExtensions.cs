using ConsulatTermine.Application.DTOs;
using ConsulatTermine.Domain.Entities;

namespace ConsulatTermine.Application.Localization;

/// <summary>
/// Liest Bezeichnung und Beschreibung eines Service in der Sprache der Anzeige.
/// </summary>
public static class ServiceLocalizationExtensions
{
    /// <summary>
    /// Bezeichnung des Service in der uebergebenen Kultur.
    /// </summary>
    public static string NameFor(this Service service, string? cultureCode)
    {
        ArgumentNullException.ThrowIfNull(service);

        return LocalizedText.ForCulture(
            service.Name,
            service.NameEnglish,
            service.NameArabic,
            cultureCode);
    }

    /// <summary>
    /// Beschreibung des Service in der uebergebenen Kultur.
    /// </summary>
    public static string DescriptionFor(this Service service, string? cultureCode)
    {
        ArgumentNullException.ThrowIfNull(service);

        return LocalizedText.ForCulture(
            service.Description,
            service.DescriptionEnglish,
            service.DescriptionArabic,
            cultureCode);
    }

    /// <summary>
    /// Bezeichnung des Service in der uebergebenen Kultur.
    /// </summary>
    public static string NameFor(this ServiceDto service, string? cultureCode)
    {
        ArgumentNullException.ThrowIfNull(service);

        return LocalizedText.ForCulture(
            service.Name,
            service.NameEnglish,
            service.NameArabic,
            cultureCode);
    }

    /// <summary>
    /// Beschreibung des Service in der uebergebenen Kultur.
    /// </summary>
    public static string DescriptionFor(this ServiceDto service, string? cultureCode)
    {
        ArgumentNullException.ThrowIfNull(service);

        return LocalizedText.ForCulture(
            service.Description,
            service.DescriptionEnglish,
            service.DescriptionArabic,
            cultureCode);
    }
}
