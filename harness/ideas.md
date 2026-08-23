# Ideen-Entwicklung

Dieses Dokument regelt, wie aus einer groben Idee eine fachlich und technisch belastbare Entscheidungsgrundlage wird, bevor Specs oder Code entstehen.

## 1. Rollen

| Rolle | Aufgabe |
|---|---|
| **Stakeholder** | beschreibt Problem und Ziel, priorisiert den Nutzen und trifft fachliche Entscheidungen |
| **Product Owner / Coding Agent** | hinterfragt die Idee, prüft den Bestand, zeigt Alternativen und Risiken auf und dokumentiert Entscheidungen |
| **Entwickler / Architekt** | unterstützt bei Architektur-, Sicherheits-, Integrations- und Machbarkeitsfragen |
| **Fachsystem- oder Betriebsverantwortlicher** | bestätigt reale Möglichkeiten, Datenhoheit, Zugänge und Betriebsbedingungen externer Systeme |

## 2. Wann gilt dieser Prozess?

Der Prozess gilt für:

- neue Funktionen,
- größere Verhaltensänderungen,
- neue Schnittstellen oder Datenflüsse,
- Änderungen an Architektur, Berechtigungen oder Datenhaltung,
- neue Hintergrund- oder Automatisierungsprozesse,
- fachlich noch nicht spezifizierte Anforderungen.

Kleine Bugfixes, Refactorings ohne Verhaltensänderung und reine Wartungsarbeiten benötigen keine neue Idee. Sie benötigen dennoch einen nachvollziehbaren Auftrag und geeignete Tests.

## 3. Grundregeln des Dialogs

- Zuerst Problem und Nutzen klären, danach Lösung und Technik.
- Pro Runde höchstens drei konkrete Fragen stellen.
- Annahmen, Widersprüche und Randfälle ausdrücklich benennen.
- Bei offenen Entscheidungen zwei bis drei Optionen mit Vor- und Nachteilen anbieten.
- Eine begründete Empfehlung aussprechen, aber die Stakeholder-Entscheidung nicht vorwegnehmen.
- Keine Fachlichkeit, Datenfelder, Rollen, Policies, Statuswerte, Datenquellen oder Schnittstellen erfinden.
- Reale Möglichkeiten externer Systeme werden geprüft, nicht vermutet.
- Entscheidungen und verworfene Alternativen direkt im Ideen-Dokument festhalten.
- Keine Specs oder Implementierung beginnen, solange wesentliche Punkte offen sind.

## 4. Phasen

### 4.1 Problem verstehen

Mindestens klären:

- Wer hat welches Problem?
- Wie wird heute damit umgegangen?
- Welche manuellen Schritte, Medienbrüche oder Fehler treten auf?
- Welcher messbare oder beobachtbare Nutzen wird erwartet?
- Welche Folgen hat es, wenn die Idee nicht umgesetzt wird?

### 4.2 Scope abgrenzen

Festlegen:

- In Scope,
- ausdrücklich Out of Scope,
- betroffene Benutzerrollen und Policies,
- relevante Rand-, Fehler- und Konkurrenzfälle,
- menschliche Freigaben,
- Abhängigkeiten zu anderen Anwendungen, Daten oder Prozessen.

### 4.3 Bestand prüfen

Der Agent prüft aktiv:

- bestehende Ideen, Specs und ADRs,
- vorhandene Module und Projektgrenzen,
- vorhandene Entities, Mappings und Migrationen,
- Datenverantwortung und tatsächliche Quellsysteme,
- Geschäftslogik und UI-Komponenten,
- Authentifizierung und Autorisierung,
- bestehende interne und externe Schnittstellen,
- Hintergrundprozesse und geplante Jobs,
- Konfiguration, Logging, Monitoring und Tests,
- UI-/UX- und Barrierefreiheitsvorgaben.

Ergebnisse werden mit konkreten Pfaden und verifizierten Systeminformationen dokumentiert.

### 4.4 Lösungsoptionen bewerten

Die Lösung wird nicht vorschnell auf eine bestimmte Bibliothek, Plattform oder Integrationsart festgelegt.

Mindestens prüfen:

- bestehende Funktion im Fachsystem,
- Erweiterung der Backend-Geschäftslogik,
- eigener Worker oder Hintergrunddienst,
- direkte API- oder Webhook-Anbindung,
- andere begründet geeignete technische Lösung.

Bewertungskriterien:

- fachliche Korrektheit,
- Sicherheit und Datenschutz,
- Zuverlässigkeit und Idempotenz,
- Wartbarkeit und Testbarkeit,
- Betriebsaufwand und Monitoring,
- Kosten und Anbieterabhängigkeit,
- Migration und Austauschbarkeit.

### 4.5 Risiken bewerten

Mindestens betrachten:

- fachliche Fehlbedienung und Dateninkonsistenz,
- Schutzbedarf, personenbezogene und finanzielle Daten,
- neue oder geänderte Berechtigungen,
- Datenmigration und Rückwärtskompatibilität,
- Performance und Parallelzugriffe,
- doppelte oder verspätete Integrationsereignisse,
- Betrieb, Monitoring, Rollback und Wiederherstellung,
- Auswirkungen auf nichtproduktive und produktive Umgebungen.

Für sicherheitskritische oder wesentliche Architekturänderungen wird geklärt, ob Threat Modeling oder ein ADR erforderlich ist.

### 4.6 Idee abschließen

Der Product Owner fasst Problem, Nutzen, Scope, fachliche Lösung, Auswirkungen, Optionen, Entscheidungen und Risiken zusammen. Der Stakeholder bestätigt die Fassung ausdrücklich.

Erst danach darf die Idee `Ready` werden.

## 5. Definition of Ready

Eine Idee ist `Ready`, wenn:

- [ ] Problem und heutiger Zustand beschrieben sind,
- [ ] Zielgruppe und erwarteter Nutzen benannt sind,
- [ ] In Scope und Out of Scope eindeutig sind,
- [ ] fachliches Verhalten und relevante Fehlerfälle konkret sind,
- [ ] Rollen, Policies und Freigaben geklärt sind,
- [ ] Datenverantwortung und betroffene Systeme geprüft wurden,
- [ ] Auswirkungen auf Specs, Daten, Backend, UI, Integrationen und Betrieb geprüft wurden,
- [ ] Sicherheits-, Datenschutz- und Barrierefreiheitsaspekte bewertet wurden,
- [ ] Idempotenz, Wiederholung und manuelle Klärung betrachtet wurden, sofern Integrationen betroffen sind,
- [ ] Abhängigkeiten, Migration, Rollback und Wiederherstellung betrachtet wurden,
- [ ] Alternativen und wesentliche Entscheidungen dokumentiert sind,
- [ ] keine umsetzungsblockierenden offenen Punkte bestehen,
- [ ] der Stakeholder die finale Fassung bestätigt hat.

## 6. Ablage

Jede Idee liegt unter:

```text
ideas/I{n} {Ideen-Name}.md
```

Der Ordner `ideas/` bleibt leer, bis die erste Idee entsteht. Nummern bleiben stabil. Neue Ideen werden hinten angefügt.

## 7. Ideen-Format

````markdown
# {Ideen-Titel}

## Meta

- **State:** Draft | Ready | Übernommen
- **Stakeholder:** {Name oder Rolle}
- **Letzte Änderung:** {YYYY-MM-DD}

## Problem und Nutzen

{Problem, heutiger Zustand, Zielgruppe und erwarteter Nutzen}

## Lösungsidee

{Fachliche Lösung ohne vorgezogene Implementierungsdetails}

## Scope

### In Scope

- ...

### Out of Scope

- ...

## Rollen, Policies und Freigaben

- ...

## Erwartetes Verhalten und Randfälle

- ...

## Auswirkungen auf den Bestand

- **Specs:** ...
- **Datenhaltung und Datenverantwortung:** ...
- **Backend/Geschäftslogik:** ...
- **UI/UX:** ...
- **Schnittstellen und Hintergrundprozesse:** ...
- **Konfiguration und Betrieb:** ...
- **Tests:** ...

## Sicherheit und Datenschutz

- **Schutzbedarf/Datenarten:** ...
- **Missbrauchsszenarien:** ...
- **Erforderliche Maßnahmen:** ...

## Lösungsoptionen und Entscheidungen

- {YYYY-MM-DD}: {Optionen, Entscheidung, Begründung und verworfene Alternativen}

## Risiken

- ...

## Offene Punkte

- ...

## Übernommene Specs

- {wird beim Übergang auf `Übernommen` ergänzt}
````

## 8. State-Werte

| State | Bedeutung |
|---|---|
| `Draft` | Die Idee wird diskutiert oder enthält offene Punkte. |
| `Ready` | Die Definition of Ready ist erfüllt und die Fassung wurde bestätigt. |
| `Übernommen` | Die beauftragte Idee wurde in Specs überführt und die Spec-Pfade sind verlinkt. |

Ein Dokument im State `Übernommen` wird fachlich nicht weiterbearbeitet. Neue Erkenntnisse werden in Specs oder einer neuen Idee dokumentiert.

## 9. Übergang zu Specs

Eine Idee wird nur in Specs überführt, wenn:

1. sie `Ready` ist und
2. der Stakeholder die Umsetzung beauftragt hat.

Danach werden Specs nach `requirements.md` angelegt, im Ideen-Dokument verlinkt und der State auf `Übernommen` gesetzt.
