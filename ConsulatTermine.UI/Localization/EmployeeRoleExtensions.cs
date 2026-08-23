using ConsulatTermine.Domain.Enums;
using ConsulatTermine.UI.Resources;
using Microsoft.Extensions.Localization;

namespace ConsulatTermine.UI.Localization;

/// <summary>
/// Beschriftet eine Rolle in der Sprache der Oberflaeche.
/// </summary>
public static class EmployeeRoleExtensions
{
    /// <summary>
    /// Gibt die uebersetzte Bezeichnung der Rolle zurueck.
    /// </summary>
    /// <remarks>
    /// Der Enum-Name ist ein technischer Bezeichner und wird nie unmittelbar
    /// angezeigt. Die Schluessel folgen dem Muster <c>Role.&lt;Enum-Name&gt;</c>,
    /// damit eine neue Rolle beim Uebersetzen sofort auffaellt.
    /// </remarks>
    public static string ToDisplayText(this EmployeeRole role, IStringLocalizer<AdministrationTexts> texts)
    {
        ArgumentNullException.ThrowIfNull(texts);

        return texts[$"Role.{role}"];
    }
}
