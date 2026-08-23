using ConsulatTermine.UI.Localization;
using Microsoft.AspNetCore.Http;

namespace ConsulatTermine.UI.Test;

/// <summary>
/// Ein Besucher, der noch nichts gewaehlt hat, soll die Oberflaeche in der Sprache
/// seines Browsers sehen, sofern sie ausgeliefert wird.
/// </summary>
public class BrowserLanguageRequestCultureProviderTests
{
    private readonly BrowserLanguageRequestCultureProvider _provider = new();

    private static DefaultHttpContext ContextWith(string? acceptLanguage)
    {
        var context = new DefaultHttpContext();

        if (acceptLanguage is not null)
        {
            context.Request.Headers.AcceptLanguage = acceptLanguage;
        }

        return context;
    }

    [Theory]
    [InlineData("ar", "ar-DZ")]
    [InlineData("ar-MA", "ar-DZ")]
    [InlineData("en-GB", "en-US")]
    [InlineData("de-AT", "de-DE")]
    public async Task Browsersprache_WirdDerAusgeliefertenKulturZugeordnet(
        string acceptLanguage,
        string expected)
    {
        // Arrange
        var context = ContextWith(acceptLanguage);

        // Act
        var result = await _provider.DetermineProviderCultureResult(context);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expected, result!.UICultures[0].Value);
    }

    [Fact]
    public async Task MehrereSprachen_HoechsteGewichtungGewinnt()
    {
        // Arrange
        var context = ContextWith("fr-FR;q=0.9, ar-MA;q=1.0, de-DE;q=0.2");

        // Act
        var result = await _provider.DetermineProviderCultureResult(context);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ar-DZ", result!.Cultures[0].Value);
    }

    [Fact]
    public async Task NurNichtAusgelieferteSprachen_ErgibtKeinErgebnis()
    {
        // Arrange
        var context = ContextWith("fr-FR, es-ES, it-IT");

        // Act
        var result = await _provider.DetermineProviderCultureResult(context);

        // Assert
        // Kein Ergebnis bedeutet: die Request-Lokalisierung faellt auf die
        // Standardsprache zurueck, statt eine falsche Sprache festzuschreiben.
        Assert.Null(result);
    }

    [Fact]
    public async Task NichtAusgelieferteSpracheVorAusgelieferter_UeberspringtDenErstenEintrag()
    {
        // Arrange
        var context = ContextWith("fr-FR, en-GB");

        // Act
        var result = await _provider.DetermineProviderCultureResult(context);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("en-US", result!.Cultures[0].Value);
    }

    [Fact]
    public async Task OhneHeader_ErgibtKeinErgebnis()
    {
        // Arrange
        var context = ContextWith(null);

        // Act
        var result = await _provider.DetermineProviderCultureResult(context);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Platzhalter_ErgibtKeinErgebnis()
    {
        // Arrange
        var context = ContextWith("*");

        // Act
        var result = await _provider.DetermineProviderCultureResult(context);

        // Assert
        Assert.Null(result);
    }
}
