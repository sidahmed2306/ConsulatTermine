using System.Globalization;
using System.Resources;

namespace ConsulatTermine.UI.Resources;

/// <summary>
/// Meldungen der Eingabepruefung in Formularen.
/// </summary>
/// <remarks>
/// Die Klasse traegt bewusst statische Eigenschaften statt nur den Namen des
/// Ressourcensatzes: Die Attribute aus <c>System.ComponentModel.DataAnnotations</c>
/// nehmen ueber <c>ErrorMessageResourceType</c> und <c>ErrorMessageResourceName</c>
/// nur eine statische Eigenschaft entgegen, weil ein Attributwert eine Konstante
/// sein muss. Der gleiche Satz ist ausserdem ueber
/// <c>IStringLocalizer&lt;ValidationTexts&gt;</c> erreichbar.
///
/// <c>ValidationTexts.resx</c> ist Deutsch und zugleich die Rueckfallebene,
/// <c>ValidationTexts.en.resx</c> und <c>ValidationTexts.ar.resx</c> ergaenzen
/// Englisch und Arabisch.
/// </remarks>
public sealed class ValidationTexts
{
    private static readonly ResourceManager _resources =
        new(typeof(ValidationTexts).FullName!, typeof(ValidationTexts).Assembly);

    public static string FirstNameRequired => Get(nameof(FirstNameRequired));

    public static string LastNameRequired => Get(nameof(LastNameRequired));

    public static string DateOfBirthRequired => Get(nameof(DateOfBirthRequired));

    public static string EmailRequired => Get(nameof(EmailRequired));

    public static string EmailInvalid => Get(nameof(EmailInvalid));

    public static string PhoneRequired => Get(nameof(PhoneRequired));

    public static string EmployeeCodeRequired => Get(nameof(EmployeeCodeRequired));

    public static string PasswordRequired => Get(nameof(PasswordRequired));

    public static string NameRequired => Get(nameof(NameRequired));

    /// <summary>
    /// Liest die Meldung in der Sprache der laufenden Anfrage. Fehlt der Eintrag,
    /// bleibt der Schluessel stehen: eine leere Fehlermeldung waere schlimmer als
    /// eine technische, weil das Formular dann ohne erkennbaren Grund abweist.
    /// </summary>
    private static string Get(string name) =>
        _resources.GetString(name, CultureInfo.CurrentUICulture) ?? name;
}
