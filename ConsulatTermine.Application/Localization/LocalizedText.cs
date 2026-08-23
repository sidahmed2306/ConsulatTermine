namespace ConsulatTermine.Application.Localization;

/// <summary>
/// Waehlt aus mehreren gepflegten Sprachfassungen eines Stammdatentexts die
/// passende aus.
/// </summary>
/// <remarks>
/// Bezeichnungen von Services stehen in der Datenbank und nicht in den
/// Ressourcendateien: sie werden von der Verwaltung gepflegt und aendern sich,
/// ohne dass die Anwendung neu ausgeliefert wird.
/// </remarks>
public static class LocalizedText
{
    /// <summary>
    /// Gibt die Fassung zur uebergebenen Kultur zurueck. Ist sie nicht gepflegt,
    /// gilt die deutsche Fassung: eine leere Bezeichnung waere fuer den Buchenden
    /// unbrauchbar, eine deutsche immerhin lesbar.
    /// </summary>
    /// <param name="german">Pflichtfassung in der Amtssprache des Standorts.</param>
    /// <param name="english">Englische Fassung, sofern gepflegt.</param>
    /// <param name="arabic">Arabische Fassung, sofern gepflegt.</param>
    /// <param name="cultureCode">Kultur der Anzeige, beliebige Schreibweise.</param>
    public static string ForCulture(string german, string? english, string? arabic, string? cultureCode)
    {
        var language = SupportedLanguages.Resolve(cultureCode);

        var translation = language.TwoLetterCode switch
        {
            "en" => english,
            "ar" => arabic,
            _ => german
        };

        return string.IsNullOrWhiteSpace(translation) ? german : translation;
    }
}
