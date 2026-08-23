using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using ConsulatTermine.Application.Localization;
using ConsulatTermine.Application.Resources;
using ConsulatTermine.Infrastructure.Services;
using ConsulatTermine.UI.Resources;

namespace ConsulatTermine.UI.Test;

/// <summary>
/// Prueft alle Ressourcensaetze der Anwendung gegen dieselben Regeln.
/// </summary>
/// <remarks>
/// Ohne diese Pruefung faellt eine vergessene Uebersetzung erst auf, wenn ein Besucher
/// die betroffene Seite in der betroffenen Sprache oeffnet. Der Test ersetzt kein
/// fachliches Lektorat: er stellt sicher, dass ueberall ein Text steht, nicht dass er
/// gut ist.
/// </remarks>
public class ResourceCompletenessTests
{
    private const string NeutralSuffix = ".resources";

    /// <summary>
    /// Die Kulturen der Satellitenzusammenstellungen. Sie tragen nur den Sprachanteil;
    /// die ausgelieferten Kulturen wie <c>ar-DZ</c> greifen ueber den Rueckfall darauf zu.
    /// </summary>
    private static readonly string[] TranslationCultures = ["en", "ar"];

    private static readonly Regex PlaceholderPattern = new(@"\{(\d+)", RegexOptions.Compiled);

    /// <summary>
    /// Alle Zusammenstellungen, die Ressourcen der Anwendung tragen.
    /// </summary>
    private static IEnumerable<Assembly> ResourceAssemblies =>
    [
        typeof(CommonTexts).Assembly,
        typeof(BusinessMessages).Assembly,
        typeof(SmtpEmailService).Assembly
    ];

    public static TheoryData<string, string> ResourceSets
    {
        get
        {
            var data = new TheoryData<string, string>();

            foreach (var assembly in ResourceAssemblies)
            {
                foreach (var baseName in BaseNamesOf(assembly))
                {
                    data.Add(assembly.GetName().Name!, baseName);
                }
            }

            return data;
        }
    }

    private static IEnumerable<string> BaseNamesOf(Assembly assembly) =>
        assembly.GetManifestResourceNames()
            .Where(name => name.EndsWith(NeutralSuffix, StringComparison.Ordinal))
            .Where(name => name.Contains(".Resources.", StringComparison.Ordinal))
            .Select(name => name[..^NeutralSuffix.Length])
            .Order(StringComparer.Ordinal);

    private static Assembly AssemblyNamed(string name) =>
        ResourceAssemblies.Single(assembly => assembly.GetName().Name == name);

    private static Dictionary<string, string> Entries(ResourceManager manager, CultureInfo culture)
    {
        var set = manager.GetResourceSet(culture, createIfNotExists: true, tryParents: false);

        Assert.NotNull(set);

        return set!.Cast<DictionaryEntry>()
            .ToDictionary(
                entry => (string)entry.Key,
                entry => (string?)entry.Value ?? string.Empty,
                StringComparer.Ordinal);
    }

    [Fact]
    public void EsGibtMindestensEinenRessourcensatz()
    {
        // Arrange
        // Act
        var count = ResourceAssemblies.Sum(assembly => BaseNamesOf(assembly).Count());

        // Assert
        Assert.True(count > 0, "Es wurde kein Ressourcensatz gefunden. Der Test wuerde sonst nichts pruefen.");
    }

    [Theory]
    [MemberData(nameof(ResourceSets))]
    public void JedeUebersetzung_HatDieselbenSchluesselWieDieDeutscheFassung(string assemblyName, string baseName)
    {
        // Arrange
        var assembly = AssemblyNamed(assemblyName);
        var manager = new ResourceManager(baseName, assembly);
        var german = Entries(manager, CultureInfo.InvariantCulture);

        foreach (var culture in TranslationCultures)
        {
            // Act
            var translation = Entries(manager, new CultureInfo(culture));

            // Assert
            var missing = german.Keys.Except(translation.Keys, StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();
            var surplus = translation.Keys.Except(german.Keys, StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();

            Assert.True(
                missing.Count == 0,
                $"{baseName}.{culture}: fehlende Schluessel: {string.Join(", ", missing)}");

            Assert.True(
                surplus.Count == 0,
                $"{baseName}.{culture}: ueberzaehlige Schluessel: {string.Join(", ", surplus)}");
        }
    }

    [Theory]
    [MemberData(nameof(ResourceSets))]
    public void KeinEintrag_IstLeer(string assemblyName, string baseName)
    {
        // Arrange
        var assembly = AssemblyNamed(assemblyName);
        var manager = new ResourceManager(baseName, assembly);

        var cultures = new[] { CultureInfo.InvariantCulture }
            .Concat(TranslationCultures.Select(culture => new CultureInfo(culture)));

        foreach (var culture in cultures)
        {
            // Act
            var entries = Entries(manager, culture);

            // Assert
            var empty = entries
                .Where(entry => string.IsNullOrWhiteSpace(entry.Value))
                .Select(entry => entry.Key)
                .Order(StringComparer.Ordinal)
                .ToList();

            Assert.True(
                empty.Count == 0,
                $"{baseName} ({culture.Name}): leere Eintraege: {string.Join(", ", empty)}");
        }
    }

    [Theory]
    [MemberData(nameof(ResourceSets))]
    public void JedeUebersetzung_VerwendetDieselbenPlatzhalter(string assemblyName, string baseName)
    {
        // Arrange
        var assembly = AssemblyNamed(assemblyName);
        var manager = new ResourceManager(baseName, assembly);
        var german = Entries(manager, CultureInfo.InvariantCulture);

        foreach (var culture in TranslationCultures)
        {
            var translation = Entries(manager, new CultureInfo(culture));

            foreach (var (key, germanValue) in german)
            {
                if (!translation.TryGetValue(key, out var translatedValue))
                {
                    continue;
                }

                // Act
                var expected = PlaceholdersOf(germanValue);
                var actual = PlaceholdersOf(translatedValue);

                // Assert
                // Ein fehlender Platzhalter liesse eine Angabe verschwinden, ein
                // zusaetzlicher wuerde beim Formatieren eine Ausnahme ausloesen.
                Assert.True(
                    expected.SetEquals(actual),
                    $"{baseName}.{culture}, Schluessel {key}: Platzhalter "
                    + $"[{string.Join(", ", expected.Order())}] erwartet, "
                    + $"[{string.Join(", ", actual.Order())}] gefunden.");
            }
        }
    }

    [Theory]
    [MemberData(nameof(ResourceSets))]
    public void JedeAusgelieferteSprache_LiefertZuJedemSchluesselEinenText(string assemblyName, string baseName)
    {
        // Arrange
        var assembly = AssemblyNamed(assemblyName);
        var manager = new ResourceManager(baseName, assembly);
        var german = Entries(manager, CultureInfo.InvariantCulture);

        foreach (var language in SupportedLanguages.All)
        {
            var culture = new CultureInfo(language.CultureCode);

            foreach (var key in german.Keys)
            {
                // Act
                // Genau dieser Weg wird zur Laufzeit gegangen: die ausgelieferte Kultur
                // wie ar-DZ faellt auf die Satellitenzusammenstellung ar zurueck.
                var text = manager.GetString(key, culture);

                // Assert
                Assert.False(
                    string.IsNullOrWhiteSpace(text),
                    $"{baseName}, Schluessel {key}: kein Text fuer {language.CultureCode}.");
            }
        }
    }

    private static HashSet<string> PlaceholdersOf(string value) =>
        PlaceholderPattern.Matches(value)
            .Select(match => match.Groups[1].Value)
            .ToHashSet(StringComparer.Ordinal);
}
