using System.Globalization;

namespace ConsulatTermine.Application.Localization;

/// <summary>
/// Verbindliche Liste der Sprachen, in denen die Anwendung ausgeliefert wird.
/// Sie ist die einzige Quelle fuer Sprachmenue, Request-Lokalisierung,
/// Schreibrichtung und Sprache des Schriftverkehrs.
/// </summary>
public static class SupportedLanguages
{
    /// <summary>
    /// Sprache, die gilt, solange der Besucher keine andere gewaehlt hat und der
    /// Browser keine unterstuetzte Sprache anbietet.
    /// </summary>
    public const string DefaultCultureCode = "de-DE";

    private static readonly SupportedLanguage _german = new("de-DE", "Deutsch", IsRightToLeft: false);

    /// <summary>
    /// Reihenfolge im Sprachmenue: Amtssprache des Standorts, internationale
    /// Verkehrssprache, Amtssprache des Herkunftslandes.
    /// </summary>
    public static IReadOnlyList<SupportedLanguage> All { get; } =
    [
        _german,
        new SupportedLanguage("en-US", "English", IsRightToLeft: false),
        new SupportedLanguage("ar-DZ", "العربية", IsRightToLeft: true)
    ];

    /// <summary>
    /// Standardsprache als vollstaendiger Eintrag.
    /// </summary>
    public static SupportedLanguage Default => _german;

    /// <summary>
    /// Kulturen fuer <c>RequestLocalizationOptions</c>.
    /// </summary>
    public static IReadOnlyList<CultureInfo> Cultures { get; } =
        [.. All.Select(language => new CultureInfo(language.CultureCode))];

    /// <summary>
    /// Prueft, ob genau diese Kultur ausgeliefert wird. Ein Sprachanteil ohne Land,
    /// zum Beispiel <c>ar</c>, gilt hier bewusst nicht als unterstuetzt; dafuer
    /// existiert <see cref="Resolve"/>.
    /// </summary>
    public static bool IsSupported(string? cultureCode) =>
        All.Any(language =>
            string.Equals(language.CultureCode, cultureCode, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Ordnet eine beliebige Kulturangabe einer ausgelieferten Sprache zu.
    /// Zuerst exakt, danach ueber den Sprachanteil, damit auch Angaben wie
    /// <c>ar</c>, <c>ar-MA</c> oder <c>en-GB</c> aus dem Browser greifen.
    /// Unbekannte oder leere Angaben ergeben die Standardsprache.
    /// </summary>
    public static SupportedLanguage Resolve(string? cultureCode)
    {
        if (string.IsNullOrWhiteSpace(cultureCode))
        {
            return Default;
        }

        var candidate = cultureCode.Trim();

        var exactMatch = All.FirstOrDefault(language =>
            string.Equals(language.CultureCode, candidate, StringComparison.OrdinalIgnoreCase));

        if (exactMatch is not null)
        {
            return exactMatch;
        }

        var twoLetterCandidate = candidate.Length >= 2 ? candidate[..2] : candidate;

        var languageMatch = All.FirstOrDefault(language =>
            string.Equals(language.TwoLetterCode, twoLetterCandidate, StringComparison.OrdinalIgnoreCase));

        return languageMatch ?? Default;
    }

    /// <summary>
    /// Kulturname einer beliebigen Angabe in der ausgelieferten Schreibweise.
    /// </summary>
    public static string Normalize(string? cultureCode) => Resolve(cultureCode).CultureCode;

    /// <summary>
    /// Kultur fuer die Formatierung von Datum, Uhrzeit und Zahlen sowie fuer den
    /// Zugriff auf Ressourcen. Fuer den Schriftverkehr wird die Kultur explizit
    /// uebergeben, weil sie sich von der Kultur der ausloesenden Sitzung
    /// unterscheiden kann.
    /// </summary>
    public static CultureInfo ToCultureInfo(string? cultureCode) =>
        new(Normalize(cultureCode));
}
