using System.Globalization;
using System.Resources;

namespace ConsulatTermine.Infrastructure.Resources;

/// <summary>
/// Texte des Schriftverkehrs in den ausgelieferten Sprachen.
/// </summary>
/// <remarks>
/// Bewusst ueber <see cref="ResourceManager"/> mit ausdruecklich uebergebener Kultur
/// und nicht ueber <c>IStringLocalizer</c>: Die Sprache eines Schreibens ist die
/// Sprache des Empfaengers und nicht die der Sitzung, die den Versand ausloest. Sagt
/// ein Mitarbeiter in deutscher Oberflaeche einen Termin ab, der auf Arabisch gebucht
/// wurde, geht die Absage auf Arabisch hinaus.
///
/// <c>EmailTexts.resx</c> ist Deutsch und zugleich die Rueckfallebene,
/// <c>EmailTexts.en.resx</c> und <c>EmailTexts.ar.resx</c> ergaenzen Englisch und Arabisch.
/// </remarks>
internal static class EmailTexts
{
    private static readonly ResourceManager _resources =
        new("ConsulatTermine.Infrastructure.Resources.EmailTexts", typeof(EmailTexts).Assembly);

    /// <summary>
    /// Liest einen Text in der uebergebenen Kultur. Fehlt der Eintrag, bleibt der
    /// Schluessel stehen: eine Luecke im Schreiben faellt so beim Pruefen auf,
    /// statt unbemerkt eine leere Zeile zu erzeugen.
    /// </summary>
    public static string Get(string key, CultureInfo culture) =>
        _resources.GetString(key, culture) ?? key;

    /// <summary>
    /// Liest einen Text mit Platzhaltern und setzt die Werte in der Kultur des
    /// Empfaengers ein.
    /// </summary>
    public static string Format(string key, CultureInfo culture, params object?[] arguments) =>
        string.Format(culture, Get(key, culture), arguments);
}
