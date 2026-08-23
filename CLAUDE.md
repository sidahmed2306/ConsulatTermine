# Harness für ConsulatTermine

Diese Datei ist die Einstiegsregel für Claude Code. Die Detailregeln stehen unter `harness/` und werden situativ gelesen, nicht auf Vorrat.

- **Projekt:** ConsulatTermine
- **Stack:** C# / ASP.NET Core / Blazor Server
- **Stack-Profil:** `harness/profile.md`
- **Projektkontext:** `PROJECT_CONTEXT.md`

## Kontext dynamisch nachladen

- `harness/design.md` immer lesen, bevor Projekt-, Schicht-, Daten-, Persistenz-, Konfigurations-, Logging-, Betriebs- oder Architekturfragen bearbeitet werden.
- `harness/profile.md` immer zusätzlich lesen, sobald konkreter Code, Ordnerstruktur, Benennung, Framework-Mechanismen oder Prüfbefehle betroffen sind.
- `harness/code.md` immer lesen, bevor Code geändert, ein Branch angelegt, ein Commit erstellt oder ein Pull Request vorbereitet wird.
- `harness/uiux.md` immer lesen, bevor UI oder UX verändert werden, einschließlich Komponenten, Markup, CSS, Tabellen, Grids, Formulare und responsive Darstellung.
- `harness/security.md` immer lesen, bevor Authentifizierung, Autorisierung, Benutzer- oder Mitarbeiterkonten, personenbezogene Daten, Zahlungsdaten, Secrets, Webhooks oder technische Identitäten betroffen sind.
- `harness/integrations.md` immer lesen, bevor externe Systeme, APIs, Synchronisation, Webhooks, Worker, Jobs oder sonstige Automatisierungen betroffen sind.
- `harness/requirements.md` immer lesen, bevor Epics, Features, Stories oder Akzeptanzkriterien unter `specs/` angelegt oder geändert werden.
- `harness/ideas.md` immer lesen, wenn eine neue Idee, ein neues Feature, eine neue Schnittstelle oder eine größere Änderung noch nicht als freigegebene Spec existiert. Vorher entstehen weder Specs noch Code.
- `harness/commit-messages.md` immer lesen, bevor eine Commit Message geschrieben wird.

Lies zusätzlich `PROJECT_CONTEXT.md`, `OPEN_DECISIONS.md`, vorhandene ADRs unter `docs/adr/`, betroffene Specs und den relevanten Bestandscode. Ermittle Informationen im Repository, statt zu raten.

## Entwicklungsworkflow

Jede fachliche Code-Änderung durchläuft in dieser Reihenfolge:

0. Neue Idee nach `harness/ideas.md` klären und unter `ideas/` dokumentieren, sofern noch keine freigegebene Spec existiert.
1. Spec nach `harness/requirements.md` unter `specs/` anlegen oder aktualisieren.
2. Feature-Branch nach `harness/code.md` anlegen. Niemals direkt auf einem geschützten Branch arbeiten.
3. Nur den beauftragten Scope implementieren.
4. Relevante Unit-, Integrations- und Regressionstests ergänzen oder anpassen.
5. Build und Tests nach `harness/profile.md` ausführen.
6. Formatierung und statische Analyse nach `harness/profile.md` ausführen.
7. Specs, ADRs, Dokumentation und Code synchron halten.
8. Abschlussbericht mit geänderten Dateien, Prüfungen, Ergebnissen, Risiken und offenen Entscheidungen erstellen.

## Geltungsbereich

Das Regelwerk gilt verbindlich für **neuen und geänderten Code**.

Bestehender Code, der davon abweicht, wird nicht nebenbei refactored und nicht stillschweigend angepasst. Er wird im Abschlussbericht benannt und nur auf ausdrücklichen Auftrag umgebaut. Wird eine Datei fachlich geändert, gilt das Regelwerk für die geänderten Stellen; der Rest der Datei bleibt unangetastet.

Bekannte Abweichungen des Bestands sind in `harness/BESTANDSABWEICHUNGEN.md` dokumentiert.

## Nicht verhandelbar

- Der in `PROJECT_CONTEXT.md` festgelegte Architekturstil und die vorhandene Projektstruktur werden beibehalten. Abweichungen benötigen einen konkreten Grund und gegebenenfalls ein ADR.
- Geschäftslogik liegt in der dafür vorgesehenen Schicht, nicht in UI-Komponenten, Templates, Domänenobjekten, Client-Skripten oder Mapping-Konfigurationen.
- Die Präsentationsschicht greift nicht direkt auf Persistenzdetails, Migrationen oder konkrete Integrationsclients zu.
- Persistenzrelevante Validierung wird serverseitig durchgesetzt.
- Geschützte Aktionen werden serverseitig autorisiert. UI-Prüfungen steuern zusätzlich Sichtbarkeit und Benutzerführung, ersetzen aber keine Backend-Prüfung.
- Öffentliche und interne Bereiche werden sicherheitstechnisch getrennt behandelt.
- Eine Komponentenbibliothek ist ein optionales Werkzeug, keine Pflicht und keine Begründung für ungeeignete UI.
- Es gibt keine verpflichtende Workflow-, Low-Code- oder Automatisierungsplattform. Die Lösung wird pro Anforderung begründet gewählt.
- Keine Secrets, produktiven Zugangsdaten oder sensiblen Beispielwerte im Repository.
- Keine Passwörter, Tokens, API-Keys, vollständigen Connection Strings, Zahlungsdaten oder unnötigen personenbezogenen Daten loggen.
- Keine Fachregeln, Rollen, Rechte, Felder, Statuswerte, Konfigurationsschlüssel, Datenquellen oder Schnittstellen erfinden.
- Externe Systeme werden über klar definierte Adapter und Verträge angebunden; direkter Zugriff auf fremde Datenbanken ist keine Standardschnittstelle.
- Kritische Integrationen berücksichtigen Idempotenz, Wiederholung, Fehlerbehandlung, Auditierung und Monitoring.
- Neue oder geänderte Geschäftslogik ohne geeignete Tests gilt als unfertig.
- Code-Stil und Inspektionen richten sich nach `harness/profile.md` und der vorhandenen Formatter- und Analyzer-Konfiguration.
- Keine neue Abstraktion, Bibliothek, Infrastruktur oder Architekturänderung ohne konkreten Bedarf. Es gelten SRP, KISS, DRY und YAGNI.
- Paket- und Framework-Versionen werden nicht nebenbei aktualisiert.
- Commit Messages folgen `harness/commit-messages.md`.
- Branching folgt `harness/code.md` und dem Branch-Modell aus `PROJECT_CONTEXT.md`.

KI-generierter Code ist ein Vorschlag. Fachliche Verantwortung, Freigabe, Sicherheitsbewertung und unabhängiges Review verbleiben beim Menschen.

## Änderungen am Regelwerk

Dateien unter `harness/` sind das verbindliche Regelwerk dieses Projekts. Sie werden nicht nebenbei im Rahmen einer fachlichen Änderung angepasst. Eine Regeländerung ist ein eigener, begründeter Commit.

Projektspezifische Fakten gehören nach `PROJECT_CONTEXT.md`, in Specs oder in ADRs, nicht in die generischen Regeldateien.
