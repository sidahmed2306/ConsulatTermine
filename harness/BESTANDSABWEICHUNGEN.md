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
| 1 | | | | | offen | |

## Pflege

- Neue Abweichungen werden nur ergänzt, wenn sie im Bestand gefunden werden, nicht wenn neuer Code sie erzeugt.
- Neuer Code, der gegen das Regelwerk verstößt, gehört nicht in diese Liste, sondern wird korrigiert.
- Behobene Abweichungen werden aus der Tabelle entfernt und im zugehörigen Commit genannt.
- Die Liste wird bei `/harness check` gegen den tatsächlichen Stand geprüft.
