using ConsulatTermine.Application.DTOs;
using ConsulatTermine.Application.Localization;
using ConsulatTermine.Domain.Entities;

namespace ConsulatTermine.Application.Test;

public class LocalizedTextTests
{
    private const string German = "Reisepass";
    private const string English = "Passport";
    private const string Arabic = "جواز السفر";

    [Theory]
    [InlineData("de-DE", German)]
    [InlineData("en-US", English)]
    [InlineData("ar-DZ", Arabic)]
    [InlineData("en", English)]
    [InlineData("ar-MA", Arabic)]
    public void ForCulture_UebersetzungGepflegt_LiefertDieFassungDerKultur(string culture, string expected)
    {
        // Arrange
        // Act
        var text = LocalizedText.ForCulture(German, English, Arabic, culture);

        // Assert
        Assert.Equal(expected, text);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ForCulture_UebersetzungFehlt_LiefertDieDeutscheFassung(string? translation)
    {
        // Arrange
        // Act
        var text = LocalizedText.ForCulture(German, translation, translation, "en-US");

        // Assert
        Assert.Equal(German, text);
    }

    [Fact]
    public void ForCulture_UnbekannteKultur_LiefertDieDeutscheFassung()
    {
        // Arrange
        // Act
        var text = LocalizedText.ForCulture(German, English, Arabic, "fr-FR");

        // Assert
        Assert.Equal(German, text);
    }

    [Fact]
    public void NameFor_Service_LiefertDieFassungDerKultur()
    {
        // Arrange
        var service = new Service
        {
            Name = German,
            NameEnglish = English,
            NameArabic = Arabic
        };

        // Act
        var text = service.NameFor("ar-DZ");

        // Assert
        Assert.Equal(Arabic, text);
    }

    [Fact]
    public void NameFor_ServiceOhneUebersetzung_LiefertDieDeutscheFassung()
    {
        // Arrange
        var service = new Service { Name = German };

        // Act
        var text = service.NameFor("ar-DZ");

        // Assert
        Assert.Equal(German, text);
    }

    [Fact]
    public void DescriptionFor_ServiceDto_LiefertDieFassungDerKultur()
    {
        // Arrange
        var service = new ServiceDto
        {
            Description = "Antrag auf einen Reisepass",
            DescriptionEnglish = "Application for a passport"
        };

        // Act
        var text = service.DescriptionFor("en-US");

        // Assert
        Assert.Equal("Application for a passport", text);
    }

    [Fact]
    public void NameFor_ServiceIstNull_WirdAbgewiesen()
    {
        // Arrange
        Service? service = null;

        // Act
        // Assert
        Assert.Throws<ArgumentNullException>(() => service!.NameFor("de-DE"));
    }
}
