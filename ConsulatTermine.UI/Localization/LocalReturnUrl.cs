namespace ConsulatTermine.UI.Localization;

/// <summary>
/// Prueft die Ruecksprungadresse der Sprachumschaltung. Der Wert stammt aus der
/// Anfrage und ist damit fremdgesteuert: ohne Pruefung liesse sich der Endpunkt
/// als Weiterleitung auf eine fremde Seite missbrauchen (Open Redirect).
/// Siehe harness/security.md Abschnitt 4.
/// </summary>
public static class LocalReturnUrl
{
    /// <summary>
    /// Ziel, das gilt, wenn die uebergebene Adresse nicht anwendungsintern ist.
    /// </summary>
    public const string Fallback = "/";

    /// <summary>
    /// Gibt die Adresse zurueck, wenn sie ein anwendungsinterner Pfad ist,
    /// sonst <see cref="Fallback"/>.
    /// </summary>
    /// <remarks>
    /// Zugelassen ist ausschliesslich ein Pfad, der mit genau einem Schraegstrich
    /// beginnt. Damit fallen absolute Adressen, protokollrelative Adressen
    /// (<c>//example.org</c>), zurueckgesetzte Schraegstriche (<c>/\example.org</c>)
    /// und Steuerzeichen heraus.
    /// </remarks>
    public static string Sanitize(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            return Fallback;
        }

        var candidate = returnUrl.Trim();

        if (candidate[0] != '/')
        {
            return Fallback;
        }

        if (candidate.Length > 1 && (candidate[1] == '/' || candidate[1] == '\\'))
        {
            return Fallback;
        }

        if (candidate.Any(char.IsControl))
        {
            return Fallback;
        }

        return candidate;
    }
}
