# Sprachwahl und Schreibrichtung

## Meta

- **State:** Implemented
- **Quelle:** Auftrag: die Anwendung soll Deutsch, Englisch und Arabisch anbieten, je nach Auswahl.

## User Story

Als Bürgerin oder Bürger möchte ich die Anwendung in Deutsch, Englisch oder Arabisch bedienen können, damit ich einen Termin auch dann sicher buche, wenn ich kein Deutsch lese.

## Beschreibung

Die Anwendung wird in drei Sprachen ausgeliefert: `de-DE`, `en-US` und `ar-DZ`. Die
Sprache gilt für die gesamte Oberfläche, den öffentlichen Bürgerbereich ebenso wie den
internen Mitarbeiterbereich.

Die Sprachwahl steht im Kopfbereich jeder Seite und zusätzlich auf der Anmeldemaske,
die außerhalb des Blazor-Kreises liegt. Die Wahl wird in einem Cookie abgelegt und gilt
für spätere Besuche weiter.

Arabisch wird von rechts nach links dargestellt. Das betrifft nicht nur den Text,
sondern die Anordnung der gesamten Oberfläche einschließlich der Navigationsleiste des
Mitarbeiterbereichs.

## Akzeptanzkriterien

### Fachliches Verhalten

- Die Anwendung liefert genau die Sprachen `de-DE`, `en-US` und `ar-DZ` aus.
- `de-DE` ist die Standardsprache und zugleich die Rückfallebene für jeden nicht übersetzten Text.
- Das Sprachmenü zeigt jede Sprache in ihrer eigenen Schreibweise: „Deutsch", „English", „العربية".
- Die Auswahl einer Sprache lädt die aktuelle Seite neu und behält dabei die Adresse einschließlich ihrer Abfrageparameter bei.
- Die gewählte Sprache wird in einem Cookie mit einem Jahr Gültigkeit abgelegt und gilt bei einem späteren Besuch weiter.
- Hat ein Besucher noch keine Sprache gewählt, wird der Header `Accept-Language` ausgewertet.
- Eine Browserangabe ohne Land wird der ausgelieferten Kultur zugeordnet: `ar` und `ar-MA` ergeben `ar-DZ`, `en` und `en-GB` ergeben `en-US`, `de-AT` ergibt `de-DE`.
- Meldet der Browser ausschließlich nicht ausgelieferte Sprachen, gilt `de-DE`.
- Datum, Uhrzeit, Wochentags- und Monatsnamen werden in der gewählten Sprache formatiert.
- Buchungsnummer, Mitarbeiterkennung, E-Mail-Adresse und Telefonnummer bleiben auch im arabischen Satz von links nach rechts lesbar.

### Berechtigungen und Sicherheit

- Der Endpunkt der Sprachumschaltung ist anonym erreichbar und ändert ausschließlich eine Darstellungsvorliebe.
- Der Endpunkt akzeptiert als Ruecksprungadresse nur einen anwendungsinternen Pfad; eine absolute, protokollrelative oder mit zurückgesetztem Schrägstrich beginnende Adresse wird auf `/` zurückgeführt.
- Eine Ruecksprungadresse mit Steuerzeichen wird verworfen.
- Eine unbekannte Kulturangabe im Aufruf führt zur Standardsprache und nicht zu einem Fehler.
- Das Sprach-Cookie ist `HttpOnly`, `Secure` und `SameSite=Lax` und trägt weder Identität noch Berechtigungen.
- Die Sprachwahl verändert keine Berechtigung: geschützte Seiten und Anwendungsfälle prüfen unverändert serverseitig.

### Validierung und Fehlerfälle

- Fachliche Meldungen der Anwendungsschicht erscheinen in der Sprache der Sitzung, die den Vorgang ausgelöst hat.
- Meldungen der Eingabeprüfung in Formularen erscheinen in der Sprache der Sitzung.
- Technische Fehler erscheinen als allgemeine Meldung in der gewählten Sprache; Einzelheiten stehen ausschließlich im Log.

### UI/UX

- Bei `ar-DZ` trägt das `html`-Element `dir="rtl"`, bei den übrigen Sprachen `dir="ltr"`.
- Bei `ar-DZ` steht die Navigationsleiste des Mitarbeiterbereichs rechts, sonst links.
- Bei `ar-DZ` wird eine Schriftfamilie mit arabischen Schnitten und ein größerer Zeilenabstand verwendet.
- Die aktive Sprache ist im Sprachmenü nicht allein farblich, sondern zusätzlich über ein Haken-Symbol und `aria-current` gekennzeichnet.
- Das Sprachmenü besitzt einen zugänglichen Namen und ist vollständig mit der Tastatur bedienbar.
- Auf schmalen Viewports zeigt die Schaltfläche des Menüs das Sprachkürzel statt des ausgeschriebenen Namens; der vollständige Name bleibt im geöffneten Menü sichtbar.
- Die Sprachwahl ist auch auf der Anmeldemaske erreichbar, die außerhalb der Blazor-Anwendung liegt.
- Die Sprachwahl verliert keine bereits eingegebenen Formulardaten des Buchungsablaufs, weil Personenzahl und Serviceauswahl im Sitzungsspeicher des Browsers liegen.

### Betrieb und Nachvollziehbarkeit

- Es werden keine zusätzlichen Pakete und keine Schriften aus dem Netz geladen.
