using ConsulatTermine.UI.Localization;

namespace ConsulatTermine.UI.Test;

/// <summary>
/// Die Ruecksprungadresse der Sprachumschaltung stammt aus der Anfrage. Ohne Pruefung
/// liesse sich der Endpunkt als Weiterleitung auf eine fremde Seite missbrauchen.
/// </summary>
public class LocalReturnUrlTests
{
    [Theory]
    [InlineData("/")]
    [InlineData("/services")]
    [InlineData("/appointment-cancel?ref=CONSUL-2026-ABC123&token=xyz")]
    [InlineData("/employee/dashboard#heute")]
    public void Sanitize_AnwendungsinternerPfad_BleibtErhalten(string returnUrl)
    {
        // Arrange
        // Act
        var sanitized = LocalReturnUrl.Sanitize(returnUrl);

        // Assert
        Assert.Equal(returnUrl, sanitized);
    }

    [Theory]
    [InlineData("https://example.org/phishing")]
    [InlineData("http://example.org")]
    [InlineData("//example.org")]
    [InlineData("/\\example.org")]
    [InlineData("javascript:alert(1)")]
    [InlineData("services")]
    public void Sanitize_FremdeAdresse_WirdAufDieStartseiteZurueckgefuehrt(string returnUrl)
    {
        // Arrange
        // Act
        var sanitized = LocalReturnUrl.Sanitize(returnUrl);

        // Assert
        Assert.Equal(LocalReturnUrl.Fallback, sanitized);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Sanitize_FehlendeAdresse_LiefertDieStartseite(string? returnUrl)
    {
        // Arrange
        // Act
        var sanitized = LocalReturnUrl.Sanitize(returnUrl);

        // Assert
        Assert.Equal(LocalReturnUrl.Fallback, sanitized);
    }

    [Fact]
    public void Sanitize_AdresseMitSteuerzeichen_WirdVerworfen()
    {
        // Arrange
        var returnUrl = "/services\r\nSet-Cookie: eingeschleust=1";

        // Act
        var sanitized = LocalReturnUrl.Sanitize(returnUrl);

        // Assert
        Assert.Equal(LocalReturnUrl.Fallback, sanitized);
    }

    [Fact]
    public void BuildSetCultureUrl_KodiertKulturUndZiel()
    {
        // Arrange
        // Act
        var url = CultureEndpoints.BuildSetCultureUrl("ar-DZ", "/appointment?ref=a b");

        // Assert
        Assert.StartsWith($"{CultureEndpoints.SetCultureRoute}?culture=ar-DZ&returnUrl=", url, StringComparison.Ordinal);
        Assert.DoesNotContain(" ", url, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildSetCultureUrl_FremdesZiel_WirdAufDieStartseiteZurueckgefuehrt()
    {
        // Arrange
        // Act
        var url = CultureEndpoints.BuildSetCultureUrl("de-DE", "https://example.org");

        // Assert
        Assert.EndsWith($"returnUrl={Uri.EscapeDataString(LocalReturnUrl.Fallback)}", url, StringComparison.Ordinal);
    }
}
