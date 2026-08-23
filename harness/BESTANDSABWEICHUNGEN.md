# Bestandsabweichungen

Diese Datei dokumentiert bekannte Abweichungen des vorhandenen Codes vom Regelwerk unter `harness/`. Sie wurde bei der Einführung des Regelwerks mit `/harness adopt` erstellt.

Stand: 2026-08-23

## Bedeutung

Eine hier geführte Abweichung ist **bewusst bekannt und geduldet**, nicht übersehen. Sie ist damit:

- kein Grund, das Regelwerk für neuen Code zu lockern,
- kein Auftrag, sie beim nächsten Kontakt sofort zu beheben,
- kein Freibrief, sie in neuem Code fortzusetzen.

## Status-Werte

| Status | Bedeutung |
|---|---|
| `akzeptiert` | Bleibt dauerhaft so. Neuer Code folgt trotzdem dem Regelwerk. |
| `bei Berührung` | Wird behoben, sobald die betroffene Stelle fachlich ohnehin geändert wird. |
| `geplant` | Eigener beauftragter Umbau, mit Ticket oder Spec verknüpft. |
| `offen` | Noch nicht entschieden, wie damit umgegangen wird. |

## Abweichungen

| # | Abweichung | Regel | Ort | Umfang | Status | Entscheidung |
|---|---|---|---|---|---|---|
| 1 | Umfangreiche `@code`-Blöcke steuern Ablauf, Zustand und Filterung direkt in der Seite | `design.md` §4, `code.md` §8 | `AdminWorkingHours.razor` 647 Zeilen, `Appointmentslot.razor` 399, `AppointmentFormPage.razor` 216, `EmployeeDashboard.razor` 198, fünf weitere über 100 | 8 Seiten | `bei Berührung` | Persistenz und Autorisierung liegen bereits vollständig in der Anwendungsschicht. Was bleibt, ist Ablauf- und Anzeigelogik. Ein Umbau in Teilkomponenten lohnt sich beim nächsten fachlichen Eingriff in die jeweilige Seite, nicht als Selbstzweck. |
| 2 | Listenabfragen ohne Pagination | `design.md` §6 | 38 Vorkommen von `ToListAsync` in `Infrastructure/Services`, kein einziges `Skip`/`Take` | Infrastructure-Schicht | `akzeptiert` | Die Mengen sind durch die Fachlichkeit begrenzt: ein Konsulat mit einer zweistelligen Zahl Mitarbeiter, wenigen Services und Terminen je Tag. Indizes für die tatsächlichen Abfragen sind gesetzt. Wird neu bewertet, sobald eine Ansicht über die Tagesmenge hinaus lädt. |
| 3 | Kein Testprojekt für die UI-Schicht | `code.md` §13 | `ConsulatTermine.UI` | 1 Projekt | `geplant` | Betrifft insbesondere `ClaimsEmployeeAuthorization.FromPrincipal`: die Zuordnung von Claims auf `CurrentEmployee` ist sicherheitsrelevant und derzeit nicht getestet. Die Methode ist bewusst `internal static` und ohne Abhängigkeiten geschrieben, damit ein Test sie direkt aufrufen kann. |
| 4 | Buchungs-Wizard hält seinen Zustand im `sessionStorage` des Browsers | `design.md` §4 | `Services.razor`, `Appointmentslot.razor`, `AppointmentFormPage.razor`, `AppointmentConfirmation.razor` | 4 Seiten | `akzeptiert` | Ausdrücklich kein Sicherheitsmechanismus: die Werte werden serverseitig erneut validiert, bevor eine Buchung entsteht. Der Speicher trägt nur den Bedienfortschritt über Seitenwechsel und Neuladen. Die Schlüssel sind in `SessionKeys` benannt und dort kommentiert. |
| 5 | Commit-Historie folgt nicht Conventional Commits | `commit-messages.md` | 20 der letzten 20 Commits vor der Einführung heißen „zwichenstand“ o. ä. | Historie | `akzeptiert` | Die Historie wird nicht umgeschrieben. Ab der Einführung des Regelwerks gilt die Konvention. |
| 6 | Termine werden als `DateTime` ohne Zeitzone gespeichert; `CreateBookingRequestDto.TimeZone` wird nie ausgewertet | `design.md` §5 | `Appointment.Date`, `CreateBookingRequestDto` | Domain und Buchung | `offen` | Verhalten bei Sommerzeitwechsel ist ungeklärt, siehe `OPEN_DECISIONS.md` Nummer 17. Vor einer Klärung wird an der Zeitbehandlung nichts geändert. |
| 7 | Keine Lösch- oder Anonymisierungslogik für personenbezogene Daten | `security.md` §9 | `Appointments` (Name, E-Mail, Telefon, Geburtsdatum) | Persistenz | `offen` | Aufbewahrungsfristen sind fachlich und rechtlich nicht geklärt, siehe `OPEN_DECISIONS.md` Nummer 14. |
| 8 | Keine Specs unter `specs/` | `requirements.md` | gesamtes Repository | — | `akzeptiert` | Der Bestand entstand ohne Anforderungsdokumente. Sie werden nicht rückwirkend erzeugt; ab der nächsten fachlichen Änderung entsteht je Anforderung eine Spec. |
| 9 | Secrets in der Git-Historie | `security.md` §8 | Historie bis einschließlich `5d03e4e` | 2 Zugangsdaten | `offen` | Aus dem Arbeitsbaum entfernt. Beide Zugangsdaten sind zu widerrufen und zu rotieren; das Bereinigen der Historie ist zu entscheiden, siehe `OPEN_DECISIONS.md` Nummer 15. |
| 10 | Keine CI-Pipeline | `code.md` §15 | Repository | — | `offen` | Build, Tests und Formatprüfung laufen nur lokal, siehe `OPEN_DECISIONS.md` Nummer 16. |

## Behoben bei der Einführung des Regelwerks

Diese Punkte der ursprünglichen Inventur bestehen nicht mehr und sind hier nur als Nachweis geführt:

| Ursprünglicher Befund | Behoben durch |
|---|---|
| Klartext-Passwörter, `!=`-Vergleich | PBKDF2 über `IPasswordHasher<Employee>` |
| `/employee/change-password/{id}` ohne Anmeldung erreichbar | Route ohne Id, Mitarbeiter aus der Anmeldung |
| Autorisierung nur in `@code`-Blöcken | Cookie-Auth, Policies, serverseitige Prüfung in jedem verwaltenden Anwendungsfall |
| Reset-Token im Klartext in der Datenbank | 256 Bit Zufall, gespeichert als SHA-256-Hash |
| Anmeldung verriet die Existenz einer Kennung | einheitliche Meldung samt Dummy-Hash-Vergleich |
| Keine Begrenzung von Fehlversuchen | Sperre nach konfigurierbar vielen Versuchen |
| Secrets in `appsettings.json` | User Secrets, `appsettings.Example.json` als Vorlage |
| `EmployeeProtectedComponentBase` ungenutzt | entfernt, ersetzt durch `[Authorize]` |
| `Console.WriteLine`, verschluckte Exceptions | `ILogger` mit quellcodegenerierten Meldungen |
| Fehlende `.editorconfig`, kein `EnforceCodeStyleInBuild` | beides vorhanden, Build läuft mit `-warnaserror` |
| `bin/` und `obj/` versioniert | 314 Dateien aus dem Index entfernt |
| Kein Testprojekt | zwei Testprojekte, 82 Tests |
