# Design- und Architekturregeln

Dieses Dokument ist der technische Architekturvertrag des Projekts. Es gilt stack-unabhängig. Technologiespezifische Konkretisierungen stehen in `harness/profile.md`, projektspezifische Fakten in `PROJECT_CONTEXT.md`.

## Rangfolge bei Widersprüchen

1. freigegebene Sicherheits-, Datenschutz- und Organisationsrichtlinien,
2. aktuelle ADRs und verbindliche fachliche Entscheidungen,
3. dieses Dokument,
4. `harness/profile.md`,
5. `PROJECT_CONTEXT.md` und repository-spezifische Instructions,
6. bestehende lokale Patterns,
7. allgemeine Framework-Konventionen.

Widersprüche werden nicht stillschweigend aufgelöst. Der Agent benennt sie und klärt die Entscheidung vor einer strukturellen Änderung.

## 1. Architekturziele

Unabhängig vom gewählten Architekturstil gelten:

- hohe fachliche Kohäsion,
- geringe Kopplung,
- klare Schicht- und Verantwortungsgrenzen,
- möglichst wenig notwendige Infrastruktur,
- bewusst definierte Schnittstellen zu externen Systemen,
- austauschbare Integrationsadapter,
- nachvollziehbare fachliche Zustände.

Verbindliche Prinzipien sind SRP, KISS, DRY und YAGNI.

Zusätzliche Abstraktionsschichten, Message Broker, Event Sourcing, Microservice-Schnitte, Mediator- oder Repository-Pattern werden nicht allein aufgrund theoretischer Erweiterbarkeit eingeführt. Ein konkreter fachlicher oder technischer Bedarf muss dokumentiert sein.

Der für dieses Projekt gewählte Architekturstil steht in `PROJECT_CONTEXT.md` und wird nicht als Nebenwirkung einer fachlichen Änderung verändert.

## 2. Technologischer Rahmen

Der verbindliche Stack steht in `PROJECT_CONTEXT.md` und wird in `harness/profile.md` konkretisiert.

Stack-unabhängig gilt:

- Die Manifest-, Projekt- beziehungsweise Lockdatei ist für Framework- und Paketversionen maßgeblich.
- Ein Agent aktualisiert Versionen nicht ungefragt.
- Ein Agent migriert nicht als Nebenwirkung einer fachlichen Änderung auf ein anderes Framework, Build-System, Testframework oder UI-Toolkit.
- Verfügbare Sprachfeatures für Null-Sicherheit, Typprüfung und Strict-Modi bleiben aktiviert.
- Neue Sprachen oder Laufzeitumgebungen innerhalb desselben Repositorys benötigen ein ADR.

## 3. Repository und Projektstruktur

Ein Repository bündelt Anwendungen und Bibliotheken desselben fachlichen Scopes.

- Die verbindliche Ordner- und Modulstruktur steht in `harness/profile.md`.
- Bestehende, bereits freigegebene Modul- und Projektnamen werden nicht ohne Auftrag umbenannt.
- Neue Namen sind verständlich, ausgeschrieben und folgen der vorhandenen Konvention.
- Künstliche Abkürzungen werden nur nach einer dokumentierten Namensentscheidung eingeführt.
- Zu jedem Modul mit Geschäftslogik existiert ein zugehöriges Testziel.
- Nicht benötigte Ordner werden nicht vorsorglich angelegt.
- Ein separates Contract-Modul wird nur bei einer tatsächlichen externen Vertragsgrenze eingeführt.

## 4. Schichten und Verantwortlichkeiten

Das Projekt trennt mindestens:

| Schicht | Verantwortung |
|---|---|
| Präsentation | Darstellung, Benutzerinteraktion, Routing, Sichtbarkeitssteuerung |
| Anwendungslogik | Anwendungsfälle, Geschäftsregeln, Autorisierung, Transaktionsgrenzen |
| Validierung | wiederverwendbare serverseitige Prüfungen |
| Präsentationsmodelle | flache, auf konkrete Abfragen und Interaktionen zugeschnittene Modelle |
| Persistenz | Datenzugriff, Mappings, Migrationen |
| Domänenobjekte | Datenklassen ohne Präsentations- oder Infrastrukturlogik |
| Integrationen | Adapter, Clients, Mapping und technische Verträge zu externen Systemen |

Verbindliche Grenzen:

- Geschäftslogik liegt in der Anwendungslogik, nicht in UI-Komponenten, Templates, Domänenobjekten, Mapping-Konfigurationen oder Client-Skripten.
- Die Präsentationsschicht greift nicht direkt auf Datenbankkontexte, Persistenzdetails, Migrationen oder konkrete Integrationsclients zu.
- Die Präsentationsschicht kommuniziert nie direkt mit externen Fremdsystemen.
- Persistenzrelevante Validierung wird serverseitig durchgesetzt.
- Geschützte Aktionen werden serverseitig autorisiert. UI-Prüfungen steuern zusätzlich Sichtbarkeit und Benutzerführung.

Die konkrete Ordnerzuordnung dieser Schichten steht in `harness/profile.md`.

## 5. Datenverantwortung und Datenhaltung

Es gibt keine pauschal vorausgesetzte zentrale Datenbank für alle fachlichen Daten. Für jede Datenart wird dokumentiert:

- welches System fachlich verantwortlich ist,
- welches System schreiben darf,
- welche Systeme nur lesen,
- wie Änderungen verteilt werden,
- welche Daten lokal gespeichert werden müssen,
- welche Aufbewahrungs-, Audit- und Datenschutzregeln gelten.

Diese Zuordnung wird nicht geraten. Sie wird aus dem Bestand, den Schnittstellen und der fachlichen Entscheidung ermittelt und in `PROJECT_CONTEXT.md`, Specs oder ADRs dokumentiert.

### Anwendungseigene Daten

Die eigene Datenhaltung speichert nur Daten, für die die Anwendung selbst verantwortlich ist oder die für einen belastbaren Prozesszustand erforderlich sind, zum Beispiel:

- Freigabevorgänge,
- fachliche Statusübergänge,
- Zuordnung externer IDs,
- Idempotenzschlüssel,
- Integrations- und Wiederholungsstatus,
- Audit-Einträge,
- Benachrichtigungsaufgaben,
- anwendungseigene Präsentations- oder Konfigurationsdaten.

Externe Stammdaten, Preise, Bestände oder Zahlungsinformationen werden nicht unkontrolliert dupliziert. Eine lokale Kopie benötigt einen begründeten Zweck, eine Aktualisierungsstrategie und eine klare Datenhoheit.

### Grenzen zu externen Systemen

- Direkter Zugriff auf die Datenbank eines externen Systems ist keine Standardschnittstelle.
- Datenaustausch erfolgt über freigegebene APIs, Webhooks, Exporte, Nachrichten oder andere dokumentierte Integrationswege.
- Externe Daten werden über Adapter in anwendungsinterne Modelle übersetzt.
- Externe IDs werden explizit und nachvollziehbar zugeordnet.

## 6. Persistenz

Die technologiespezifischen Regeln zu ORM, Migrationen und Abfragen stehen in `harness/profile.md`.

Stack-unabhängig gilt:

- Domänenobjekte enthalten keine Geschäftslogik und keine Präsentationslogik.
- Schema wird bewusst definiert: Pflichtfelder, Längen, Beziehungen, Indizes und Eindeutigkeiten.
- Tabellen-, Spalten- beziehungsweise Feldnamen werden explizit und stabil festgelegt.
- Pro fachlichem Kontext wird eine konsistente Domänensprache verwendet.
- Read-only-Abfragen werden ohne unnötiges Change-Tracking ausgeführt, sofern die Technologie das unterscheidet.
- Abfragen laden nur die benötigten Felder und projizieren gezielt in Präsentationsmodelle.
- N+1-Abfragen und unkontrolliertes Nachladen von Beziehungen werden vermieden.
- Große Datenmengen benötigen Pagination mit stabiler Sortierung.
- Transaktionsgrenzen werden bewusst in der Anwendungslogik gesetzt.
- Schemaänderungen sind versioniert, nachvollziehbar und besitzen bei produktiven Daten eine Rollback- beziehungsweise Wiederherstellungsstrategie.
- Datenbankstrukturen externer Systeme werden nicht durch diese Anwendung migriert.

## 7. Validierung

- Persistenzrelevante und fachliche Validierung erfolgt serverseitig.
- UI-Validierung dient schnellem Feedback und ersetzt keine Backend-Prüfung.
- Pflichtfelder, Länge, Format, Wertebereich, Eindeutigkeit, Statusübergang und Berechtigung werden autoritativ serverseitig geprüft.
- Fehlermeldungen sind fachlich verständlich und enthalten keine internen Details.

## 8. Authentifizierung und Autorisierung

Die verbindlichen Detailregeln stehen in `harness/security.md`.

Architekturgrundsätze:

- Öffentliche Bereiche dürfen anonym erreichbar sein, sofern die Spec nichts anderes verlangt.
- Endkundenkonten, interne Mitarbeiterbereiche und technische Systemzugriffe werden getrennt behandelt.
- Geschützte Routen verwenden die Authentifizierungs- und policy-basierten Autorisierungsmechanismen des Frameworks.
- Die UI steuert Sichtbarkeit und Bedienbarkeit; das Backend prüft jede geschützte Aktion unabhängig davon erneut.
- Ein ausgeblendeter Button oder ein fehlender Menüeintrag ist keine Sicherheitsgrenze.
- Rollen, Policies und Scopes werden nicht erfunden, sondern fachlich definiert.

## 9. Konfiguration und Secrets

Mindestens die Umgebungen `Development`, `Staging` und `Production` werden getrennt behandelt, sofern die Deployment-Landschaft nichts anderes vorgibt.

- Eine definierte Umgebungsvariable steuert das Hosting-Verhalten.
- Eingecheckte Konfigurationsdateien enthalten nur unkritische, umgebungsunabhängige Werte.
- Lokale Secrets werden über einen dafür vorgesehenen lokalen Mechanismus bereitgestellt, nicht über eingecheckte Dateien.
- Staging- und Produktions-Secrets liegen in einem freigegebenen Secret Store oder der Deployment-Plattform.
- Keine Secrets in Konfigurationsdateien, Quellcode, Testdaten, Dokumentation oder exportierten Workflow-Definitionen.
- Konfiguration wird über typisierte Objekte gebunden und beim Start validiert.
- Produktive Konten, Secrets und Daten werden nicht in nichtproduktiven Umgebungen verwendet.
- Fehlende kritische Konfiguration führt zu einem klaren Startfehler statt zu spätem undefiniertem Verhalten.

## 10. Logging, Audit und Fehlerbehandlung

Logs enthalten nur die für Betrieb und Diagnose notwendigen Informationen:

- Ereignis und Ergebnis,
- Anwendung und Umgebung,
- Korrelations- oder Vorgangs-ID,
- nicht sensible technische oder fachliche Identifikatoren,
- Fehlerkategorie und Wiederholungsstatus.

Logs enthalten niemals:

- Passwörter, Tokens oder API-Keys,
- vollständige Connection Strings,
- vollständige Zahlungsdaten,
- sensible Request- oder Response-Bodies,
- unnötige personenbezogene oder vertrauliche Fachdaten.

Produktive Fehlermeldungen offenbaren keine Stack Traces, internen Pfade oder Implementierungsdetails. Diagnoseinformationen gehören in geschützte Logs.

Exceptions werden nur gefangen, wenn sie behandelt, fachlich übersetzt, kompensiert oder mit zusätzlichem Kontext erneut ausgelöst werden können.

Fachliches Audit und technisches Logging werden unterschieden. Kritische Freigaben und Statusänderungen müssen nachvollziehbar sein, ohne sensible Inhalte unnötig zu duplizieren.

## 11. Schnittstellen und Hintergrundprozesse

Die verbindlichen Detailregeln stehen in `harness/integrations.md`.

- APIs und Webhooks werden nur bei einer echten Systemgrenze eingeführt.
- Interne Anwendungsfälle werden innerhalb derselben Anwendung direkt aufgerufen.
- Automatisierungen sind technologieoffen. Eine konkrete Plattform ist nicht vorgeschrieben.
- Kritische Geschäftsregeln, Berechtigungen, Statusübergänge und Auditierung bleiben in der kontrollierten Anwendungsarchitektur nachvollziehbar.
- Hintergrundprozesse dürfen als Worker, geplanter Job, Queue-Verarbeitung, Fachsystemfunktion oder geeignete externe Orchestrierung umgesetzt werden, wenn die Entscheidung begründet ist.
- Neue Infrastruktur oder externe Dienste benötigen eine dokumentierte Entscheidung zu Nutzen, Betrieb, Sicherheit, Kosten, Testbarkeit und Austauschbarkeit.

## 12. Tests und Architekturprüfung

Sämtliche neue oder geänderte Geschäftslogik wird automatisiert getestet. Besonders zu berücksichtigen sind:

- Positiv-, Negativ- und Grenzfälle,
- Berechtigungen,
- Validierung,
- konkurrierende und wiederholte Aufrufe,
- Statusübergänge,
- Idempotenz bei Integrationen,
- Fehler- und Wiederholungsfälle,
- Regressionen behobener Fehler.

Vor Abschluss einer Änderung:

1. Projekt bauen.
2. Relevante Tests ausführen.
3. Statische Analyse und Repository-Checks ausführen.
4. Architekturgrenzen prüfen.
5. Spec-State prüfen.
6. Datenbank-, Konfigurations-, Berechtigungs- und Integrationsänderungen dokumentieren.

Die konkreten Befehle stehen in `harness/profile.md`.

## 13. Architekturänderungen

Ein ADR unter `docs/adr/` ist insbesondere erforderlich, wenn eine Änderung:

- Modul- oder Schichtgrenzen verändert,
- eine neue Integrationsart oder Infrastruktur einführt,
- Authentifizierung oder Autorisierung grundlegend ändert,
- einen neuen Persistenzansatz oder ein neues verantwortliches Datensystem einführt,
- eine neue externe Plattform oder kritische Abhängigkeit einführt,
- die Projekt- oder UI-Struktur verändert,
- eine verbindliche Harness-Regel ersetzt.

Der Coding Agent trifft solche Entscheidungen nicht stillschweigend. Er beschreibt Problem, Optionen, Vor- und Nachteile, Risiken und eine begründete Empfehlung.
