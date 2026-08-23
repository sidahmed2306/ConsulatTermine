# Integrations- und Automatisierungsregeln

Dieses Dokument definiert verbindliche Regeln für externe Systeme, APIs, Webhooks, Synchronisation und Hintergrundprozesse. Welche Fremdsysteme dieses Projekt tatsächlich anbindet, steht in `PROJECT_CONTEXT.md`.

## 1. Grundsatz

Integrationen werden technologieoffen entworfen. Es ist keine bestimmte Workflow-, Low-Code-, Orchestrierungs- oder Messaging-Plattform vorgeschrieben.

Für jede Anforderung wird entschieden, ob sie am besten umgesetzt wird durch:

- vorhandene Funktionen eines Fachsystems,
- Geschäftslogik in der eigenen Anwendung,
- einen Worker- oder Hintergrunddienst,
- direkte API- oder Webhook-Integration,
- einen geplanten Hintergrundjob,
- eine Queue-basierte Verarbeitung,
- eine begründet ausgewählte externe Orchestrierungslösung.

Die Entscheidung richtet sich nach Fachlichkeit, Zuverlässigkeit, Transaktionsbedarf, Wartbarkeit, Sicherheit, Testbarkeit, Betriebsaufwand, Kosten und Austauschbarkeit.

## 2. Systemgrenzen und Datenhoheit

Vor einer Integration werden dokumentiert:

- Zweck und Konsumenten,
- verantwortliches Quellsystem je Datenart,
- erlaubte Leser und Schreiber,
- Datenvertrag und Versionierung,
- Aktualisierungsrichtung,
- erwartete Häufigkeit und Datenmenge,
- Fehler- und Wiederholungsstrategie,
- Datenschutz und Aufbewahrung,
- Monitoring und betriebliche Verantwortung.

Es wird keine pauschale Datenhoheit angenommen. Welches System für eine Datenart verantwortlich ist, wird anhand des realen Bestands entschieden und dokumentiert, nicht geraten.

## 3. Adapter und Verträge

- Externe Systeme werden über klar benannte Adapter oder Clients angebunden.
- Externe DTOs bleiben an der Integrationsgrenze.
- Mapping in interne Modelle erfolgt explizit und testbar.
- Die UI kennt keine externen API-Verträge.
- Geschäftslogik hängt von fachlichen Interfaces ab, nicht von konkreten HTTP-Clients.
- Ein Interface wird nur eingeführt, wenn eine echte Systemgrenze oder ein sinnvoller Test-Seam besteht.
- Anbieter- oder versionsspezifische Logik bleibt im jeweiligen Adapter.

Schematischer Aufbau:

```text
UI-Komponente
    ↓
<Anwendungsfall>Manager        (Geschäftslogik, Autorisierung, Transaktion)
    ↓
I<Fachliche Leistung>Service   (fachliches Interface)
    ↓
<Fremdsystem><Zweck>Adapter    (Protokoll, Mapping, Fehlerklassifikation)
```

## 4. API-Verträge

Vor Einführung oder Änderung einer API sind zu klären:

- Authentifizierung und Autorisierung,
- Scope und Zweck,
- Versionierung,
- Request- und Response-Verträge,
- Fehlerformat,
- Pagination und Limits,
- Timeouts,
- Rate Limits,
- Rückwärtskompatibilität,
- Monitoring und Supportverantwortung.

OpenAPI-Dokumentation wird für eigene externe HTTP-APIs bereitgestellt, sofern kein anderer freigegebener Vertragsstandard gilt.

## 5. Webhooks und Ereignisse

- Ereignisse werden nur eingeführt, wenn eine zeitnahe oder entkoppelte Reaktion tatsächlich benötigt wird.
- Eventnamen beschreiben ein bereits eingetretenes fachliches oder technisches Ereignis.
- Webhook-Authentizität wird nach `harness/security.md` geprüft.
- Doppelte, verspätete und außerhalb der Reihenfolge eintreffende Ereignisse werden berücksichtigt.
- Der Empfang wird schnell bestätigt; längere Verarbeitung wird gegebenenfalls entkoppelt.
- Eingegangene Ereignisse erhalten eine Korrelations- und gegebenenfalls externe Event-ID.
- Unbekannte Versionen oder Eventtypen werden kontrolliert behandelt.

## 6. Idempotenz

Schreibende Integrationen und wiederholbare Hintergrundprozesse müssen sicher mit Wiederholungen umgehen.

- Für kritische Vorgänge wird ein stabiler Idempotenzschlüssel verwendet.
- Externe Vorgangs-, Zahlungs-, Rechnungs- und Ereignis-IDs werden eindeutig zugeordnet.
- Ein Retry darf keinen zweiten Auftrag, keine zweite Zahlungsanforderung und keine doppelte Kommunikation erzeugen.
- Bereits erfolgreich verarbeitete Vorgänge werden erkannt.
- Teilweise erfolgreiche Abläufe besitzen einen nachvollziehbaren Zustand und eine definierte Fortsetzung oder Kompensation.

## 7. Timeouts, Wiederholung und Fehlerklassifikation

Fehler werden mindestens unterschieden in:

- fachlich abgewiesen,
- nicht autorisiert oder nicht authentifiziert,
- temporär technisch fehlgeschlagen,
- dauerhaft technisch fehlgeschlagen,
- ungültiger oder inkompatibler Vertrag,
- manueller Klärungsbedarf.

Regeln:

- Jeder externe Aufruf besitzt einen angemessenen Timeout.
- Retries nur bei wahrscheinlich temporären und sicher wiederholbaren Fehlern.
- Exponentielles Backoff und Jitter verwenden, wenn viele Wiederholungen möglich sind.
- Rate-Limit-Antworten respektieren.
- Keine unendlichen Wiederholungsschleifen.
- Nach ausgeschöpften Versuchen wird ein kontrollierter Fehlerzustand erzeugt.
- Manuelle Wiederholung ist nur zulässig, wenn sie autorisiert, auditiert und idempotent ist.

## 8. Hintergrundprozesse

- Hintergrundprozesse besitzen einen klaren fachlichen Zweck und Owner.
- Start, Stopp und kontrollierte Beendigung werden unterstützt.
- Parallelität wird begrenzt und bewusst gewählt.
- Mehrfachinstanzen dürfen denselben Vorgang nicht unkontrolliert parallel bearbeiten.
- Zeitgesteuerte Prüfungen speichern einen verlässlichen Fortschritt oder nutzen eine eindeutige Abfragegrenze.
- Fehler eines einzelnen Vorgangs dürfen nicht den gesamten Prozess dauerhaft blockieren.
- Lange Aufgaben werden beobachtbar und abbrechbar gestaltet.

## 9. Menschliche Freigaben

- Menschliche Entscheidungen werden als fachlicher Zustand und nicht als bloße UI-Aktion modelliert.
- Vor der Freigabe zeigt das System alle entscheidungsrelevanten, verifizierten Informationen.
- Freigaben werden serverseitig autorisiert und auditiert.
- Nach der Freigabe ausgelöste Schritte sind idempotent.
- Ablehnung, Rückfrage, Ablauf und erneute Einreichung werden berücksichtigt, sofern fachlich relevant.
- Eine Automatisierung darf eine vorgeschriebene menschliche Freigabe nicht umgehen.

## 10. Synchronisation und Konflikte

- Push über Webhooks oder Events wird bevorzugt, wenn das Quellsystem dies zuverlässig unterstützt.
- Polling ist zulässig, wenn keine geeignete Push-Schnittstelle existiert oder ein Abgleich erforderlich ist.
- Für kritische Daten wird ein regelmäßiger Reconciliation-Abgleich erwogen, auch wenn Webhooks existieren.
- Konfliktregeln werden je Datenart dokumentiert.
- Zeitstempel allein sind nicht automatisch eine fachlich korrekte Konfliktstrategie.
- Änderungen im Zielsystem werden nicht überschrieben, wenn dessen Datenhoheit nicht geklärt ist.
- Manuelle Änderungen und automatische Synchronisation dürfen sich nicht unkontrolliert gegenseitig überschreiben.

## 11. Nachvollziehbarkeit und Monitoring

Für relevante Integrationsvorgänge werden mindestens erfasst:

- Vorgangs- oder Korrelations-ID,
- Quell- und Zielsystem,
- Ereignisart oder Operation,
- Start- und Endzeit,
- Ergebnis und Fehlerkategorie,
- Anzahl der Versuche,
- externer Referenzschlüssel,
- gegebenenfalls manueller Klärungsstatus.

Nicht protokolliert werden Secrets, vollständige Zahlungsdaten oder unnötige personenbezogene Payloads.

Monitoring berücksichtigt:

- Fehlerrate,
- ausstehende Vorgänge,
- Alter der ältesten Aufgabe,
- Wiederholungen,
- nicht verarbeitbare Vorgänge,
- Erreichbarkeit externer Systeme,
- ungewöhnliche Mengen oder Laufzeiten.

## 12. Tests

Mindestens testen:

- korrektes Mapping,
- fehlende Pflichtdaten,
- fachliche Ablehnung,
- Timeout,
- temporärer Fehler und Retry,
- dauerhafter Fehler,
- doppelte Zustellung,
- Ereignisse außerhalb der Reihenfolge,
- Teilfehler,
- Authentifizierungs- und Berechtigungsfehler,
- manuelle Wiederholung,
- keine doppelte fachliche Wirkung.

Tests verwenden Fakes, Stub-Server, Contract-Tests oder freigegebene Sandbox-Systeme. Produktive Systeme werden nicht für automatisierte Tests verwendet.

## 13. Neue technische Lösungen

Bevor eine neue Plattform, Queue, Bibliothek, Cloud-Komponente oder externe Orchestrierung eingeführt wird, legt der Agent vor:

- konkretes Problem,
- bestehende einfachere Alternativen,
- zwei bis drei geeignete Optionen,
- Nutzen und Nachteile,
- Sicherheits- und Datenschutzwirkung,
- Betriebs- und Kostenwirkung,
- Migrations- und Austauschbarkeit,
- Empfehlung.

Eine grundlegende Entscheidung wird vor der Implementierung als ADR freigegeben.
