# Sprache des Schriftverkehrs

## Meta

- **State:** Implemented
- **Quelle:** Auftrag zur Mehrsprachigkeit. Eine arabischsprachige Bürgerin soll keine deutsche Bestätigung erhalten.

## User Story

Als Bürgerin oder Bürger möchte ich Bestätigung und Absage in der Sprache erhalten, in der ich gebucht habe, damit ich Datum, Uhrzeit und Absagelink zweifelsfrei verstehe.

## Beschreibung

Die Sprache, in der eine Buchung entsteht, wird je Termin gespeichert. Jedes Schreiben
zu diesem Termin geht in dieser Sprache hinaus, unabhängig davon, in welcher Sprache es
ausgelöst wird. Sagt ein Mitarbeiter aus einer deutschen Oberfläche einen auf Arabisch
gebuchten Termin ab, erhält der Bürger die Absage auf Arabisch.

## Akzeptanzkriterien

### Fachliches Verhalten

- Jeder Termin trägt die Sprache der Buchung im Feld `Language` als Kulturname, zum Beispiel `ar-DZ`.
- Alle Termine einer Buchung tragen dieselbe Sprache.
- Die Terminbestätigung wird in der Sprache der Buchung erzeugt.
- Die Teilabsage und die vollständige Absage werden in der Sprache der Buchung erzeugt, auch wenn ein Mitarbeiter sie in einer anderen Sprache auslöst.
- Servicebezeichnungen im Schreiben folgen der Sprache der Buchung und fallen ohne gepflegte Übersetzung auf Deutsch zurück.
- Datum und Uhrzeit werden in der Sprache der Buchung formatiert.
- Eine unbekannte oder fehlende Sprachangabe ergibt die Standardsprache `de-DE`.
- Schreiben an Mitarbeiter — Willkommen, Passwort zurücksetzen, Passwort geändert — gehen in der Sprache der auslösenden Sitzung hinaus; der beim Start erzeugte erste Administrator erhält die Standardsprache.

### Berechtigungen und Sicherheit

- Die Sprache ist eine Darstellungsangabe und trägt weder Identität noch Berechtigungen.
- Namen und Servicebezeichnungen werden im HTML des Schreibens kodiert, damit eine Eingabe den Aufbau des Schreibens nicht verändern kann.
- Ein fehlgeschlagener Versand macht die Buchung beziehungsweise die Absage nicht rückgängig; der Fehler wird protokolliert.

### Validierung und Fehlerfälle

- `Language` ist ein Pflichtfeld mit höchstens 10 Zeichen.
- Fehlt zu einem Schlüssel ein Text, erscheint der Schlüssel; das Schreiben bleibt zustellbar.

### UI/UX

- Ein arabisches Schreiben trägt `dir="rtl"` und `lang="ar-DZ"` sowie eine Schriftfamilie mit arabischen Schnitten.
- Betreff und Inhalt werden als UTF-8 kodiert, damit Umlaute und arabische Zeichen unversehrt ankommen.

### Daten und Integrationen

- Die Spalte `Language` entsteht über die Migration `MehrsprachigkeitServiceUndBuchung`.
- Bestehende Termine erhalten den Vorgabewert `de-DE`, die bisher einzige Sprache des Schriftverkehrs.
- Der Versand läuft unverändert über `SmtpEmailService`; es kommt keine weitere Anbindung hinzu.
