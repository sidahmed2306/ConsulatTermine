# ADR 0003 – Klassisches Blazor-Server-Hosting vorerst beibehalten

- **Status:** angenommen
- **Datum:** 2026-08-23
- **Betrifft:** `harness/profile.md` Abschnitt 5

## Problem

Die Anwendung läuft im Hosting-Modell von Blazor Server aus .NET 6:
`_Host.cshtml` als Wirtsseite, `MapBlazorHub()` und
`MapFallbackToPage("/_Host")`. Seit .NET 8 ist das Blazor-Web-App-Modell
(`MapRazorComponents<App>()` mit Render-Modi je Komponente) der empfohlene Weg.

Im Rahmen der Aktualisierung auf .NET 10 stellte sich die Frage, ob das
Hosting-Modell mit umgestellt wird.

## Entscheidung

Das bestehende Modell bleibt vorerst. Die Umstellung wird als eigener,
beauftragter Umbau geführt (`OPEN_DECISIONS.md` Nummer 13).

## Begründung

- Das klassische Modell ist in .NET 10 vollständig unterstützt, nicht veraltet.
- Die Umstellung berührt jede der 22 Razor-Seiten (Render-Modus je Seite),
  `App.razor`, `_Host.cshtml` und `Program.cs`. Sie ändert Vorrender-Verhalten
  und den Zeitpunkt, zu dem JS-Interop verfügbar ist — beides betrifft hier die
  Wartezimmer-Anzeige und die Sprachumschaltung.
- Der eigentliche Anlass für eine Umstellung wäre statisches serverseitiges
  Rendern für die Anmeldemaske gewesen. Dieses Problem ist bereits gelöst:
  Anmeldung und Abmeldung sind Razor Pages (siehe ADR 0001).
- Ein Umbau dieser Größe zusammen mit dem Sicherheitsumbau in einem Schritt wäre
  nicht mehr sinnvoll überprüfbar.

## Folgen

- `App.razor` bleibt der `Router` mit `AuthorizeRouteView`.
- Alle Seiten sind interaktiv; es gibt keine statisch gerenderten Blazor-Seiten.
- Der Umbau bleibt möglich, ohne Policies, `[Authorize]`-Attribute oder die
  serverseitigen Prüfungen anzufassen.
