using ConsulatTermine.Application.Localization;
using Microsoft.AspNetCore.Localization;

namespace ConsulatTermine.UI.Localization;

/// <summary>
/// Ermittelt die Sprache eines Besuchers, der noch nichts gewaehlt hat, aus dem
/// Header <c>Accept-Language</c>.
/// </summary>
/// <remarks>
/// Der eingebaute <c>AcceptLanguageHeaderRequestCultureProvider</c> vergleicht die
/// Browserangabe mit der Liste der unterstuetzten Kulturen. Ein Browser meldet aber
/// regelmaessig <c>ar</c>, <c>ar-MA</c> oder <c>en-GB</c>; ausgeliefert werden
/// <c>ar-DZ</c> und <c>en-US</c>. Ohne Zuordnung ueber den Sprachanteil laendet ein
/// arabisch- oder englischsprachiger Besucher auf der deutschen Oberflaeche.
/// </remarks>
public sealed class BrowserLanguageRequestCultureProvider : RequestCultureProvider
{
    /// <summary>
    /// Mehr Eintraege wertet der Provider nicht aus. Der Header ist frei setzbar und
    /// soll keine unbegrenzte Arbeit je Anfrage ausloesen.
    /// </summary>
    private const int MaxEvaluatedLanguages = 10;

    public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var acceptLanguageHeader = httpContext.Request.GetTypedHeaders().AcceptLanguage;

        if (acceptLanguageHeader.Count == 0)
        {
            return Task.FromResult<ProviderCultureResult?>(null);
        }

        var preferredLanguages = acceptLanguageHeader
            .OrderByDescending(header => header.Quality ?? 1d)
            .Take(MaxEvaluatedLanguages);

        foreach (var language in preferredLanguages)
        {
            var requestedCode = language.Value.Value;

            if (string.IsNullOrWhiteSpace(requestedCode) || requestedCode == "*")
            {
                continue;
            }

            // Resolve liefert bei einer unbekannten Angabe die Standardsprache. Hier ist
            // aber gefragt, ob dieser Eintrag tatsaechlich passt: sonst wuerde der erste
            // beliebige Header-Eintrag die Auswertung der folgenden verhindern.
            var match = SupportedLanguages.Resolve(requestedCode);

            if (!MatchesRequestedLanguage(match, requestedCode))
            {
                continue;
            }

            return Task.FromResult<ProviderCultureResult?>(
                new ProviderCultureResult(match.CultureCode, match.CultureCode));
        }

        return Task.FromResult<ProviderCultureResult?>(null);
    }

    private static bool MatchesRequestedLanguage(SupportedLanguage language, string requestedCode) =>
        requestedCode.StartsWith(language.TwoLetterCode, StringComparison.OrdinalIgnoreCase);
}
