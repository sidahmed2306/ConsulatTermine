using ConsulatTermine.Application.Localization;
using Microsoft.AspNetCore.Localization;

namespace ConsulatTermine.UI.Localization;

/// <summary>
/// Endpunkt, ueber den ein Besucher die Sprache waehlt.
/// </summary>
public static class CultureEndpoints
{
    /// <summary>
    /// Route des Endpunkts. Wird auch vom Sprachmenue verwendet.
    /// </summary>
    public const string SetCultureRoute = "/culture/set";

    private const int CookieLifetimeInDays = 365;

    /// <summary>
    /// Registriert die Sprachumschaltung.
    /// </summary>
    /// <remarks>
    /// Bewusst ein vollstaendiger Seitenaufruf statt einer Aktion im Blazor-Kreis:
    /// Die Kultur wird zu Beginn einer Anfrage von der Request-Lokalisierung gesetzt.
    /// Ein bereits aufgebauter Kreis wuerde die neue Sprache erst nach einem Neuladen
    /// uebernehmen, und das Cookie liesse sich aus dem Kreis heraus ohnehin nicht setzen.
    ///
    /// Der Endpunkt ist bewusst per GET erreichbar, damit das Sprachmenue aus einem
    /// gewoehnlichen Verweis besteht und schon vor dem Aufbau der Verbindung bedienbar
    /// ist. Er aendert ausschliesslich eine Darstellungsvorliebe und keinen fachlichen
    /// oder sicherheitsrelevanten Zustand.
    /// </remarks>
    public static IEndpointRouteBuilder MapCultureEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapGet(SetCultureRoute, (HttpContext httpContext, string? culture, string? returnUrl) =>
        {
            // Eine unbekannte Angabe ergibt die Standardsprache, statt die Anfrage
            // abzuweisen: die Sprache ist eine Vorliebe, kein fachlicher Vorgang.
            var language = SupportedLanguages.Resolve(culture);

            httpContext.Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(language.CultureCode)),
                new CookieOptions
                {
                    Path = "/",
                    Expires = DateTimeOffset.UtcNow.AddDays(CookieLifetimeInDays),
                    HttpOnly = true,
                    Secure = true,

                    // Lax, damit die Sprachwahl auch erhalten bleibt, wenn der Besucher
                    // die Seite ueber einen externen Verweis wieder betritt.
                    SameSite = SameSiteMode.Lax,

                    // Die Sprachwahl ist fuer den Betrieb der Seite erforderlich und
                    // faellt damit nicht unter einwilligungspflichtige Cookies.
                    IsEssential = true
                });

            return Results.LocalRedirect(LocalReturnUrl.Sanitize(returnUrl));
        })
        .AllowAnonymous()
        .WithName("SetCulture");

        return endpoints;
    }

    /// <summary>
    /// Baut die Adresse, die eine Sprache setzt und danach zur angegebenen Seite
    /// zurueckkehrt.
    /// </summary>
    public static string BuildSetCultureUrl(string cultureCode, string returnUrl) =>
        $"{SetCultureRoute}?culture={Uri.EscapeDataString(cultureCode)}"
        + $"&returnUrl={Uri.EscapeDataString(LocalReturnUrl.Sanitize(returnUrl))}";
}
