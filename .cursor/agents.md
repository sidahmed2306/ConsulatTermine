# Agenten-Übersicht – ConsulatTermine

## Orchestrator (Einstieg)

| # | Agent | Datei | Rolle |
|---|-------|-------|-------|
| 0 | **Orchestrator** | `rules/00-orchestrator-agent.mdc` | Projektleitung – analysiert Aufgaben, klärt mit dir, weist Unter-Agenten zu, führt schrittweise aus. **Du sprichst zuerst mit ihm.** |

---

## Fachliche Agenten (aktiv)

| # | Agent | Datei | Zuständig für |
|---|-------|-------|---------------|
| 1 | **Domain Agent** | `rules/01-domain-agent.mdc` | Entities, Enums in `ConsulatTermine.Domain/` |
| 2 | **Booking Agent** | `rules/02-booking-agent.mdc` | Slot-Logik, Buchungsvalidierung, Mehrpersonen/Mehrservice |
| 3 | **Employee & Auth Agent** | `rules/03-employee-auth-agent.mdc` | Login, Passwort, Rollen, Mitarbeiter, Zuweisungen |
| 4 | **Working Schedule Agent** | `rules/04-working-schedule-agent.mdc` | Arbeitszeiten, Pläne, Overrides |
| 5 | **UI/UX Agent** | `rules/05-ui-ux-agent.mdc` | Blazor, MudBlazor, Layout, Seiten |
| 6 | **Localization Agent** | `rules/06-localization-agent.mdc` | .resx, DE/EN/ar-DZ, RTL |
| 7 | **Infrastructure Agent** | `rules/07-infrastructure-agent.mdc` | DbContext, Migrations, Services, E-Mail, SignalR |

## Verwendung

- **Empfohlen:** Mit **@00-orchestrator-agent** oder allgemein starten – er analysiert, plant und koordiniert alle anderen Agenten
- **Direkt:** `@02-booking-agent` etc. – wenn du einen spezifischen Bereich ansprichst
- **Automatisch:** Datei öffnen → passender Agent wird berücksichtigt
- **Projektkontext:** `.cursor/CONTEXT.md`

## Eskalation / Überlappung

| Thema | Primär | Sekundär |
|-------|--------|----------|
| Neue Entity | Domain Agent | Infrastructure Agent (Migration) |
| Neue Buchungs-Regel | Booking Agent | Domain Agent (falls Entity) |
| Neue Seite + Texte | UI/UX Agent | Localization Agent |
| Neue Rolle | Employee & Auth Agent | Domain Agent (EmployeeRole) |
| RTL für Arabisch | Localization Agent | UI/UX Agent (Layout) |
