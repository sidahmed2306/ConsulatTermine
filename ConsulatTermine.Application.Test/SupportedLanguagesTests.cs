using ConsulatTermine.Application.Localization;

namespace ConsulatTermine.Application.Test;

public class SupportedLanguagesTests
{
    [Fact]
    public void All_EnthaeltDieDreiAusgeliefertenSprachen()
    {
        // Arrange
        // Act
        var codes = SupportedLanguages.All.Select(language => language.CultureCode).ToList();

        // Assert
        Assert.Equal(["de-DE", "en-US", "ar-DZ"], codes);
    }

    [Fact]
    public void Default_IstDeutsch()
    {
        // Arrange
        // Act
        var language = SupportedLanguages.Default;

        // Assert
        Assert.Equal(SupportedLanguages.DefaultCultureCode, language.CultureCode);
        Assert.False(language.IsRightToLeft);
    }

    [Fact]
    public void Arabisch_WirdVonRechtsNachLinksGeschrieben()
    {
        // Arrange
        // Act
        var language = SupportedLanguages.Resolve("ar-DZ");

        // Assert
        Assert.True(language.IsRightToLeft);
        Assert.Equal("rtl", language.TextDirection);
    }

    [Theory]
    [InlineData("de-DE", "de-DE")]
    [InlineData("en-US", "en-US")]
    [InlineData("ar-DZ", "ar-DZ")]
    public void Resolve_ExakteKultur_LiefertGenauDieseSprache(string requested, string expected)
    {
        // Arrange
        // Act
        var language = SupportedLanguages.Resolve(requested);

        // Assert
        Assert.Equal(expected, language.CultureCode);
    }

    [Theory]
    [InlineData("EN-us", "en-US")]
    [InlineData("Ar-Dz", "ar-DZ")]
    public void Resolve_AbweichendeSchreibweise_LiefertDieSprache(string requested, string expected)
    {
        // Arrange
        // Act
        var language = SupportedLanguages.Resolve(requested);

        // Assert
        Assert.Equal(expected, language.CultureCode);
    }

    [Theory]
    [InlineData("ar", "ar-DZ")]
    [InlineData("ar-MA", "ar-DZ")]
    [InlineData("en", "en-US")]
    [InlineData("en-GB", "en-US")]
    [InlineData("de", "de-DE")]
    [InlineData("de-AT", "de-DE")]
    public void Resolve_NurSprachanteilOderAnderesLand_LiefertDieAusgelieferteKultur(
        string requested,
        string expected)
    {
        // Arrange
        // Act
        var language = SupportedLanguages.Resolve(requested);

        // Assert
        Assert.Equal(expected, language.CultureCode);
    }

    [Theory]
    [InlineData("fr-FR")]
    [InlineData("zz")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Resolve_UnbekannteOderFehlendeAngabe_LiefertDieStandardsprache(string? requested)
    {
        // Arrange
        // Act
        var language = SupportedLanguages.Resolve(requested);

        // Assert
        Assert.Equal(SupportedLanguages.DefaultCultureCode, language.CultureCode);
    }

    [Theory]
    [InlineData("de-DE", true)]
    [InlineData("ar-DZ", true)]
    [InlineData("ar", false)]
    [InlineData("fr-FR", false)]
    [InlineData(null, false)]
    public void IsSupported_PrueftNurDieVollstaendigeKultur(string? requested, bool expected)
    {
        // Arrange
        // Act
        var supported = SupportedLanguages.IsSupported(requested);

        // Assert
        Assert.Equal(expected, supported);
    }

    [Fact]
    public void Normalize_UnbekannteAngabe_LiefertDieStandardsprache()
    {
        // Arrange
        // Act
        var code = SupportedLanguages.Normalize("kl-KL");

        // Assert
        Assert.Equal(SupportedLanguages.DefaultCultureCode, code);
    }

    [Fact]
    public void ToCultureInfo_LiefertDieKulturDerAusgeliefertenSprache()
    {
        // Arrange
        // Act
        var culture = SupportedLanguages.ToCultureInfo("ar");

        // Assert
        Assert.Equal("ar-DZ", culture.Name);
    }

    [Fact]
    public void Cultures_EntsprichtDenAusgeliefertenSprachen()
    {
        // Arrange
        var expected = SupportedLanguages.All.Select(language => language.CultureCode);

        // Act
        var actual = SupportedLanguages.Cultures.Select(culture => culture.Name);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TwoLetterCode_LiefertDenSprachanteil()
    {
        // Arrange
        // Act
        var language = SupportedLanguages.Resolve("ar-DZ");

        // Assert
        Assert.Equal("ar", language.TwoLetterCode);
    }
}
