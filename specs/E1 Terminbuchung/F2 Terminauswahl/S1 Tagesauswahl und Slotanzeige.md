# Tagesauswahl und Slotanzeige

## Meta

- **State:** Implemented
- **Quelle:** Fehlermeldung aus der Erprobung: nach der Serviceauswahl zeigt der Kalender keine Termine; erst ein Wechsel auf einen anderen Tag und zurück lädt die freien Plätze.

## User Story

Als Bürgerin oder Bürger möchte ich im zweiten Buchungsschritt einen Tag wählen und dessen freie Termine sehen, damit ich für jeden gebuchten Service einen passenden Zeitpunkt festlegen kann.

## Beschreibung

Die öffentliche Seite `/appointment` ist Schritt 2 des Buchungs-Wizards und ohne Anmeldung erreichbar.

Links steht ein Kalender, rechts die freien Termine des gewählten Tages. Gebucht wird je Service nacheinander; die Anzahl der benötigten Termine stammt aus Schritt 1.

Ein Tag ist genau dann anklickbar, wenn er tatsächlich Termine liefert. Kalender und Terminliste beruhen auf derselben Berechnung, damit ein auswählbarer Tag nicht leer sein kann.

Termine werden im Anzeigeraster von 30 Minuten angeboten. Am laufenden Tag entfallen Termine ohne ausreichenden Vorlauf. Zwischen zwei Terminen derselben Buchung gilt ein Mindestabstand.

## Akzeptanzkriterien

### Fachliches Verhalten

- Beim Öffnen der Seite ist kein Tag vorausgewählt; die Terminliste fordert zur Tageswahl auf.
- Ein Tag ist genau dann auswählbar, wenn für ihn mindestens ein auswählbarer Termin existiert.
- Ein Termin ist auswählbar, wenn er freie Kapazität besitzt, den Mindestabstand zu den bereits gewählten Terminen der anderen Services einhält und am laufenden Tag mindestens 30 Minuten in der Zukunft liegt.
- Termine werden auf ein Raster von 30 Minuten zusammengefasst; die angezeigte freie Kapazität eines Rasters ist die kleinste Kapazität der enthaltenen Termine.
- Ein Raster ohne freie Kapazität wird nicht angeboten.
- Termine werden aufsteigend nach Uhrzeit angezeigt.
- Vergangene Tage sind nicht auswählbar.
- Beim Wechsel des Monats wird die Tagesauswahl zurückgesetzt und die Verfügbarkeit des neuen Monats ermittelt.
- Beim Wechsel auf den nächsten Service wird die Tagesauswahl zurückgesetzt und die Verfügbarkeit unter Berücksichtigung der bereits gewählten Termine neu ermittelt.
- Die Termine des gewählten Tages werden beim Klick frisch geladen, damit zwischenzeitliche Buchungen anderer berücksichtigt sind.
- „Weiter" ist genau dann freigeschaltet, wenn für den aktuellen Service so viele Termine gewählt sind, wie Schritt 1 vorgibt.

### Berechtigungen und Sicherheit

- Die Seite ist anonym erreichbar und erfordert keine Anmeldung.
- Personenzahl, Service-Zuordnungen und gewählte Termine liegen im Browser-Session-Speicher und sind reiner Bedienzustand ohne Identität oder Berechtigung.
- Die Verfügbarkeit wird serverseitig aus Arbeitsplan, Öffnungszeiten, Ausnahmen und bestehenden Terminen berechnet und nicht aus dem Bedienzustand abgeleitet.

### Validierung und Fehlerfälle

- Solange die Verfügbarkeit eines Monats geladen wird, ist kein Tag auswählbar.
- Ein Monatswechsel während eines laufenden Ladevorgangs wird ignoriert.
- Die Auswahl eines Tages wird nicht stillschweigend verworfen. Liefert der Tag keine Termine, bleibt er gewählt und die Terminliste nennt den Grund.
- Liegt für den Service kein aktiver Arbeitsplan vor, ist kein Tag auswählbar.

### UI/UX

- Während die Verfügbarkeit eines Monats geladen wird, erscheint über dem Kalender ein Ladeindikator mit erklärendem Text.
- Während die Termine eines Tages geladen werden, erscheint an ihrer Stelle ein Ladeindikator mit erklärendem Text.
- Beide Ladehinweise werden Screenreadern als `aria-live`-Bereich bekannt gegeben.
- Ein Hinweistext erklärt, dass Tage ohne verfügbare Termine nicht auswählbar sind.
- Ohne gewählten Tag und ohne Termine am gewählten Tag erscheinen unterschiedliche, verständliche Meldungen statt einer leeren Fläche.
- Bei genau einem benötigten Termin ist die gesamte Terminkachel eine Schaltfläche und mit der Tastatur bedienbar.
- Alle Zähler-Schaltflächen an einem Termin besitzen einen zugänglichen Namen, der die Uhrzeit nennt.

### Betrieb und Nachvollziehbarkeit

- Die Regel, welche Termine auswählbar sind, liegt als reine Berechnung in der Application-Schicht und ist ohne UI und ohne Datenbank testbar.
- Der aktuelle Zeitpunkt wird der Berechnung übergeben, damit sie deterministisch prüfbar ist.
- Die Verfügbarkeit eines Monats wird mit einer Abfrage je Monat ermittelt, nicht mit einer Abfrage je Tag.
