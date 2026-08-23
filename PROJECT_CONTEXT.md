# Projektkontext: ConsulatTermine

Diese Datei enthält die projektspezifischen Fakten. Die generischen Regeln stehen unter `harness/`. Bei Widersprüchen gilt die Rangfolge aus `harness/design.md`.

Stand: 2026-08-23

## 1. Projekt

| Feld | Wert |
|---|---|
| Name | ConsulatTermine |
| Kurzbeschreibung | Terminbuchungssystem des Konsulats Algerien in Frankfurt: oeffentliche Terminbuchung fuer Buergerinnen und Buerger, interner Mitarbeiterbereich mit Wartezimmer-Aufruf und Verwaltung. |
| Projekttyp | Web-Anwendung mit oeffentlichem Buergerbereich und internem Mitarbeiterbereich |
| Dokumentations- und UI-Sprache | Deutsch (UI zusaetzlich Englisch und Arabisch) |
| Öffentlicher Bereich vorhanden | ja |
| Interner Mitarbeiterbereich vorhanden | ja |

## 2. Technischer Stack

| Feld | Wert |
|---|---|
| Stack-Profil | dotnet-blazor |
| Sprache und Framework | C# / ASP.NET Core / Blazor Server |
| Zusätzliche Sprachen | JavaScript ausschliesslich fuer Browser-Aufgaben via JS Interop (Kulturumschaltung, Audio-Wiedergabe im Wartezimmer) |
| Datenhaltung | SQL Server ueber EF Core, Code-First mit Migrationen |
| Testframework | xUnit |
| Komponentenbibliothek | MudBlazor |

## 3. Struktur

Die verbindliche Ordner- und Modulstruktur steht in `harness/profile.md`.

Abweichungen oder Ergänzungen dieses Projekts:

- noch keine

## 4. Befehle

| Zweck | Befehl |
|---|---|
| Abhängigkeiten | `dotnet restore` |
| Build | `dotnet build ConsulatTermine.sln -warnaserror` |
| Tests | `dotnet test` |
| Format und Analyse | `dotnet format ConsulatTermine.sln --verify-no-changes` |
| Anwendung starten | `dotnet run --project ConsulatTermine.UI` |

## 5. Branch-Modell

| Branch | Zweck |
|---|---|
| `main` | dauerhafter Branch, Produktionsstand. Keine direkten Commits. |
| `<typ>/<kurzbeschreibung>` | Feature-Branch, zweigt von `main` ab, wird per Squash-Merge zurueckgefuehrt. |

Auf geschützten Branches wird nicht direkt committet oder gepusht.

## 6. Architekturstil

Schichtenarchitektur mit vier Projekten: Domain (Entities, Enums, ohne Abhaengigkeiten), Application (Interfaces, DTOs, UI-Modelle, reine Berechnungslogik), Infrastructure (EF Core, Manager mit Geschaeftslogik, SignalR, E-Mail), UI (Blazor Server). Abhaengigkeiten zeigen ausschliesslich nach innen.

## 7. Identitäten und Berechtigungen

| Identitätstyp | Vorhanden | Provider / Mechanismus |
|---|---|---|
| Anonyme Besucher | ja | - |
| Endkunden / externe Benutzer | nein (Buerger buchen ohne Konto, Identifikation ueber BookingReference und CancelToken) | noch offen |
| Interne Mitarbeiter | ja | noch offen |
| Technische Systeme | nein | noch offen |

Gültige Policies:

- noch nicht definiert

## 8. Externe Systeme

| System | Zweck | Datenhoheit | Schnittstelle | Status |
|---|---|---|---|---|
| SMTP-Postausgang | | | | |

## 9. Umgebungen

| Umgebung | Vorhanden | Secret Store | Deployment |
|---|---|---|---|
| Development | ja | noch offen | noch offen |
| Staging | noch offen | noch offen | noch offen |
| Production | noch offen | noch offen | noch offen |

## 10. Datenverantwortung

Je Datenart wird festgehalten, welches System Master ist und welches nur liest.

| Datenart | Master | Schreiber | Leser | Verteilung |
|---|---|---|---|---|
| noch nicht definiert | | | | |

## 11. Monitoring und Betrieb

- Zentrale Logs: noch offen
- Alerts: noch offen
- Integrationsstatus: noch offen
- Supportprozess: noch offen

## 12. Glossar

| Begriff | Bedeutung |
|---|---|
| | |
