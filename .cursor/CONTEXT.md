# ConsulatTermine – Projektkontext

> Dieses Dokument dient als zentrale Referenz für alle Agenten.
> Es beschreibt das Projekt, die Architektur und Konventionen.

---

## 1. Projektbeschreibung

**ConsulatTermine** ist ein Terminbuchungs-System für das **Konsulat Algerien in Frankfurt**.

### Zielgruppen
- **Bürger:** Online-Terminbuchung für Pass, Visa, Standesamt u.a.
- **Mitarbeiter:** Login, Dashboard, Wartezimmer-Aufruf, Service-Auswahl
- **ServiceChef/Admin:** Mitarbeiterverwaltung, Service-Zuweisungen, Arbeitszeiten, Services

### Technologie
- .NET 8, Blazor Server, MudBlazor
- Entity Framework Core, SQL Server
- SignalR (DisplayHub, EmployeeHub)
- E-Mail (SMTP)

---

## 2. Architektur

```
ConsulatTermine.Domain      → Entitäten, Enums (keine Abhängigkeiten)
ConsulatTermine.Application → Interfaces, DTOs, ViewModels, Domain-Logik
ConsulatTermine.Infrastructure → EF Core, Services, E-Mail, SignalR
ConsulatTermine.UI          → Blazor Pages, Shared, Theme
```

### Agenten-Zuordnung
| Agent | Regel-Datei | Verantwortung |
|-------|-------------|---------------|
| Orchestrator | `rules/00-orchestrator-agent.mdc` | Projektleitung – Einstieg, koordiniert Unter-Agenten |
| Domain | `rules/01-domain-agent.mdc` | Entities, Enums |
| Booking | `rules/02-booking-agent.mdc` | Slot-Berechnung, Buchungsvalidierung, Mehrpersonen/Mehrservice |
| Employee & Auth | `rules/03-employee-auth-agent.mdc` | Login, Passwort, Rollen, Mitarbeiter, Zuweisungen |
| Working Schedule | `rules/04-working-schedule-agent.mdc` | Arbeitszeiten, Pläne, Overrides |
| UI/UX | `rules/05-ui-ux-agent.mdc` | Razor-Seiten, MudBlazor, Layout |
| Localization | `rules/06-localization-agent.mdc` | .resx, Mehrsprachigkeit (DE/EN/ar-DZ) |
| Infrastructure | `rules/07-infrastructure-agent.mdc` | DbContext, Migrations, Service-Implementierungen |

---

## 3. Wichtige fachliche Regeln

- **Mehrpersonen-Buchung:** Eine Buchung kann Hauptperson + Begleitpersonen haben
- **Mehrservice-Buchung:** Eine Person kann mehrere Services buchen (z.B. Pass + Visa)
- **BookingReference:** Alle Termine einer Buchung teilen dieselbe Referenz
- **CancelToken:** Absage per E-Mail-Link bis 24h vor Termin
- **Rollen:** Employee, ServiceChef, Admin (steigende Rechte)
- **Slot-Berechnung:** Override (Datum) > Override (Wochentag) > Reguläre Öffnungszeiten

---

## 4. Sprachen
- Deutsch (de-DE) – Standard
- Englisch (en-US)
- Arabisch (ar-DZ) – RTL beachten
