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
| Sprache und Framework | C# 14 / ASP.NET Core 10 / Blazor Server (net10.0) |
| Zusätzliche Sprachen | JavaScript ausschliesslich fuer Browser-Aufgaben via JS Interop (Kulturumschaltung, Audio-Wiedergabe im Wartezimmer) |
| Datenhaltung | SQL Server über EF Core 10, Code-First mit Migrationen |
| Testframework | xUnit v3, Testdatenbank SQLite in-memory |
| Komponentenbibliothek | MudBlazor 8.15 |

## 3. Struktur

Die verbindliche Ordner- und Modulstruktur steht in `harness/profile.md`.

Dieses Projekt behält seine gewachsene Vier-Schichten-Struktur statt der
Zwei-Projekt-Aufteilung des Profils. Die Zuordnung steht in `harness/profile.md`
Abschnitt 3, die Begründung in `docs/adr/0002-bestehende-schichtstruktur-beibehalten.md`.

Weitere Abweichungen:

- Anwendungsfallklassen tragen den Suffix `Service`, nicht `Manager`. Der Suffix
  wird projektweit konsistent verwendet.
- Routbare Razor-Seiten des Bestands behalten ihre Dateinamen. Neue Seiten enden
  auf `Page.razor`.
- Paketversionen stehen zentral in `Directory.Packages.props`, Build-Einstellungen
  in `Directory.Build.props`.

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
| Anonyme Besucher | ja | keiner. Terminbuchung, Absage und Wartezimmer-Anzeige sind öffentlich |
| Endkunden / externe Benutzer | nein | Bürger buchen ohne Konto. Identifikation über `BookingReference` und `CancelToken` aus der Bestätigungsmail |
| Interne Mitarbeiter | ja | Cookie-Authentifizierung von ASP.NET Core gegen die Tabelle `Employees`, Passwort als PBKDF2-Hash. Siehe `docs/adr/0001-cookie-authentifizierung-fuer-den-mitarbeiterbereich.md` |
| Technische Systeme | nein | — |

### Rollen

Aufsteigend, jede Rolle schließt die Rechte der vorherigen ein:

| Rolle | Bedeutung |
|---|---|
| `Employee` | Bearbeitet Termine am eigenen Arbeitsplatz, ruft Bürger im Wartezimmer auf |
| `ServiceChef` | Zusätzlich Mitarbeiterverwaltung, Service-Zuweisungen und Arbeitszeiten |
| `Admin` | Zusätzlich Serviceverwaltung, Rollenvergabe und Löschen von Mitarbeitern |

### Gültige Policies

| Policy | Rollen | Verwendet auf |
|---|---|---|
| `Mitarbeiter.Zugriff` | Employee, ServiceChef, Admin | `/employee/home`, `/employee/dashboard`, `/employee/select-service`, `/employee/change-password` |
| `Dienstplan.Verwalten` | ServiceChef, Admin | `/admin/employees`, `/admin/assignments`, `/admin/working-schedule` |
| `Administration.Verwalten` | Admin | `/admin/services` |

Objektbezogene Rechte:

- Ein `Employee` darf über `IEmployeeService.GetEmployeeByIdAsync` nur den eigenen
  Datensatz lesen.
- Ein `ServiceChef` sieht in Zuweisungen und Arbeitszeiten nur die ihm
  zugewiesenen Services; ein `Admin` alle.
- Nur ein `Admin` darf Rollen ändern. Der letzte aktive Administrator und das
  eigene Konto können nicht gelöscht werden.

Die UI steuert über `AuthorizeView` nur Sichtbarkeit. Jede geschützte Aktion wird
zusätzlich serverseitig über `IEmployeeAuthorization` geprüft.

## 8. Externe Systeme

| System | Zweck | Datenhoheit | Schnittstelle | Status |
|---|---|---|---|---|
| SMTP-Postausgang | Terminbestätigung, Absage, Willkommens- und Passwort-Mails an Mitarbeiter | keine. Reiner Versand, keine Daten werden zurückgelesen | `System.Net.Mail.SmtpClient` über `SmtpEmailService` | in Betrieb |

Weitere Fremdsysteme sind derzeit nicht angebunden.

## 9. Umgebungen

| Umgebung | Vorhanden | Secret Store | Deployment |
|---|---|---|---|
| Development | ja | .NET User Secrets, `UserSecretsId` = `consulat-termine-ui-2025` | lokal über `dotnet run` |
| Staging | noch offen | noch offen | noch offen |
| Production | noch offen | noch offen | noch offen |

Fehlende kritische Konfiguration führt beim Start zu einem Fehler: die
Options-Klassen unter `ConsulatTermine.Application/Configuration` sind mit
`ValidateDataAnnotations().ValidateOnStart()` registriert. Die erwartete Struktur
steht in `ConsulatTermine.UI/appsettings.Example.json`.

## 10. Datenverantwortung

Je Datenart wird festgehalten, welches System Master ist und welches nur liest.

Alle fachlichen Daten liegen in der anwendungseigenen SQL-Server-Datenbank. Es
gibt kein führendes Fremdsystem.

| Datenart | Master | Schreiber | Leser | Verteilung |
|---|---|---|---|---|
| Termine (`Appointments`) | diese Anwendung | Bürger über die Buchung, Mitarbeiter über das Dashboard | Mitarbeiter, Wartezimmer-Anzeige | SignalR an `DisplayHub` und `EmployeeHub` |
| Mitarbeiter (`Employees`) | diese Anwendung | ServiceChef und Admin | Mitarbeiter (nur eigener Datensatz), ServiceChef, Admin | — |
| Services (`Services`) | diese Anwendung | Admin | alle | — |
| Arbeitszeiten und Pläne | diese Anwendung | ServiceChef und Admin | alle | — |

Aufbewahrung, Löschfristen und Auskunftsfähigkeit für die personenbezogenen Daten
in `Appointments` sind noch nicht geklärt, siehe `OPEN_DECISIONS.md` Nummer 14.

## 11. Monitoring und Betrieb

- Protokollierung: `Microsoft.Extensions.Logging` mit quellcodegenerierten
  Meldungen (`ServiceLog`, `UiLog`). Ausgabe bislang nur auf die Konsole.
- Zentrale Logs: noch offen
- Alerts: noch offen
- Integrationsstatus: noch offen
- Supportprozess: noch offen

## 12. Glossar

| Begriff | Bedeutung |
|---|---|
| Service | Dienstleistung des Konsulats, zum Beispiel Pass, Visa oder Standesamt. Nicht zu verwechseln mit einer C#-Klasse mit Suffix `Service` |
| Slot | Terminfenster eines Service an einem Tag. Länge über `Service.SlotDurationMinutes` |
| Kapazität | Zahl der Personen, die einen Slot gleichzeitig belegen können. Ergibt sich aus den dem Service zugewiesenen Mitarbeitern |
| Buchung | Ein Vorgang mit Hauptperson und optionalen Begleitpersonen, die je mehrere Services wählen können |
| BookingReference | Gemeinsame Referenz aller Termine einer Buchung, Format `CONSUL-<Jahr>-<6 Zeichen>` |
| CancelToken | Einmalwert im Absage-Link der Bestätigungsmail. Gültig bis 24 Stunden vor dem Termin |
| Override | Ausnahme von der regulären Öffnungszeit, entweder für ein Datum oder für einen Wochentag |
| Arbeitszeitplan | `WorkingSchedulePlan`: Gültigkeitszeitraum, unter dem Arbeitszeiten und Overrides eines Service hängen. Je Service ist höchstens einer aktiv |
| Wartezimmer | Öffentliche Anzeigetafel, die aufgerufene Bürger zeigt |
| Mitarbeiterkennung | Systemseitig vergebene Kennung im Format `CDZ-001` |
