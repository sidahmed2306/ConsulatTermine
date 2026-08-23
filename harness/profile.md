# Stack-Profil: .NET / Blazor Server

Dieses Profil konkretisiert `harness/design.md` und `harness/code.md` für den Stack C# / ASP.NET Core / Blazor Server. Bei Widersprüchen gilt die Rangfolge aus `harness/design.md`.

## 1. Technologischer Rahmen

- C# und ASP.NET Core auf dem in der Projektdatei freigegebenen Target Framework.
- Blazor Server beziehungsweise der im Bestand freigegebene interaktive Server-Render-Modus für Web-UIs.
- `Nullable` und `ImplicitUsings` bleiben aktiviert.
- EF Core mit einer freigegebenen relationalen Datenbank für anwendungseigene persistente Daten.
- xUnit für automatisierte Tests.
- Logging über `Microsoft.Extensions.Logging`; der konkrete Provider folgt der vorhandenen Projektkonfiguration.
- MudBlazor ist als Open-Source-Komponentenbibliothek freigegeben, aber keine Pflichttechnologie.

Die konkrete Projektdatei ist für Framework- und Paketversionen maßgeblich. Ein Agent aktualisiert Versionen nicht ungefragt und führt keine Migration auf ein anderes UI-Framework als Nebenwirkung einer fachlichen Änderung durch.

## 2. Zusätzliche Sprachen

Der Hauptstack ist verbindlich .NET mit Blazor Server. Fachliche Geschäftslogik, Persistenz, Autorisierung und UI werden in C# umgesetzt. Zusätzliche Sprachen sind für klar abgegrenzte Zwecke zulässig:

### JavaScript / TypeScript

- Zulässig für Browser-Aufgaben, die Blazor, HTML und CSS nicht geeignet abdecken, zum Beispiel Canvas- und WebGL-Darstellung, Karten, Diagramme, Clipboard, Datei-Downloads, Focus- und Scroll-Steuerung oder die Anbindung einer etablierten Browser-Bibliothek.
- Einbindung ausschließlich über JS Interop mit einer klar benannten, isolierten Modul-Datei.
- Kein Geschäftsregel-, Berechtigungs- oder Validierungscode in JavaScript. Serverseitige Prüfungen bleiben autoritativ.
- Kein paralleles Frontend-Framework und kein zusätzlicher Bundler ohne ADR.
- Skripte werden mit dem zugehörigen Komponenten- oder Feature-Namen abgelegt und bei Bedarf sauber entladen (`IAsyncDisposable`).
- Zuerst wird geprüft, ob eine native HTML-, CSS- oder Blazor-Lösung ausreicht.

### Sonstiges

- PowerShell und Bash sind fuer Build-, Deployment- und Wartungsskripte zulaessig.
- SQL-Skripte sind fuer Analyse und dokumentierte Datenkorrekturen zulaessig; Schemaaenderungen laufen ueber EF-Core-Migrationen.
- Python ist in diesem Projekt nicht im Einsatz. Jede weitere Sprache oder Laufzeitumgebung im Repository benoetigt ein ADR.

## 3. Projektmappe

Dieses Projekt wurde mit `/harness adopt` in ein Bestandsprojekt eingefuehrt. Es behaelt seine
gewachsene Vier-Schichten-Struktur. Die im Core-Profil beschriebene Zwei-Projekt-Aufteilung
(`.Web` / `.Backend`) wird **nicht** uebergestuelpt; sie ist funktional gleichwertig abgebildet.

```text
ConsulatTermine.sln
├── ConsulatTermine.Domain              # Entities und Enums, keine Abhaengigkeiten
├── ConsulatTermine.Application         # Interfaces, DTOs, UI-Modelle, reine Berechnungen
├── ConsulatTermine.Application.Test    # Tests der Application-Schicht
├── ConsulatTermine.Infrastructure      # EF Core, Manager mit Geschaeftslogik, SignalR, E-Mail
├── ConsulatTermine.Infrastructure.Test # Tests der Infrastructure-Schicht
└── ConsulatTermine.UI                  # Blazor-Server-UI
```

Zuordnung zum Core-Profil:

| Core-Profil | Dieses Projekt |
|---|---|
| `<Projekt>.Web` | `ConsulatTermine.UI` |
| `<Projekt>.Backend` (Logic + Data) | `ConsulatTermine.Infrastructure` |
| `<Projekt>.Backend` (Interfaces, UiModels) | `ConsulatTermine.Application` |
| Entities | `ConsulatTermine.Domain` |
| `<Projekt>.Backend.Test` | `ConsulatTermine.Application.Test`, `ConsulatTermine.Infrastructure.Test` |

Erlaubte Abhaengigkeitsrichtung — Verstoesse sind Architekturfehler:

```text
UI ──▶ Application ──▶ Domain
UI ──▶ Infrastructure ──▶ Application ──▶ Domain
```

Die UI referenziert `Infrastructure` ausschliesslich zur Registrierung im DI-Container
(`AddInfrastructure`). Fachliche Aufrufe aus der UI laufen ueber die Interfaces in
`ConsulatTermine.Application.Interfaces`.

## 4. Struktur der Backend-Projekte

```text
ConsulatTermine.Domain/
├── Entities/                # mutable POCOs, keine Geschaeftslogik, keine EF-Attribute
└── Enums/

ConsulatTermine.Application/
├── Interfaces/              # Vertraege der fachlichen Anwendungsfaelle
│   └── Booking/
├── DTOs/                    # flache, auf konkrete Abfragen zugeschnittene Modelle
├── ViewModels/              # UI-nahe Praesentationsmodelle
├── Configuration/           # typisierte Options-Klassen samt Validierung
└── Services/                # reine, persistenzfreie Berechnungen

ConsulatTermine.Infrastructure/
├── Persistence/
│   ├── ApplicationDbContext.cs
│   └── Mappings/            # je Entity eine IEntityTypeConfiguration<T>
├── Migrations/              # ausschliesslich EF-Core-Migrationen
├── Services/                # Geschaeftslogik der Anwendungsfaelle
│   └── Booking/
├── Security/                # Passwort-Hashing, Token-Erzeugung
├── SignalR/                 # Hubs
└── DependencyInjection.cs   # AddInfrastructure(...)
```

| Bereich | Verantwortung |
|---|---|
| `Application/Interfaces` | Vertraege fachlicher Anwendungsfaelle und echter technischer Grenzen |
| `Application/DTOs`, `ViewModels` | flache Modelle fuer konkrete UI-Abfragen und Interaktionen |
| `Application/Configuration` | typisierte Options-Klassen und Validierung von Konfiguration |
| `Application/Services` | Berechnungen ohne Persistenz- und Infrastrukturbezug |
| `Infrastructure/Services` | Geschaeftslogik, Autorisierung und Transaktionsgrenzen |
| `Infrastructure/Persistence` | `DbContext`, Mappings und Persistenzdetails |
| `Infrastructure/Security` | Passwort- und Token-Verfahren auf Basis von ASP.NET-Core-Mechanismen |
| `Domain/Entities` | mutable POCO-Datenklassen ohne UI- oder Geschaeftslogik |

Die UI darf `Application` und `Domain` verwenden. Direkte Zugriffe aus der UI auf
`ApplicationDbContext`, `Persistence`, `Migrations` oder konkrete Integrationsclients sind verboten.

Der Suffix fuer fachliche Anwendungsfallklassen ist in diesem Projekt `Service`
(zum Beispiel `BookingService`, `EmployeeAuthService`), nicht `Manager`. Der Suffix wird
projektweit konsistent verwendet und nicht gemischt.

## 5. Blazor-UI-Struktur

```text
ConsulatTermine.UI/
├── App.razor
├── _Imports.razor
├── Program.cs
├── Authentication/          # AuthenticationStateProvider, Policies, Konstanten
├── Pages/                   # routbare Seiten
├── Shared/                  # Layouts und wiederverwendbare Komponenten
├── Theme/                   # MudTheme
├── Resources/               # .resx fuer de-DE, en-US, ar-DZ
└── wwwroot/
    ├── css/
    └── js/                  # isolierte Interop-Module
```

- Der `@code`-Block bleibt grundsaetzlich in der Razor-Datei. Er enthaelt jedoch keine
  persistenzrelevante Geschaeftslogik, sondern ruft die Interfaces der Application-Schicht auf.
- Bestandsseiten behalten ihre gewachsenen Dateinamen. Neue routbare Seiten enden auf
  `Page.razor`; bestehende werden nicht ohne Auftrag umbenannt.
- Geschuetzte Seiten verwenden `[Authorize]` mit einer konkreten Policy aus
  `PROJECT_CONTEXT.md` Abschnitt 7.
- Oeffentliche Seiten verwenden `[AllowAnonymous]`.
- `App.razor` behandelt oeffentliche, angemeldete und nicht autorisierte Zustaende eindeutig.
- Lade-, Fehler- und Speichervorgaenge werden gegen Mehrfachausfuehrung abgesichert.
- `IDisposable` oder `IAsyncDisposable` implementieren, wenn Subscriptions oder Ressourcen
  bereinigt werden muessen. Das betrifft insbesondere SignalR-Verbindungen und
  Event-Abonnements auf `AuthenticationStateProvider`.
- Integrationsclients werden nie direkt aus Razor-Komponenten aufgerufen.

## 6. Benennung

| Element | Konvention | Beispiel |
|---|---|---|
| Namespace | PascalCase, entspricht der Ordnerstruktur | `ConsulatTermine.Backend.Logic.Manager` |
| Klasse, Record, Struct, Enum | PascalCase | `OrderApprovalManager` |
| Interface | `I` + PascalCase | `IOrderApprovalManager` |
| Methode | PascalCase | `ApproveOrderAsync` |
| Property | PascalCase | `PaymentDueAt` |
| Parameter und lokale Variable | camelCase | `orderId` |
| privates Feld | `_` + camelCase | `_contextFactory` |
| Konstante | PascalCase | `DefaultPageSize` |
| asynchrone Methode | Suffix `Async` | `SaveAsync` |
| Testprojekt | `<Projekt>.Test` | `ConsulatTermine.Backend.Test` |

- `Id`, nicht `ID`, außer ein externer Vertrag verlangt exakt eine andere Schreibweise.
- Fachliche Anwendungsfallklassen im Backend tragen das Suffix `Manager`.
- Integrationsklassen tragen einen fachlich-technischen Namen wie `<System>OrderClient`, `<System>ProductAdapter` oder `<System>WebhookHandler`.

## 7. Dateien, Namespaces und Formatierung

- Ein Top-Level-Typ pro Datei; Dateiname entspricht dem Typnamen.
- File-scoped Namespaces verwenden; Namespace entspricht der Ordnerstruktur.
- Vier Leerzeichen, keine Tabs.
- Zeilen sinnvoll umbrechen; Richtwert 120 Zeichen.
- Modifier in der üblichen C#-Reihenfolge.
- Allman-Klammerstil.
- Bei `if`, `for`, `foreach`, `while`, `do`, `lock` und `fixed` ist ein Block auch bei einer einzelnen Anweisung verpflichtend.

## 8. C#-Stil

- `var` verwenden, wenn der Typ aus der rechten Seite eindeutig erkennbar ist.
- C#-Schlüsselwörter verwenden: `string`, `int`, `bool`.
- String-Interpolation statt unübersichtlicher Verkettung verwenden.
- `nameof` für Symbolnamen verwenden, aber nicht für stabile externe Vertrags- oder Datenbanknamen.
- Nullable Reference Types ernst nehmen; kein Null-Forgiving-Operator ohne belegbaren Grund.
- Collection- und Object-Initializer verwenden.
- LINQ nur verwenden, wenn Ausdruck, Übersetzung und Performance verständlich bleiben.

## 9. Async

- Keine Verwendung von `.Result`, `.Wait()` oder `.GetAwaiter().GetResult()` im Anwendungscode.
- Asynchrone Methoden enden auf `Async`.
- `CancellationToken` bei externen, datenbankbezogenen und länger laufenden Vorgängen weiterreichen.
- Kein `async void`, außer bei frameworkbedingten Event-Handlern.

## 10. Dependency Injection

- Abhängigkeiten über Konstruktoren injizieren.
- Blazor Server verwendet für eigene EF-Core-Daten grundsätzlich `IDbContextFactory<TContext>`; pro Vorgang wird ein Kontext erzeugt und mit `await using` freigegeben.
- Kein langlebiger `DbContext` in Komponenten, Singletons oder Integrationsclients.
- Interfaces liegen unter `Logic/Interfaces` oder bei externen Grenzen unter `Integrations/Interfaces`.

## 11. EF Core

- Entities sind mutable POCOs ohne EF-Attribute und ohne Geschäftslogik.
- Audit- oder Soft-Delete-Basistypen werden nur verwendet, wenn sie im Projekt verbindlich definiert sind.
- Jedes Mapping liegt in einer eigenen `IEntityTypeConfiguration<T>` unter `Data/Mappings`.
- Tabellen- und Spaltennamen werden explizit als stabile Strings konfiguriert.
- Tabellen und Spalten verwenden Singular und PascalCase, sofern keine bestehende Datenbankkonvention etwas anderes vorgibt.
- `IsRequired`, `HasMaxLength`, Beziehungen und Indizes werden bewusst festgelegt.
- Read-only-Abfragen verwenden `AsNoTracking()`.
- Für UI-Abfragen gezielt in `UiModels` projizieren.
- Keine Datenbankabfrage in Schleifen, wenn dadurch N+1 entsteht.
- Pagination besitzt eine stabile Sortierung.
- `IgnoreQueryFilters()` nur mit dokumentierter Absicht.
- `SaveChangesAsync` bildet eine bewusste Transaktionsgrenze im Logic-Layer.
- Migrationen gehören zum owning context der Daten und werden nicht kosmetisch umgeschrieben.
- Razor-Komponenten verwenden keinen `DbContext` direkt.

```csharp
public sealed class OrderApprovalMapping : IEntityTypeConfiguration<OrderApproval>
{
    public void Configure(EntityTypeBuilder<OrderApproval> builder)
    {
        builder.ToTable("OrderApproval");

        builder.HasKey(approval => approval.OrderApprovalId);

        builder.Property(approval => approval.ExternalOrderNumber)
            .HasColumnName("ExternalOrderNumber")
            .HasMaxLength(100)
            .IsRequired();
    }
}
```

## 12. MudBlazor

MudBlazor ist ein optionales Werkzeug. Es wird nur verwendet, wenn die konkrete Komponente fachlich, visuell, barrierefrei und technisch passt. Eigene Razor-, HTML- und CSS-Komponenten sind ausdrücklich zulässig.

Typisch geeignet: Dialoge, Formularfelder, Selects, Autocomplete, Date- und Time-Picker, Tabs, Snackbar, einfache Tabellen, interne Datenansichten, Navigation, administrative Dashboards.

Typisch besser selbst gebaut: markenprägende Hero-Bereiche, Produktkarten, Galerien, Landingpages, Vergleiche, individuelle responsive Tabellen, Medien- und Storytelling-Bereiche, Prozessvisualisierungen.

Vor einer Tabelle wird entschieden, ob `MudDataGrid`, `MudTable`, eine semantische HTML-Tabelle oder eine eigene Razor-Komponente am besten passt. Themes werden über `MudTheme` und CSS Custom Properties gepflegt.

## 13. Tests

- Testframework ist xUnit, sofern der Bestand nichts anderes vorgibt.
- Relationales Datenbankverhalten wird nicht ausschließlich mit einem nicht relationalen In-Memory-Provider beurteilt; bei Bedarf SQLite in-memory oder eine isolierte Testdatenbank verwenden.

```csharp
[Fact]
public async Task ApproveAsync_WhenUserHasNoPermission_ReturnsForbidden()
{
    // Arrange
    // Act
    // Assert
}
```

## 14. Konfiguration und Secrets

- `ASPNETCORE_ENVIRONMENT` steuert das Hosting-Verhalten.
- `appsettings.json` enthält nur unkritische, umgebungsunabhängige Werte.
- Lokale Secrets über .NET User Secrets.
- Konfiguration wird über Options-Klassen unter `Logic/Configuration` gebunden und beim Start validiert.

## 15. Code-Stil und statische Analyse

Es wird **kein kostenpflichtiges Zusatzwerkzeug** vorausgesetzt. Stil und Analyse laufen vollständig über das .NET SDK.

### Konfiguration

Die verbindlichen Stileinstellungen stehen in `.editorconfig` im Repository-Root. Roslyn liest die Datei automatisch — in der IDE, im Build und in CI.

Damit Stilverstöße auch im Build auffallen, gehört in `Directory.Build.props` beziehungsweise in jede Projektdatei:

```xml
<PropertyGroup>
  <Nullable>enable</Nullable>
  <ImplicitUsings>enable</ImplicitUsings>
  <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
  <EnableNETAnalyzers>true</EnableNETAnalyzers>
  <AnalysisLevel>latest-recommended</AnalysisLevel>
</PropertyGroup>
```

Ohne `EnforceCodeStyleInBuild` melden die `IDE`-Regeln nur in der IDE, nicht im Build.

### Befehle

```powershell
dotnet format <Solution>.sln --include <geaenderte Dateien>
dotnet format <Solution>.sln --verify-no-changes
dotnet build <Solution>.sln -warnaserror
```

- `dotnet format` ist Teil des SDK, es ist keine Installation nötig.
- Beim Formatieren werden ausschließlich die **geänderten Dateien** über `--include` angegeben.
- `--verify-no-changes` ist die Prüfvariante für CI: sie ändert nichts und schlägt bei Abweichung fehl.
- `dotnet format` umfasst Whitespace, Stilregeln und Analyzer. Einzeln ansteuerbar über `dotnet format whitespace`, `style` beziehungsweise `analyzers`.

### Regeln

- Umformatierungen außerhalb der fachlichen Änderung werden zurückgenommen.
- Warnungen in geänderten Dateien blockieren den Abschluss.
- Befunde in unverändertem Bestandscode werden nicht nebenbei behoben, sondern im Abschlussbericht benannt.
- Regeln werden nicht per `#pragma warning disable` oder Absenkung in `.editorconfig` stillgelegt, nur um einen Lauf grün zu bekommen. Eine Ausnahme braucht einen dokumentierten Grund.
- Änderungen an `.editorconfig` sind eine Regeländerung und damit ein eigener, begründeter Commit.

### Optionale Ergänzungen

Zusätzliche Analyzer-Pakete sind zulässig, aber nicht vorgeschrieben. Wird eines eingeführt, gilt `code.md` Abschnitt 14: konkreter Bedarf, Begründung im Pull Request, Version explizit.

## 16. Befehle der Abschlussprüfung

```powershell
dotnet restore
dotnet build <Solution>.sln -warnaserror
dotnet test
dotnet format <Solution>.sln --include <geaenderte Dateien>
dotnet format <Solution>.sln --verify-no-changes
```

Zusätzlich alle vorhandenen Analyzer-, Security- und Frontend-Prüfungen ausführen.
