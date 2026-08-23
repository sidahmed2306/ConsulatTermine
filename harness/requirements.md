# Anforderungs-Konventionen

Dieses Dokument definiert Struktur, Status und Qualitätsanforderungen für Epics, Features, Stories und Akzeptanzkriterien.

Specs beschreiben überprüfbares fachliches Verhalten. Architektur- und Implementierungsregeln stehen in `design.md`, `uiux.md`, `security.md`, `integrations.md` und `code.md`.

## 1. Struktur

```text
specs/
├── E{n} {Epic-Name}/
│   ├── _Datenstruktur.md
│   ├── _Menüstruktur.md
│   ├── _Schnittstellen.md
│   └── F{n} {Feature-Name}/
│       └── S{n} {Story-Name}.md
```

Nur benötigte Querschnittsdateien werden angelegt. Der Ordner `specs/` bleibt leer, bis die erste Spec entsteht.

- `_Datenstruktur.md`: fachliche Entities, Felder, Beziehungen, Constraints, Audit-/Soft-Delete-Verhalten und Datenverantwortung. Für jede relevante Datenart wird angegeben, ob sie anwendungseigen ist oder aus einem externen System stammt.
- `_Menüstruktur.md`: Navigation, Sichtbarkeit, Zielgruppe und erforderliche Policies.
- `_Schnittstellen.md`: externe APIs, Webhooks, Events, Importe, Exporte, Hintergrundprozesse und Datenflüsse. Keine Datei nur für interne Methoden anlegen.

Nummern starten pro Ebene bei 1, bleiben stabil und werden nicht neu sortiert. Neue Einträge werden hinten angefügt.

## 2. Story-Format

````markdown
# {Story-Titel}

## Meta

- **State:** Modified | Implemented
- **Quelle:** {Idee, Ticket oder Auftrag}

## User Story

Als {Rolle} möchte ich {Ziel}, damit {Nutzen}.

## Beschreibung

{Fachliches Verhalten, Erreichbarkeit und wichtigste Interaktionen.
Keine unnötigen Implementierungsdetails.}

## Akzeptanzkriterien

### Fachliches Verhalten

- ...

### Berechtigungen und Sicherheit

- ...

### Validierung und Fehlerfälle

- ...

### UI/UX

- ...

### Daten und Integrationen

- ...

### Betrieb und Nachvollziehbarkeit

- ...
````

Nicht relevante Unterabschnitte dürfen entfallen. Sicherheits-, Berechtigungs- oder Integrationsabschnitte dürfen nicht entfallen, wenn die Story geschützte Funktionen, externe Systeme oder sensible Daten betrifft.

## 3. State

| State | Bedeutung |
|---|---|
| `Modified` | Die Story ist neu oder wurde inhaltlich geändert. Der Code entspricht ihr nicht nachweislich vollständig. |
| `Implemented` | UI, Backend, Datenhaltung, Sicherheit, Integrationen und Tests entsprechen allen relevanten Akzeptanzkriterien. |

### Pflege-Regeln

- Jede inhaltliche Änderung einer `Implemented`-Story setzt sie auf `Modified`.
- Reine Rechtschreib- oder Formatkorrekturen ohne Bedeutungsänderung ändern den State nicht.
- Reine Codeänderungen ohne Spec-Änderung ändern den State nicht automatisch.
- Werden Spec und Code gemeinsam vollständig geändert und geprüft, darf direkt `Implemented` gesetzt werden.
- Bei Unsicherheit gilt konservativ `Modified`.
- Der State wird bei jeder Spec-Bearbeitung ausdrücklich geprüft.

## 4. Gute Akzeptanzkriterien

Jedes Kriterium muss:

- eine einzelne überprüfbare Aussage enthalten,
- fachliches Verhalten statt unnötiger Implementierungsdetails beschreiben,
- einen eindeutigen Pass-/Fail-Zustand besitzen,
- Rolle, Auslöser oder Vorbedingung erkennen lassen, sofern relevant,
- Positiv-, Negativ- und Grenzfälle angemessen abdecken.

### Zulässige Beispiele

```markdown
- Das Feld `ExternalOrderNumber` ist ein Pflichtfeld.
- Das Feld `ExternalOrderNumber` akzeptiert höchstens 100 Zeichen.
- Das Backend lehnt eine Freigabe ab, wenn der Auftrag nicht den Status `PendingReview` besitzt.
- Benutzer ohne die Policy `Orders.Approve` können die Aktion „Freigeben“ nicht ausführen.
- Ein direkter serverseitiger Freigabeaufruf ohne `Orders.Approve` wird abgewiesen.
- Eine wiederholte Verarbeitung derselben externen Bestell-ID erzeugt keinen zweiten Auftrag.
- Bei einem temporären Schnittstellenfehler bleibt der Vorgang nachvollziehbar wiederholbar.
- Das UI zeigt die vom Backend gelieferte fachliche Fehlermeldung am betroffenen Vorgang an.
```

### Nicht zulässige Beispiele

```markdown
- Alle Eingaben werden korrekt validiert.
- Die Seite funktioniert wie erwartet.
- Es wird ein schöner Dialog implementiert.
- Die Daten werden sicher verarbeitet.
- Die Schnittstelle funktioniert zuverlässig.
```

## 5. Verbindliche Abdeckung

Für jede Story wird geprüft, ob Kriterien für folgende Bereiche erforderlich sind.

### Fachlichkeit

- Normalfall,
- leere Zustände,
- Grenzwerte,
- konkurrierende Änderungen,
- Abbruch und Wiederholung,
- Statusübergänge,
- fachliche Fehlermeldungen,
- menschliche Freigaben.

### Datenhaltung

- Datenverantwortung und Quellsystem,
- Pflichtfelder, Längen und Wertebereiche,
- Eindeutigkeit und Beziehungen,
- Audit-Felder,
- Soft Delete und Wiederherstellung,
- Migration bestehender Daten,
- Transaktionsgrenzen,
- Aufbewahrung und Löschung.

### Sicherheit und Datenschutz

- Authentifizierung,
- serverseitige Autorisierung,
- ergänzende Sichtbarkeit im UI,
- minimal erforderliche Policies und Scopes,
- objektbezogene Berechtigungen,
- erlaubte Datenarten,
- Ausschluss sensibler Inhalte aus UI, Fehlern und Logs,
- relevante Missbrauchs- und Negativfälle.

### UI/UX und Barrierefreiheit

- Lade-, Leer-, Fehler-, Berechtigungs- und Erfolgszustände,
- Tastaturbedienung und Fokus,
- verständliche Bezeichnungen,
- responsive Darstellung,
- relevante Themes,
- reduzierte Bewegung,
- Regeln aus `uiux.md`.

### Integrationen

- Quell- und Zielsystem,
- Datenvertrag und Mapping,
- Authentifizierung und Scopes,
- Idempotenz,
- Timeouts und Wiederholung,
- doppelte oder verspätete Ereignisse,
- Fehler- und manueller Klärungszustand,
- Rückwärtskompatibilität,
- Reconciliation, sofern erforderlich.

### Betrieb

- Konfigurationsänderungen,
- Logging und Auditierung,
- Monitoring und Benachrichtigungen,
- Migration,
- Rollback und Wiederherstellung,
- Support- und Betriebsverantwortung.

## 6. Architekturgrenze UI und Backend

Fachregeln, Eindeutigkeit, persistenzrelevante Validierung, Statusübergänge und geschützte Aktionen werden als Backend-Verhalten spezifiziert.

UI-Kriterien beschreiben zusätzlich:

- Sichtbarkeit und Bedienbarkeit,
- Benutzerfeedback,
- Bestätigungen,
- Fokus und Barrierefreiheit.

Clientseitige Validierung und ausgeblendete Aktionen ersetzen keine serverseitige Prüfung.

Richtig:

```markdown
- Das Backend prüft, ob der Vorgang freigegeben werden darf.
- Das Backend weist einen Aufruf ohne `Orders.Approve` ab.
- Das UI zeigt „Freigeben“ nur Benutzern mit `Orders.Approve` an.
- Bei einer fachlichen Ablehnung zeigt das UI die Backend-Meldung am Vorgang an.
```

Falsch:

```markdown
- Das Formular stellt die Freigabeberechtigung sicher.
```

## 7. Daten- und Integrationsgrenze

Eine Spec darf nicht voraussetzen, dass ein System bestimmte Daten besitzt oder schreiben darf, wenn dies nicht geprüft wurde.

In `_Datenstruktur.md` beziehungsweise `_Schnittstellen.md` wird festgehalten:

- fachliche Bezeichnung,
- verantwortliches System,
- interne und externe IDs,
- Leser und Schreiber,
- Aktualisierungsrichtung,
- Konfliktregel,
- Fehlerverhalten,
- Sicherheits- und Datenschutzanforderungen.

Technologieentscheidungen werden nur dann Teil einer Story, wenn sie selbst eine freigegebene Anforderung oder Architekturentscheidung darstellen.

## 8. Begriffe und Schreibweise

- Feld-, Event-, Policy-, Status- und Enum-Namen stehen in Backticks.
- Namen entsprechen exakt der freigegebenen Datenstruktur oder dem vorhandenen Code.
- Policies werden mit ihrem tatsächlichen Namen genannt. Keine Rechte oder Rollen erfinden.
- UI-Texte werden in Anführungszeichen geschrieben und sind grundsätzlich deutsch, sofern die Produktentscheidung nichts anderes vorgibt.
- Keine neuen technischen Namen erfinden, solange die fachliche Entscheidung offen ist.
- Dieselbe Aktion wird in allen Stories gleich bezeichnet.
- Begriffe externer Systeme werden nur verwendet, wenn sie verifiziert sind.

## 9. Definition of Done einer Story

Eine Story darf nur `Implemented` sein, wenn:

- [ ] alle Akzeptanzkriterien umgesetzt und einzeln geprüft sind,
- [ ] Backend-Validierung und serverseitige Autorisierung vorhanden sind,
- [ ] ergänzende UI-Berechtigungs- und Feedbackzustände umgesetzt sind,
- [ ] relevante Geschäftslogik mit geeigneten Tests abgedeckt ist,
- [ ] Positiv-, Negativ-, Grenz-, Berechtigungs- und Regressionstests erfolgreich sind,
- [ ] Integrationsfälle einschließlich Idempotenz und Fehlerbehandlung getestet sind, sofern relevant,
- [ ] Build und verpflichtende Prüfungen erfolgreich sind,
- [ ] keine Secrets oder produktiven Daten enthalten sind,
- [ ] Logging keine sensiblen Inhalte offenlegt,
- [ ] Datenbank-, Konfigurations-, Berechtigungs- und Integrationsänderungen dokumentiert sind,
- [ ] Migration, Rollback und Wiederherstellung geklärt sind, sofern relevant,
- [ ] UI in relevanten Viewports und vorhandenen Themes geprüft ist,
- [ ] Dokumentation, ADRs, Specs und Code übereinstimmen,
- [ ] ein unabhängiges menschliches Review erfolgen kann beziehungsweise erfolgt ist.

Der Coding Agent setzt den State nicht allein aufgrund einer plausibel wirkenden Implementierung auf `Implemented`. Der Nachweis erfolgt anhand der Kriterien und ausgeführten Prüfungen.
