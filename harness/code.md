# Code- und Qualitätsregeln

Dieses Dokument definiert verbindliche Code- und Qualitätsregeln. Architekturregeln stehen in `harness/design.md`, UI-Regeln in `harness/uiux.md`, Sicherheitsregeln in `harness/security.md`, Integrationsregeln in `harness/integrations.md`. Sprach- und framework-spezifische Regeln stehen in `harness/profile.md`.

## 1. Grundsatz

Code muss fachlich korrekt, verständlich, wartbar, testbar und sicher sein.

### Geltungsbereich

Das Regelwerk gilt verbindlich für **neuen und geänderten Code**.

Bestehender Code, der davon abweicht, wird:

- nicht nebenbei refactored,
- nicht stillschweigend an die Regeln angepasst,
- im Abschlussbericht benannt,
- nur auf ausdrücklichen Auftrag umgebaut.

Wird eine Datei fachlich geändert, gilt das Regelwerk für die geänderten Stellen. Der Rest der Datei bleibt unangetastet, auch wenn er abweicht.

Bekannte, bewusst akzeptierte Abweichungen des Bestands stehen in `harness/BESTANDSABWEICHUNGEN.md`, sofern das Regelwerk in ein bestehendes Projekt eingeführt wurde. Eine dort geführte Abweichung ist kein Freibrief für neuen Code, der sie fortsetzt.

Wenn eine beauftragte Änderung ohne Berührung einer bekannten Abweichung nicht sinnvoll umsetzbar ist, wird das benannt und der Umbau als eigener Scope vorgeschlagen, statt ihn unangekündigt mitzuerledigen.

Warnungen werden nicht unterdrückt, um Prüfungen künstlich grün zu machen. Eine Unterdrückung benötigt einen konkreten, dokumentierten Grund.

Maßgeblich sind in dieser Reihenfolge:

1. die in `harness/profile.md` genannte Stil- und Analyzer-Konfiguration,
2. vorhandene Formatter-, Linter- und Editor-Konfiguration im Repository,
3. der Stil des umgebenden Codes.

Formatierung ist nie Teil einer fachlichen Änderung. Dateien werden nicht nebenbei vollständig umformatiert.

## 2. Benennung

Die konkreten Schreibweisen je Sprache stehen in `harness/profile.md`. Stack-unabhängig gilt:

- Namen beschreiben die fachliche Bedeutung, nicht lediglich den Datentyp.
- Keine unklaren Abkürzungen und keine ungarische Notation.
- Neue Projektpräfixe oder Akronyme werden nicht ohne dokumentierte Namensentscheidung eingeführt.
- Pro Schicht und fachlichem Kontext wird eine konsistente Domänensprache verwendet.
- Die Schreibweise externer Vertragsfelder wird exakt übernommen, auch wenn sie der internen Konvention widerspricht.
- Ein einmal gewähltes Suffix für Anwendungsfallklassen wird im gesamten Projekt konsistent verwendet.
- Integrationsklassen tragen einen fachlich-technischen Namen, der System und Zweck erkennen lässt.

## 3. Dateien und Module

- Ein Top-Level-Typ beziehungsweise eine klar erkennbare Hauptverantwortung pro Datei.
- Der Dateiname entspricht dem enthaltenen Haupttyp.
- Modul- oder Namespace-Pfad entspricht der Ordnerstruktur.
- Unbenutzte Importe entfernen.
- Keine auskommentierten Codeblöcke committen.
- Generierter Code wird nicht manuell wie regulärer Anwendungscode gepflegt.
- Keine toten Dateien, ungenutzten Exporte oder verwaisten Hilfsklassen zurücklassen.

## 4. Formatierung

- Einrückung, Zeilenlänge und Klammerstil folgen `harness/profile.md`.
- Eine Anweisung pro Zeile.
- Keine mehrfachen Leerzeilen.
- Formatierung außerhalb der fachlich geänderten Bereiche wird vermieden.
- Automatische Formatierer werden verwendet, statt Stil manuell nachzuziehen.

## 5. Allgemeiner Codestil

- Typinferenz verwenden, wenn der Typ aus dem Kontext eindeutig erkennbar ist; sonst explizit annotieren.
- Null-, Undefined- und Optional-Semantik ernst nehmen; keine Unterdrückung der Typprüfung ohne belegbaren Grund.
- Guard Clauses bevorzugen, wenn sie Verschachtelung reduzieren.
- Pattern Matching, Destrukturierung und Null-Operatoren einsetzen, wenn sie die Lesbarkeit erhöhen.
- Keine cleveren Einzeiler zulasten der Verständlichkeit.
- Funktionale Ketten und Query-Ausdrücke nur verwenden, wenn Ausdruck, Übersetzung und Performance verständlich bleiben.
- Keine unnötigen Materialisierungen oder mehrfachen Durchläufe derselben Sequenz.
- Keine versteckten Seiteneffekte in Queries, Properties, Gettern oder Mapping-Funktionen.
- Magische Zahlen und Strings durch benannte Konstanten ersetzen, wenn sie fachliche Bedeutung tragen.

## 6. Asynchronität und Parallelität

- Asynchronen Code durchgängig asynchron implementieren; nicht blockierend auf Ergebnisse warten.
- Abbruch-Token beziehungsweise Abbruchsignale bei externen, datenbankbezogenen und länger laufenden Vorgängen weiterreichen.
- Unabhängige Aufgaben nur parallelisieren, wenn Thread-Sicherheit, Fehlerbehandlung, Reihenfolge und Last berücksichtigt sind.
- Hintergrundprozesse müssen kontrolliert beendet werden können.
- Fehler in nebenläufigen Aufgaben werden nicht verschluckt.
- Keine unkontrollierten Fire-and-forget-Aufrufe für fachlich relevante Vorgänge.

## 7. Abhängigkeiten im Code

- Abhängigkeiten explizit über Konstruktoren, Parameter oder definierte Container injizieren.
- Keine Service-Locator-Aufrufe und keine versteckten globalen Zustände.
- Lifetimes und Scopes bewusst wählen und dokumentieren, wenn sie nicht offensichtlich sind.
- Kein langlebiger Datenbankkontext oder Request-Zustand in langlebigen Objekten.
- Neue Interfaces oder Abstraktionen nur einführen, wenn eine echte Grenze, mehrere Implementierungen oder ein sinnvoller Test-Seam besteht.
- Keine Wrapper-Abstraktion, die ein Framework unverändert weiterreicht.

## 8. Geschäftslogik

- Geschäftslogik liegt in der dafür vorgesehenen Schicht laut `harness/design.md`.
- Methoden bilden fachlich verständliche Anwendungsfälle ab.
- Transaktionsgrenzen sind bewusst gesetzt und liegen in der Anwendungslogik.
- Validierung und serverseitige Autorisierung erfolgen vor zustandsverändernden Aktionen.
- UI-Komponenten orchestrieren nur Darstellung und Interaktion.
- Keine Geschäftsregeln in Templates, Markup, Client-Skripten, Mapping-Konfigurationen, Controllern oder Integrationsclients verstecken.
- Statusübergänge werden explizit geprüft; ungültige Übergänge werden fachlich abgewiesen.
- Kritische Vorgänge sind gegen doppelte Ausführung abzusichern.
- Wiederverwendung wird nicht durch generische Abstraktionen erzwungen, wenn fachliche Regeln unterschiedlich sind.

## 9. Validierung, Autorisierung und Sicherheit

- Externe und benutzergesteuerte Eingaben werden serverseitig validiert.
- Länge, Typ, Format, Wertebereich, Status, Eindeutigkeit und fachliche Berechtigung prüfen.
- Die UI steuert Sichtbarkeit und Benutzerführung; das Backend erzwingt Berechtigungen unabhängig davon erneut an der geschützten Anwendungsfall- oder API-Grenze.
- Keine Datenbankabfragen aus Benutzereingaben zusammensetzen; parametrisierte Abfragen verwenden.
- Keine internen Details in Fehlermeldungen ausgeben.
- Keine Secrets, produktiven Daten oder echten Zahlungsinformationen im Repository.
- Keine sensitiven Werte in Logs.
- Keine eigenen Kryptografie-, Token-, Passwort- oder Signaturverfahren entwickeln. Bewährte Framework- und Providermechanismen verwenden.
- Detailregeln stehen in `harness/security.md`.

## 10. Integrationen und externe Verträge

- Externe Systeme werden ausschließlich über definierte Adapter und Verträge angebunden.
- Fachliche Geschäftslogik gehört nicht in HTTP-Clients, Webhook-Handler oder Mapping-Code.
- Externe DTOs werden nicht ungeprüft als interne Domänen- oder UI-Modelle verwendet.
- Timeouts, Abbruch, Wiederholung und Fehlerklassifikation werden explizit behandelt.
- Schreibende Integrationen müssen Idempotenz und doppelte Zustellung berücksichtigen.
- Webhook-Verarbeitung prüft Authentizität und speichert einen nachvollziehbaren Verarbeitungsstatus.
- Externe IDs, Versionen und Korrelationsmerkmale werden bewusst verwaltet.
- Eine konkrete Automatisierungstechnologie wird nicht vorausgesetzt.
- Detailregeln stehen in `harness/integrations.md`.

## 11. Fehlerbehandlung und Logging

- Fehler nicht verschlucken und keine leeren Catch-Blöcke verwenden.
- Nur Fehler fangen, die sinnvoll behandelt werden können.
- Beim erneuten Auslösen den ursprünglichen Stack erhalten.
- Erwartete fachliche Fehler von technischen Fehlern unterscheiden.
- Logs mit strukturierten Parametern statt Stringverkettung schreiben.
- Keine doppelte Protokollierung desselben Fehlers auf mehreren Schichten.
- Log-Level bewusst verwenden:
  - `Debug` für Entwicklungsdiagnose,
  - `Information` für relevante erfolgreiche Abläufe,
  - `Warning` für beherrschte Auffälligkeiten,
  - `Error` für fehlgeschlagene Vorgänge,
  - `Critical` für nicht fortsetzbare oder systemweite Zustände.
- Korrelations- und Vorgangs-IDs verwenden, wenn mehrere Systeme oder Hintergrundschritte beteiligt sind.

## 12. Dokumentation und Kommentare

- Öffentliche, wiederverwendbare und fachlich nicht triviale Member besitzen eine aussagekräftige Kurzbeschreibung im Dokumentationsformat der Sprache.
- Kommentare erklären das Warum, nicht den offensichtlichen Code.
- Kommentare und Dokumentation verwenden innerhalb eines Projekts eine einheitliche Sprache.
- `TODO` enthält einen konkreten Auftrag oder eine Ticketreferenz.
- Keine Dokumentation erzeugen, die nur Typ- oder Methodennamen wiederholt.
- Externe Verträge und nicht offensichtliche Mapping-Entscheidungen werden dokumentiert.

## 13. Tests

- Zu jedem Modul mit Geschäftslogik existiert ein Testziel; Benennung nach `harness/profile.md`.
- Testname beschreibt Ausgangslage, Aktion und Ergebnis.
- Tests sind deterministisch und voneinander unabhängig.
- Keine produktiven Daten, Secrets, reale Uhrzeit oder unkontrollierte externe Systeme verwenden.
- Neue oder geänderte Geschäftslogik ohne Tests gilt als unfertig.
- Geschäftslogik benötigt Positiv-, Negativ- und Grenzfälle.
- Berechtigungs-, Validierungs- und Statusregeln werden getestet.
- Integrationen werden über kontrollierte Fakes, Stub-Server oder Testumgebungen geprüft.
- Datenbankverhalten wird nicht ausschließlich mit einem abweichenden In-Memory-Ersatz beurteilt; bei Bedarf eine strukturgleiche isolierte Testdatenbank verwenden.
- Für reproduzierbare Fehler nach Möglichkeit einen Regressionstest ergänzen.
- Keine dauerhaft ignorierten oder auskommentierten Tests.

## 14. Abhängigkeiten und Pakete

- Neue Pakete nur bei konkretem Bedarf.
- Vorhandene, freigegebene Projektpakete bevorzugen, wenn sie die Anforderung geeignet erfüllen.
- Eine Komponentenbibliothek ist keine Begründung für eine ungeeignete Lösung.
- Native Sprach- und Plattformmittel sind zu bevorzugen, wenn sie einfacher, zugänglicher oder besser anpassbar sind.
- Paketversionen explizit und reproduzierbar halten; Lockdatei committen.
- Keine Paketaktualisierung als Nebenwirkung einer fachlichen Änderung.
- Neue oder geänderte Abhängigkeiten im Pull Request begründen.
- Nicht mehr benötigte Pakete entfernen, sofern dies zum Scope der Änderung gehört.

## 15. Abschlussprüfung

Vor Abschluss einer Änderung werden die in `harness/profile.md` definierten Befehle für Restore, Build, Test, Format und statische Analyse ausgeführt.

Regeln für statische Analyse:

- Der Lauf muss die projekteigene Konfiguration tatsächlich laden.
- Automatische Formatierung läuft ausschließlich auf geänderten Dateien.
- Umformatierungen außerhalb der fachlichen Änderung werden zurückgenommen.
- Befunde ab Warnstufe in geänderten Dateien blockieren den Abschluss.
- Befunde in unverändertem Bestandscode werden nicht nebenbei behoben, sondern im Abschlussbericht benannt.
- Prüfungen werden nicht lokal unterdrückt oder abgesenkt, nur um einen Lauf grün zu bekommen.
- Berichtsdateien werden nicht committet.

Die Änderung ist nicht fertig, wenn:

- Build oder relevante Tests fehlschlagen,
- neue Warnungen in geänderten Dateien bestehen,
- Akzeptanzkriterien nicht geprüft wurden,
- Architekturgrenzen verletzt sind,
- Berechtigungs-, Validierungs- oder Integrationsfehlerfälle fehlen,
- Specs und Code auseinanderlaufen,
- neue externe Abhängigkeiten oder Datenflüsse nicht dokumentiert sind.

Der Abschlussbericht nennt:

- geänderte Dateien,
- umgesetzte Anforderungen,
- ausgeführte Befehle und Ergebnisse,
- nicht ausgeführte Prüfungen mit Begründung,
- verbleibende Risiken und offene Entscheidungen.

## 16. Branching und Pull Requests

Das verbindliche Branch-Modell dieses Projekts steht in `PROJECT_CONTEXT.md`.

### Feature-Branches

- Immer von einem aktuellen Integrationsbranch abzweigen.
- Ein Branch bildet genau eine Anforderung oder ein Ticket ab.
- Namensschema: `<typ>/<ticket>-<kurzbeschreibung>`.
- Ohne Ticket entfällt der Ticketteil.
- Kurzbeschreibung kleingeschrieben, mit Bindestrichen, ohne Umlaute und Sonderzeichen.
- Beispiele: `feat/123-bestellfreigabe`, `fix/141-zahlungsstatus-zuordnung`.
- Aktualisierung per Rebase auf den Integrationsbranch, nicht per Merge in den Feature-Branch.
- Force-Push nur auf den eigenen, noch nicht gemergten Feature-Branch.

### Promotion

- Feature-Branch in den Integrationsbranch: Squash-Merge.
- Zwischen dauerhaften Branches: Pull Request und Merge-Commit.
- Stufen werden nicht übersprungen.
- Gemergte Feature-Branches werden gelöscht.

### Hotfix

- Dringende Produktionsfehler werden auf `fix/<ticket>-<kurzbeschreibung>` vom Produktionsbranch behoben.
- Nach Merge wird derselbe Stand in alle vorgelagerten Branches zurückgeführt.

### Pull Request

- Erst eröffnen, wenn Abschlussprüfung und betroffene Specs aktuell sind.
- Beschreibung nennt Ticket, Anforderung, betroffene Specs, Datenbank-/Konfigurations-/Integrationsänderungen und neue Abhängigkeiten.
- Review erfolgt durch einen Menschen.

### Regeln für Coding Agents

- Vor der ersten Änderung den aktuellen Branch prüfen.
- Auf einem geschützten Branch zuerst einen Feature-Branch anlegen.
- Sind Typ und Ticket eindeutig, darf der Agent den Branch anlegen. Sonst nachfragen.
- Der Agent darf validierte Änderungen auf seinen Feature-Branch committen und pushen.
- Der Agent mergt nicht, löscht keine Branches und pusht nicht mit Force auf fremde Branches.

KI-generierter Code ist ein Vorschlag. Fachliche Verantwortung, Freigabe, Sicherheitsbewertung und unabhängiges Review verbleiben beim Menschen.
