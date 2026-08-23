# Regelwerk

Dieses Verzeichnis enthält die verbindlichen Entwicklungsregeln für ConsulatTermine. Es wurde mit dem `/harness`-Skill aus dem stack-neutralen Core und dem Stack-Profil `dotnet-blazor` erzeugt.

| Datei | Inhalt |
|---|---|
| `design.md` | Architektur, Schichten, Datenhaltung, Konfiguration, Logging, Betrieb |
| `profile.md` | stack-spezifische Konkretisierung: Struktur, Benennung, Stil, Befehle |
| `code.md` | Code-, Test-, Git- und Qualitätsregeln |
| `uiux.md` | UI-, UX-, Komponenten- und Barrierefreiheitsregeln |
| `security.md` | Authentifizierung, Autorisierung, Secrets, Datenschutz |
| `integrations.md` | externe Systeme, Schnittstellen, Synchronisation, Hintergrundprozesse |
| `requirements.md` | Epics, Features, Stories und Akzeptanzkriterien |
| `ideas.md` | strukturierte Klärung neuer Ideen vor Specs und Code |
| `commit-messages.md` | Conventional Commits |

## Lesereihenfolge

Der Einstieg ist `../CLAUDE.md`. Von dort wird situativ nachgeladen. Regeldateien werden nicht auf Vorrat gelesen.

## Rangfolge

`design.md` legt die verbindliche Rangfolge bei Widersprüchen fest. Kurzform: Sicherheitsrichtlinien vor ADRs vor `design.md` vor `profile.md` vor `PROJECT_CONTEXT.md` vor lokalen Patterns vor Framework-Konventionen.

## Änderungen

Regeländerungen sind eigene, begründete Commits. Projektspezifische Fakten gehören nach `../PROJECT_CONTEXT.md`, nicht in diese Dateien.

Aktualisierung des Regelwerks aus dem Skill: `/harness update`.
