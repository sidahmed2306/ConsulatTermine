using ConsulatTermine.Domain.Enums;
using ConsulatTermine.UI.Resources;
using Microsoft.Extensions.Localization;

namespace ConsulatTermine.UI.Localization;

/// <summary>
/// Beschriftet den Status eines Termins in der Sprache der Oberflaeche.
/// </summary>
public static class AppointmentStatusExtensions
{
    /// <summary>
    /// Gibt die uebersetzte Bezeichnung des Status zurueck.
    /// </summary>
    /// <remarks>
    /// Der Enum-Name wird nie unmittelbar angezeigt: er ist ein technischer Bezeichner
    /// und waere in jeder Sprache englisch. Die Schluessel folgen dem Muster
    /// <c>Status.&lt;Enum-Name&gt;</c>, damit ein neuer Statuswert beim Uebersetzen
    /// sofort auffaellt.
    /// </remarks>
    public static string ToDisplayText(this AppointmentStatus status, IStringLocalizer<CommonTexts> texts)
    {
        ArgumentNullException.ThrowIfNull(texts);

        return texts[$"Status.{status}"];
    }
}
