# Offene Entscheidungen

Diese Liste enthält keine verbindlichen Architekturregeln. Sie sammelt Punkte, die vor der jeweils betroffenen Umsetzung geklärt werden müssen.

Ein Punkt wird erst dann geklärt, wenn er tatsächlich gebraucht wird. Ergebnisse wandern nach `PROJECT_CONTEXT.md`, in eine Spec oder in ein ADR und werden hier auf `geklärt` gesetzt.

Stand: 2026-08-23

| # | Thema | Zu klären | Blockiert | Status |
|---|---|---|---|---|
| 1 | Target Framework | tatsächlich verwendete Version aus dem Projekt bestätigen | erste Implementierung | offen |
| 2 | Architekturstil | gewählten Stil bestätigen und in PROJECT_CONTEXT.md festschreiben | erste Strukturänderung | offen |
| 3 | Datenhaltung | konkrete Datenbank und Verantwortung je Datenart festlegen | erste Persistenz | offen |
| 4 | Authentifizierung extern | Identity-Provider für Endkunden klären | erster geschützter Kundenbereich | offen |
| 5 | Authentifizierung intern | Unternehmens-Login und benötigte Policies klären | erster interner Bereich | offen |
| 6 | Berechtigungsmodell | Rollen, Policies und objektbezogene Rechte definieren | erste geschützte Aktion | offen |
| 7 | Umgebungen | Development, Staging, Production sowie Secret Store und Deployment klären | erstes Deployment | offen |
| 8 | Monitoring | zentrale Logs, Alerts, Integrationsstatus und Supportprozess festlegen | Produktivbetrieb | offen |
| 9 | Externe Systeme | Versionen, Module, API-Möglichkeiten und Zuständigkeiten verifizieren | erste Integration | offen |
| 10 | UI-Bestand | Komponenten, Marken-Tokens und Bibliotheksnutzung dokumentieren | erste UI-Story | offen |
| 11 | Statische Analyse | `.editorconfig` bestätigen und `EnforceCodeStyleInBuild` in Build und CI verankern | erster Pull Request | offen |
| 12 | Erste Specs | Ideen und Specs mit dem ersten beauftragten fachlichen Prozess anlegen | erste Story | offen |
