# Personenzahl und Serviceauswahl

## Meta

- **State:** Implemented
- **Quelle:** Fehlermeldung aus dem Betrieb: „Bestätigen" wird bei zwei oder drei Personen nie freigeschaltet.

## User Story

Als Bürgerin oder Bürger möchte ich im ersten Buchungsschritt festlegen, für wie viele Personen ich buche und welche Services diese Personen benötigen, damit ich anschließend passende Termine auswählen kann.

## Beschreibung

Die öffentliche Seite `/services` ist Schritt 1 des Buchungs-Wizards und ohne Anmeldung erreichbar.

Sie besteht aus einem Zähler für die Personenzahl und einer Liste der buchbaren Services mit je einem eigenen Zähler. Der Zähler an einem Service gibt an, wie viele der angegebenen Personen diesen Service benötigen.

Erst wenn jede Person mindestens einen Termin zugeordnet bekommen hat, führt „Bestätigen" in Schritt 2. Die Auswahl wird im Browser-Session-Speicher abgelegt und ist ausdrücklich kein Sicherheitsmechanismus.

## Akzeptanzkriterien

### Fachliches Verhalten

- Die Personenzahl beträgt mindestens `1` und höchstens `5`.
- Ein einzelner Service kann höchstens einmal je Person belegt werden.
- Über alle Services hinweg sind höchstens `3` Termine je Person möglich.
- „Bestätigen" ist genau dann freigeschaltet, wenn die Summe aller Service-Zuordnungen mindestens der Personenzahl entspricht und die Obergrenze aus `Personenzahl × 3` nicht überschreitet.
- Zwei Personen mit zwei Terminen desselben Service erfüllen die Mindestanforderung.
- Zwei Personen mit je einem Termin an zwei verschiedenen Services erfüllen die Mindestanforderung ebenfalls.
- Wird die Personenzahl verringert, wird jeder Service-Zähler auf die neue Personenzahl gekappt und anschließend die Gesamtobergrenze durchgesetzt; abgebaut wird zuerst beim Service mit den meisten Zuordnungen.
- Wird die Personenzahl erhöht, bleiben bestehende Zuordnungen unverändert.
- „Zurücksetzen" setzt die Personenzahl auf `1` und entfernt alle Service-Zuordnungen.
- Nach „Bestätigen" stehen Personenzahl und Service-Zuordnungen für Schritt 2 zur Verfügung.

### Berechtigungen und Sicherheit

- Die Seite ist anonym erreichbar und erfordert keine Anmeldung.
- Personenzahl und Service-Zuordnungen liegen im Browser-Session-Speicher und sind reiner Bedienzustand. Sie tragen weder Identität noch Berechtigungen.
- Die Auswahlregeln werden vor der Weiterleitung erneut geprüft und nicht allein über den Zustand der Schaltfläche durchgesetzt.

### Validierung und Fehlerfälle

- Ein „+" an einem Service ist gesperrt, sobald der Service bereits jede Person abdeckt oder die Gesamtobergrenze erreicht ist.
- Ein „−" an einem Service ist gesperrt, solange dem Service kein Termin zugeordnet ist.
- Ein Zustand, in dem ein Service mehr Zuordnungen trägt als es Personen gibt, ist nicht bestätigbar und wird beim Ändern der Personenzahl automatisch bereinigt.
- Eine Personenzahl außerhalb von `1` bis `5` wird von der Auswahlberechnung abgewiesen.

### UI/UX

- Ein Hinweistext erklärt, dass der Zähler je Service die Anzahl der Personen meint und dass jede Person mindestens einen Termin benötigt.
- Solange die Mindestanforderung nicht erfüllt ist, nennt eine dauerhaft sichtbare Statusmeldung den aktuellen Stand und die Anzahl der noch fehlenden Termine.
- Ist die Mindestanforderung erfüllt, bestätigt die Statusmeldung, dass fortgefahren werden kann.
- Die Statusmeldung wird Screenreadern als `aria-live`-Bereich bekannt gegeben.
- Alle Zähler-Schaltflächen besitzen einen zugänglichen Namen, der Service und Richtung nennt.
- Stehen keine Services zur Verfügung, erscheint statt einer leeren Liste ein Hinweis.
- Während des Ladens erscheint ein Ladeindikator mit zugänglichem Namen.

### Betrieb und Nachvollziehbarkeit

- Die Auswahlregeln liegen als reine Berechnung in der Application-Schicht und sind ohne UI und ohne Datenbank testbar.
