namespace ConsulatTermine.Application.Localization;

/// <summary>
/// Eine von der Oberflaeche unterstuetzte Sprache.
/// </summary>
/// <param name="CultureCode">Kultur im Format <c>xx-YY</c>, zum Beispiel <c>de-DE</c>.</param>
/// <param name="NativeName">Bezeichnung in der Sprache selbst, damit sie im Sprachmenue
/// auch dann lesbar bleibt, wenn die Oberflaeche gerade eine andere Sprache zeigt.</param>
/// <param name="IsRightToLeft">Schreibrichtung von rechts nach links.</param>
public sealed record SupportedLanguage(
    string CultureCode,
    string NativeName,
    bool IsRightToLeft)
{
    /// <summary>
    /// Zweibuchstabiger Sprachanteil der Kultur, zum Beispiel <c>de</c> zu <c>de-DE</c>.
    /// </summary>
    public string TwoLetterCode => CultureCode[..2];

    /// <summary>
    /// Wert fuer das HTML-Attribut <c>dir</c>.
    /// </summary>
    public string TextDirection => IsRightToLeft ? "rtl" : "ltr";
}
