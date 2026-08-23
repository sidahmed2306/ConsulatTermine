# Offene Entscheidungen

Diese Liste enthält keine verbindlichen Architekturregeln. Sie sammelt Punkte, die vor der jeweils betroffenen Umsetzung geklärt werden müssen.

Ein Punkt wird erst dann geklärt, wenn er tatsächlich gebraucht wird. Ergebnisse wandern nach `PROJECT_CONTEXT.md`, in eine Spec oder in ein ADR und werden hier auf `geklärt` gesetzt.

Stand: 2026-08-23

| # | Thema | Zu klären | Blockiert | Status |
|---|---|---|---|---|
| 1 | Target Framework | — | — | geklärt: `net10.0`, siehe `PROJECT_CONTEXT.md` Abschnitt 2 |
| 2 | Architekturstil | — | — | geklärt: Schichtenarchitektur mit vier Projekten, siehe ADR 0002 |
| 3 | Datenhaltung | — | — | geklärt: SQL Server, Verantwortung je Datenart in `PROJECT_CONTEXT.md` Abschnitt 10 |
| 4 | Authentifizierung extern | — | — | geklärt: kein Konto für Bürger, Identifikation über `BookingReference` und `CancelToken` |
| 5 | Authentifizierung intern | Soll die Mitarbeiteranmeldung mittelfristig an einen Verzeichnisdienst des Konsulats angebunden werden, statt Passwörter selbst zu verwalten? | Einführung eines Identity Providers | offen |
| 6 | Berechtigungsmodell | — | — | geklärt: drei Rollen, drei Policies, siehe `PROJECT_CONTEXT.md` Abschnitt 7 und ADR 0001 |
| 7 | Umgebungen | Gibt es Staging und Production, wo liegen dort die Secrets und wie wird deployed? Bisher ist nur Development eingerichtet. | erstes Deployment | offen |
| 8 | Monitoring | Wohin gehen Logs im Betrieb, welche Alerts gibt es, wer betreut den Support? Derzeit wird nur auf die Konsole protokolliert. | Produktivbetrieb | offen |
| 9 | Externe Systeme | Bleibt SMTP die einzige Anbindung, oder kommen Fachverfahren des Konsulats hinzu? | erste weitere Integration | offen |
| 10 | UI-Bestand | Marken-Tokens, Logo-Nutzung und Barrierefreiheitsanforderungen sind nicht dokumentiert. `AlgerienTheme.cs` definiert Farben ohne belegte Quelle. | erste UI-Story | offen |
| 11 | Statische Analyse | — | — | geklärt: `.editorconfig` plus `Directory.Build.props` mit `EnforceCodeStyleInBuild`, Build läuft mit `-warnaserror` |
| 12 | Erste Specs | — | — | geklärt: die erste Spec entstand mit der Korrektur der Serviceauswahl, siehe `specs/E1 Terminbuchung/F1 Serviceauswahl/S1 Personenzahl und Serviceauswahl.md`. Der Bestand wird nicht rückwirkend dokumentiert. |
| 13 | Blazor-Hosting-Modell | Soll auf das Blazor-Web-App-Modell umgestellt werden? Entscheidung und Begründung der Vertagung in ADR 0003. | größerer UI-Umbau | offen |
| 14 | Aufbewahrung personenbezogener Daten | Wie lange bleiben Name, E-Mail, Telefonnummer und Geburtsdatum in `Appointments` gespeichert, und wie wird gelöscht? Derzeit gibt es keine Löschung. | Produktivbetrieb, Datenschutzfreigabe | offen |
| 15 | Historie der Secrets | Das Repository enthält in der Historie ein Gmail-App-Passwort und ein SQL-`sa`-Passwort. Beide sind zu widerrufen. Soll die Historie zusätzlich bereinigt werden? | Veröffentlichung des Repositorys | offen |
| 16 | CI | Es gibt keine Pipeline. Build, Tests und `dotnet format --verify-no-changes` laufen nur lokal. | verlässliche Prüfung vor Merge | offen |
| 17 | Zeitzonen | Termine werden als `DateTime` ohne Zeitzone gespeichert, `CreateBookingRequestDto.TimeZone` wird nirgends ausgewertet. Was gilt bei Sommerzeitwechsel? | Buchungen über einen Zeitumstellungstermin hinweg | offen |
