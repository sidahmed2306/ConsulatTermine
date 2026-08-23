# ADR 0001 – Cookie-Authentifizierung für den Mitarbeiterbereich

- **Status:** angenommen
- **Datum:** 2026-08-23
- **Betrifft:** `harness/security.md` Abschnitt 1–4, `harness/design.md` Abschnitt 8

## Problem

Der Mitarbeiterbereich war nicht wirksam geschützt.

Die Identität lag im `sessionStorage` des Browsers (`EmployeeId`, `EmployeeCode`,
`EmployeeRole`). Geprüft wurde sie ausschließlich in `@code`-Blöcken einzelner
Razor-Seiten. Der Browser-Speicher ist für den Benutzer frei veränderbar: wer
`EmployeeRole` auf `3` setzt, war Administrator. Die Anwendungsfälle in der
Infrastructure-Schicht prüften überhaupt keine Berechtigung, ein direkter Aufruf
umging die Oberfläche also vollständig.

Zusätzlich war `/employee/change-password/{id}` ohne jede Anmeldung erreichbar.

## Optionen

1. **Cookie-Authentifizierung von ASP.NET Core** mit policy-basierter Autorisierung.
2. **ASP.NET Core Identity** vollständig einführen.
3. **Externer Identity Provider** (Entra ID oder vergleichbar).

## Entscheidung

Option 1.

Die Identität liegt in einem serverseitig signierten Cookie
(`HttpOnly`, `SameSite=Strict`, `Secure`, gleitender Ablauf). Rolle und
Mitarbeiter-Id stehen als Claims darin. Drei Policies bilden die bereits
vorhandenen Rollen ab:

| Policy | Rollen |
|---|---|
| `Mitarbeiter.Zugriff` | Employee, ServiceChef, Admin |
| `Dienstplan.Verwalten` | ServiceChef, Admin |
| `Administration.Verwalten` | Admin |

Jede Seite trägt `[Authorize]` mit einer Policy oder ausdrücklich
`[AllowAnonymous]`. Zusätzlich prüfen die Anwendungsfälle über
`IEmployeeAuthorization` serverseitig erneut. Die Sichtbarkeit in der Oberfläche
steuert nur die Benutzerführung.

Passwörter liegen als PBKDF2-Hash über `IPasswordHasher<Employee>` vor.

## Begründung

Option 2 bringt ein vollständiges Benutzerschema mit, das die bestehende
`Employee`-Tabelle verdoppeln würde, ohne dass Funktionen wie externe Logins oder
Zwei-Faktor-Authentifizierung derzeit gefordert sind. Das widerspricht YAGNI.

Option 3 ist die fachlich wahrscheinlich richtige Zielrichtung für eine
Behörde, setzt aber eine Entscheidung über den Verzeichnisdienst des Konsulats
voraus, die noch offen ist (`OPEN_DECISIONS.md` Nummer 5). Die Umstellung
betrifft dann nur `Pages/Account/Login.cshtml.cs` und die Registrierung in
`Program.cs`; Policies, `[Authorize]`-Attribute und die serverseitigen Prüfungen
bleiben unverändert.

## Folgen

- Anmeldung und Abmeldung sind Razor Pages, keine Blazor-Komponenten: ein Cookie
  lässt sich nur im HTTP-Kontext setzen, nicht aus einem laufenden Circuit. Als
  Formular-Post gilt zugleich der Antiforgery-Schutz.
- Die Anmeldemaske funktioniert ohne JavaScript und ohne Blazor-Circuit.
- Bestehende Klartext-Passwörter gehen mit der Migration verloren. Alle
  Mitarbeiter müssen ihr Passwort über „Passwort vergessen“ neu setzen.
- `EmployeeSessionService`, `EmployeeSessionKeys` und
  `EmployeeProtectedComponentBase` entfallen.
