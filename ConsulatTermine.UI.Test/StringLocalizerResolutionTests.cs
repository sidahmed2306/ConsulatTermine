using System.Globalization;
using ConsulatTermine.UI.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace ConsulatTermine.UI.Test;

/// <summary>
/// Prueft, dass die Ressourcen ueber denselben Weg gefunden werden wie zur Laufzeit.
/// </summary>
/// <remarks>
/// <c>IStringLocalizer&lt;T&gt;</c> bildet den Namen des Ressourcensatzes aus dem
/// Typnamen und einem in <c>AddLocalization</c> gesetzten <c>ResourcesPath</c>. Steht
/// dort ein Pfad, obwohl der Ordner bereits Teil des Namensraums ist, wird er doppelt
/// vorangestellt und jede Suche laeuft ins Leere: die Oberflaeche zeigt dann still
/// die Schluessel statt der Texte. Genau das faengt dieser Test ab.
/// </remarks>
public class StringLocalizerResolutionTests
{
    private static readonly ServiceProvider Provider = BuildProvider();

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();

        // Die Fabrik der Localizer nimmt eine ILoggerFactory entgegen; im Host stellt
        // sie der Standardaufbau bereit, hier wird sie ausdruecklich registriert.
        services.AddLogging();

        // Gleiche Registrierung wie in Program.cs: ohne ResourcesPath.
        services.AddLocalization();

        return services.BuildServiceProvider();
    }

    private static IStringLocalizer<T> LocalizerFor<T>() =>
        Provider.GetRequiredService<IStringLocalizer<T>>();

    /// <summary>
    /// Setzt die Sprache der Oberflaeche fuer die Dauer eines Tests und stellt sie
    /// danach wieder her.
    /// </summary>
    private static CultureScope UseCulture(string cultureCode) => new(cultureCode);

    private sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo _previousUiCulture = CultureInfo.CurrentUICulture;
        private readonly CultureInfo _previousCulture = CultureInfo.CurrentCulture;

        public CultureScope(string cultureCode)
        {
            var culture = new CultureInfo(cultureCode);
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.CurrentCulture = culture;
        }

        public void Dispose()
        {
            CultureInfo.CurrentUICulture = _previousUiCulture;
            CultureInfo.CurrentCulture = _previousCulture;
        }
    }

    [Fact]
    public void Localizer_FindetDenDeutschenText()
    {
        // Arrange
        using var scope = UseCulture("de-DE");
        var texts = LocalizerFor<CommonTexts>();

        // Act
        var text = texts["Action.Save"];

        // Assert
        Assert.False(text.ResourceNotFound, "Der Ressourcensatz CommonTexts wurde nicht gefunden.");
        Assert.Equal("Speichern", text.Value);
    }

    [Theory]
    [InlineData("en-US", "Save")]
    [InlineData("ar-DZ", "حفظ")]
    public void Localizer_FindetDieUebersetzung(string cultureCode, string expected)
    {
        // Arrange
        using var scope = UseCulture(cultureCode);
        var texts = LocalizerFor<CommonTexts>();

        // Act
        var text = texts["Action.Save"];

        // Assert
        Assert.False(text.ResourceNotFound);
        Assert.Equal(expected, text.Value);
    }

    [Fact]
    public void Localizer_UnbekannterSchluessel_WirdAlsFehlendGemeldet()
    {
        // Arrange
        using var scope = UseCulture("de-DE");
        var texts = LocalizerFor<CommonTexts>();

        // Act
        var text = texts["Diesen.Schluessel.Gibt.Es.Nicht"];

        // Assert
        Assert.True(text.ResourceNotFound);
    }

    [Fact]
    public void Localizer_MitPlatzhalter_SetztDenWertEin()
    {
        // Arrange
        using var scope = UseCulture("de-DE");
        var texts = LocalizerFor<CommonTexts>();

        // Act
        var text = texts["Language.Current", "Deutsch"];

        // Assert
        Assert.False(text.ResourceNotFound);
        Assert.Contains("Deutsch", text.Value, StringComparison.Ordinal);
    }

    [Fact]
    public void AlleRessourcenanker_SindUeberDenLocalizerErreichbar()
    {
        // Arrange
        using var scope = UseCulture("de-DE");

        var probes = new (string Name, LocalizedString Text)[]
        {
            (nameof(CommonTexts), LocalizerFor<CommonTexts>()["Action.Save"]),
            (nameof(BookingTexts), LocalizerFor<BookingTexts>()["Services.Title"]),
            (nameof(WaitingRoomTexts), LocalizerFor<WaitingRoomTexts>()["Title"]),
            (nameof(EmployeeTexts), LocalizerFor<EmployeeTexts>()["Login.Title"]),
            (nameof(AdministrationTexts), LocalizerFor<AdministrationTexts>()["Employees.Title"]),
            (nameof(ValidationTexts), LocalizerFor<ValidationTexts>()["EmailRequired"])
        };

        // Act
        var missing = probes.Where(probe => probe.Text.ResourceNotFound).Select(probe => probe.Name).ToList();

        // Assert
        Assert.True(missing.Count == 0, $"Nicht gefundene Ressourcensaetze: {string.Join(", ", missing)}");
    }

    [Theory]
    [InlineData("de-DE", "Bitte geben Sie eine gültige E-Mail-Adresse an.")]
    [InlineData("en-US", "Please enter a valid email address.")]
    public void ValidationTexts_LiefertDieMeldungInDerSpracheDerSitzung(string cultureCode, string expected)
    {
        // Arrange
        using var scope = UseCulture(cultureCode);

        // Act
        // Diesen Weg gehen die Attribute aus DataAnnotations ueber
        // ErrorMessageResourceType und ErrorMessageResourceName.
        var message = ValidationTexts.EmailInvalid;

        // Assert
        Assert.Equal(expected, message);
    }

    [Theory]
    [InlineData("de-DE", "Der Service wurde nicht gefunden.")]
    [InlineData("ar-DZ", "لم يُعثر على الخدمة.")]
    public void BusinessMessages_LiefertDieMeldungInDerSpracheDerSitzung(string cultureCode, string expected)
    {
        // Arrange
        using var scope = UseCulture(cultureCode);

        // Act
        var message = ConsulatTermine.Application.Resources.BusinessMessages.Get("ServiceNotFound");

        // Assert
        Assert.Equal(expected, message);
    }

    [Fact]
    public void BusinessMessages_UnbekannteSprache_LiefertDieDeutscheFassung()
    {
        // Arrange
        using var scope = UseCulture("de-DE");

        // Act
        var message = ConsulatTermine.Application.Resources.BusinessMessages.Get("NotAuthorized");

        // Assert
        Assert.Equal("Für diese Aktion fehlt die Berechtigung.", message);
    }
}
