# Übersetzte Servicebezeichnungen

## Meta

- **State:** Implemented
- **Quelle:** Auftrag zur Mehrsprachigkeit. Die Bezeichnung eines Service ist der wichtigste Text des Buchungsablaufs und stammt aus der Datenbank, nicht aus den Ressourcendateien.

## User Story

Als Administrator möchte ich zu jedem Service eine englische und eine arabische Bezeichnung pflegen, damit Bürgerinnen und Bürger den passenden Service in ihrer Sprache erkennen.

## Beschreibung

Bezeichnung und Beschreibung eines Service werden von der Verwaltung gepflegt und ändern
sich, ohne dass die Anwendung neu ausgeliefert wird. Sie liegen deshalb in der Tabelle
`Services` und nicht in den Ressourcendateien.

Zur deutschen Pflichtangabe treten je eine optionale englische und arabische Fassung.
Fehlt eine Übersetzung, zeigt die Anwendung die deutsche Fassung.

## Akzeptanzkriterien

### Fachliches Verhalten

- Ein Service besitzt die Pflichtfelder `Name` und die optionalen Felder `NameEnglish`, `NameArabic`, `DescriptionEnglish` und `DescriptionArabic`.
- Ist für die gewählte Sprache eine Bezeichnung gepflegt, wird diese angezeigt.
- Ist sie leer oder nicht gepflegt, wird die deutsche Bezeichnung angezeigt.
- Die Regel gilt in der Serviceauswahl, der Terminauswahl, dem Buchungsformular, der Absageseite, dem Wartezimmer, der Arbeitsplatzwahl, den Zuweisungen und der Dienstplanverwaltung.
- Ein Service bleibt ohne gepflegte Übersetzung uneingeschränkt buchbar.

### Berechtigungen und Sicherheit

- Nur Benutzer mit der Policy `Administration.Verwalten` können Services und damit ihre Übersetzungen anlegen, ändern oder löschen.
- Die Prüfung erfolgt serverseitig in `ServiceService` und nicht allein über die Sichtbarkeit der Bedienelemente.

### Validierung und Fehlerfälle

- `NameEnglish` und `NameArabic` akzeptieren höchstens 200 Zeichen.
- `DescriptionEnglish` und `DescriptionArabic` akzeptieren höchstens 2000 Zeichen.
- Die Übersetzungsfelder sind keine Pflichtfelder.

### UI/UX

- Die Serviceverwaltung zeigt je Service, ob die Übersetzungen vollständig sind; fehlende Sprachen werden benannt und nicht nur farblich angedeutet.
- Das Formular führt die Eingabefelder in der Reihenfolge Deutsch, Englisch, Arabisch und weist auf die Rückfallebene hin.
- Das Eingabefeld für die arabische Bezeichnung wird von rechts nach links dargestellt, die englischen Felder von links nach rechts.

### Daten und Integrationen

- Die Spalten entstehen über die Migration `MehrsprachigkeitServiceUndBuchung`.
- Bestehende Datensätze behalten ihre deutsche Bezeichnung; die Übersetzungsspalten bleiben leer.
