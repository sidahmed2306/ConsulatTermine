# ADR 0002 – Bestehende Vier-Schichten-Struktur beibehalten

- **Status:** angenommen
- **Datum:** 2026-08-23
- **Betrifft:** `harness/profile.md` Abschnitt 3–5, `harness/design.md` Abschnitt 3

## Problem

Das Stack-Profil des Harness beschreibt eine Zwei-Projekt-Aufteilung
(`<Projekt>.Web` und `<Projekt>.Backend`). Das Bestandsprojekt ist in vier
Projekte geschnitten: Domain, Application, Infrastructure, UI.

## Entscheidung

Die vorhandene Struktur bleibt. `harness/profile.md` wurde bei der Einführung des
Regelwerks auf die IST-Struktur angepasst und hält die Zuordnung fest:

| Profil | Dieses Projekt |
|---|---|
| `<Projekt>.Web` | `ConsulatTermine.UI` |
| `<Projekt>.Backend` (Logic + Data) | `ConsulatTermine.Infrastructure` |
| `<Projekt>.Backend` (Interfaces, UiModels) | `ConsulatTermine.Application` |
| Entities | `ConsulatTermine.Domain` |

## Begründung

- `harness/design.md` Abschnitt 3: freigegebene Modul- und Projektnamen werden
  nicht ohne Auftrag umbenannt.
- Die vier Projekte erfüllen denselben Zweck wie die zwei des Profils, nur feiner
  geschnitten. Die Abhängigkeiten zeigen bereits sauber nach innen.
- Ein Umbau berührte jede Datei des Repositorys, ohne eine einzige fachliche oder
  technische Eigenschaft zu verbessern.

Das Profil verlangt für Anwendungsfallklassen den Suffix `Manager`. Der Bestand
verwendet durchgängig `Service`. Auch das bleibt: der Suffix ist projektweit
konsistent, und eine Umbenennung von 16 Klassen samt Interfaces und
Registrierungen trägt nichts bei.

## Folgen

- Geschäftslogik liegt in `ConsulatTermine.Infrastructure/Services`, nicht in
  einem eigenen Logic-Projekt. Das entspricht dem Profil, das `Logic` und `Data`
  ebenfalls in einem Projekt zusammenfasst.
- Je Backend-Projekt existiert ein Testprojekt: `ConsulatTermine.Application.Test`
  und `ConsulatTermine.Infrastructure.Test`.
