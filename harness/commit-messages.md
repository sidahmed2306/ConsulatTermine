# Regeln für Commit Messages

Commit Messages folgen der Conventional-Commits-Konvention.

## Aufbau

```text
<typ>(<scope>): <kurze beschreibung>
```

Beispiel:

```text
feat(orders): bestellfreigabe ergänzen
```

## Erlaubte Typen

- `feat`: neue Funktion
- `fix`: Fehlerbehebung
- `docs`: Dokumentation
- `style`: Formatierung ohne Logikänderung
- `refactor`: Umstrukturierung ohne neue Funktion oder Bugfix
- `test`: Tests hinzufügen oder ändern
- `chore`: Wartung, Build, Abhängigkeiten oder Tooling
- `ci`: Änderungen an CI/CD

## Regeln

- Beschreibung kurz und prägnant halten.
- Im Imperativ formulieren, zum Beispiel `füge`, `entferne`, `aktualisiere`.
- Beschreibung mit Kleinbuchstaben beginnen.
- Keinen Punkt am Ende verwenden.
- Pro Commit möglichst eine fachliche Änderung.
- Einen Scope verwenden, wenn er die betroffene Komponente klar macht.
- Keine `Co-Authored-by`-Zeilen verwenden.
- Ticketnummer bei Bedarf im Body oder gemäß Repository-Konvention ergänzen.
- Breaking Changes mit `!` markieren und im Commit-Body erläutern.

```text
feat(api)!: authentifizierung auf oauth2 umstellen
```

## Gute Beispiele

```text
feat(shop): produktvergleich ergänzen
feat(orders): mitarbeiterfreigabe umsetzen
fix(payments): doppelten zahlungsstatus verhindern
docs(architecture): datenverantwortung dokumentieren
refactor(integrations): externes mapping vereinfachen
test(auth): tests für serverseitige policies ergänzen
chore(harness): regelstand aktualisieren
```

## Schlechte Beispiele

```text
update
bugfix
changes
final fix
wip
```
