# UI- und UX-Richtlinien

Dieses Dokument definiert verbindliche UI- und UX-Regeln. Es gilt unabhängig von Framework und Komponentenbibliothek. Die konkret eingesetzte Bibliothek, das Theme-System und die Komponentenkonventionen stehen in `harness/profile.md`.

Abschnitte, die sich auf einen öffentlichen kundenorientierten Bereich beziehen, gelten nur, wenn das Projekt einen solchen besitzt. Ob das der Fall ist, steht in `PROJECT_CONTEXT.md`.

## 1. Grundlagen

- Das bestehende Corporate Design und die freigegebenen Markenfarben bilden die gestalterische Grundlage.
- Öffentliche und interne Bereiche dürfen unterschiedliche Layouts und Informationsdichten besitzen.
- Eine Komponentenbibliothek ist ein Werkzeug, keine verpflichtende Lösung für jede UI-Anforderung.
- Native Plattformelemente, eigene Komponenten und eigenes CSS sind ausdrücklich zulässig und werden bevorzugt, wenn sie einfacher, zugänglicher, performanter oder besser an das Design anpassbar sind.
- Es wird keine Komponentenbibliothek nur aus Gewohnheit verwendet.
- Eine ungeeignete Komponente wird nicht durch umfangreiche CSS- oder Skript-Hacks erzwungen, wenn eine klare Eigenentwicklung besser wäre.
- Gleichartige Funktionen besitzen konsistente Bezeichnungen, Icons, Reihenfolgen und Interaktionen.
- UI-Entscheidungen berücksichtigen Desktop, Tablet und mobile Nutzung von Beginn an.

## 2. Komponentenstrategie

Vor der Auswahl einer UI-Komponente werden mindestens geprüft:

- fachliche Eignung,
- Barrierefreiheit,
- Responsive-Verhalten,
- Performance,
- Wartbarkeit,
- Anpassbarkeit an das Projektdesign,
- Abhängigkeiten und Betriebsaufwand,
- Verhalten bei Lade-, Leer-, Fehler- und Berechtigungszuständen.

### Bibliothekskomponente verwenden

Eine Bibliothekskomponente ist sinnvoll, wenn sie die Anforderungen ohne unverhältnismäßige Anpassungen erfüllt. Typische Fälle:

- Dialoge,
- Formularfelder,
- Selects und Autocomplete,
- Date- und Time-Picker,
- Tabs,
- transiente Feedback-Elemente,
- einfache Tabellen und interne Datenansichten,
- Navigation und administrative Dashboards.

### Eigene Komponente verwenden

Eigene Komponenten sind regelmäßig geeigneter bei:

- markenprägenden Hero-Bereichen,
- Karten- und Kacheldarstellungen mit eigenem Layout,
- Galerien,
- Landingpages,
- Vergleichsdarstellungen,
- individuellen responsiven Tabellen,
- Medien- und Storytelling-Bereichen,
- fachlichen Prozess- oder Flussvisualisierungen,
- Komponenten, die mit einer Bibliothek nur durch viele Overrides umsetzbar wären.

## 3. Design-Tokens und Themes

Farben, Typografie, Abstände, Radien, Schatten, Breakpoints und Bewegungswerte werden zentral als Design-Tokens gepflegt, beispielsweise über CSS Custom Properties oder das Theme-System der eingesetzten Bibliothek.

Beispielhafte Rollen:

```text
--color-brand-primary
--color-brand-secondary
--color-surface
--color-surface-muted
--color-text
--color-text-muted
--color-border
--color-success
--color-warning
--color-error
--color-info
--space-1 bis --space-n
--radius-small bis --radius-large
```

- Keine beliebigen neuen Markenfarben komponentenweise einführen.
- Statusfarben nur passend zur Semantik verwenden.
- Status niemals ausschließlich über Farbe vermitteln; Text oder Icon ergänzen.
- Ein Dark Theme wird nur verbindlich, wenn es im Projekt vorhanden oder fachlich beauftragt ist.
- Eine Theme-Umschaltung darf Inhalt, Fokus, Navigation und Benutzerzustand nicht verlieren.

## 4. Barrierefreiheit

Alle UI-Änderungen berücksichtigen mindestens:

- vollständige Tastaturbedienung,
- sichtbaren Fokus,
- sinnvolle Tab-Reihenfolge,
- programmatisch zugeordnete Labels,
- semantische Überschriften und Landmarken,
- verständliche Fehlermeldungen,
- ausreichenden Kontrast,
- Skalierbarkeit bei Zoom,
- Bedienung ohne reine Farb-, Hover-, Drag- oder Mausinformation,
- zugängliche Namen für Icon-Buttons,
- reduzierte Bewegung über `prefers-reduced-motion` oder das Plattformäquivalent.

Kontrast-Richtwerte:

- normaler Text mindestens `4.5:1`,
- großer Text und wesentliche UI-Grafiken mindestens `3:1`.

Native semantische Elemente werden eigenen ARIA-Nachbildungen vorgezogen. ARIA ergänzt Semantik, ersetzt sie aber nicht.

## 5. Öffentliche und kundenorientierte Bereiche

Gilt nur, wenn das Projekt einen öffentlichen Bereich besitzt. Öffentliche Oberflächen sind zielorientiert, glaubwürdig und leicht scanbar.

### Übersichtsdarstellungen

- Inhalte werden mit klarer visueller Hierarchie dargestellt.
- Karten zeigen nur die wichtigsten Informationen.
- Die identifizierenden Merkmale eines Eintrags sind eindeutig unterscheidbar.
- Preise, Verfügbarkeit, Fristen und Bewertungen werden nur angezeigt, wenn reale Daten vorliegen.
- Bilder behalten ihr Seitenverhältnis und werden optimiert ausgeliefert.
- Primäre und sekundäre Aktionen sind klar priorisiert.
- Filter, Sortierung und Ergebnisanzahl sind nachvollziehbar.
- Aktive Filter sind sichtbar und können gezielt zurückgesetzt werden.

### Detailseiten

Eine Detailseite berücksichtigt abhängig von den verfügbaren Daten:

1. identifizierende Angaben,
2. Medien oder Galerie,
3. primäre Aktion,
4. verifizierte Highlights,
5. strukturierte Fachdaten,
6. Downloads und Dokumente,
7. Abhängigkeiten oder Kompatibilität,
8. verwandte Einträge,
9. Kontakt- oder Anfrageweg.

Fachliche Informationen sind strukturiert und nicht als unübersichtlicher Fließtext dargestellt. Fehlende optionale Daten führen nicht zu leeren oder gebrochenen Bereichen.

### Suche und Filter

- Suche berücksichtigt nur tatsächlich verfügbare Felder.
- Leere Suchanfragen und keine Treffer besitzen verständliche Zustände.
- Filterwerte entstehen aus realen Daten, nicht aus erfundenen Kategorien.
- Mobile Filter werden als gut bedienbarer Drawer, Dialog oder gleichwertige Lösung umgesetzt.
- Fachliche Filter müssen für die jeweilige Kategorie sinnvoll sein.

## 6. Interne Mitarbeiter- und Admin-Bereiche

Interne Oberflächen priorisieren Effizienz, Übersicht und Nachvollziehbarkeit.

- Seiten zeigen Status, Verantwortlichkeit und nächste Aktion klar an.
- Freigaben, Prüfungen und Integrationsfehler erhalten eindeutige Zustände.
- Kritische Aktionen zeigen Auswirkung, betroffenen Vorgang und Bestätigung.
- Benutzer sehen nur Funktionen, für die sie berechtigt sind; das Backend autorisiert zusätzlich.
- Tabellen, Filter und Stapelaktionen sind für wiederkehrende Arbeitsabläufe optimiert.
- Audit- und Fehlerdetails sind verständlich, ohne Secrets oder sensible technische Daten offenzulegen.

## 7. Seitenstruktur und Navigation

Eine fachliche Seite enthält in konsistenter Reihenfolge:

1. Seitentitel,
2. optional Breadcrumb oder Kontext,
3. kurze fachliche Einordnung, falls erforderlich,
4. primäre Aktionen,
5. Hauptinhalt,
6. Lade-, Leer-, Fehler-, Berechtigungs- oder Erfolgsfeedback.

- Pro Seite grundsätzlich eine visuell dominante Primäraktion.
- Destruktive Aktionen sind nicht als Primäraktion gestaltet.
- Navigationseinträge besitzen Bezeichnung, Route, Icon und gegebenenfalls erforderliche Policy.
- Ein fehlender Menüeintrag ersetzt keine Autorisierung.
- Öffentliche und interne Navigation werden nicht unnötig vermischt.
- Navigation bleibt auf kleinen Viewports vollständig bedienbar.

## 8. Formulare

- Felder folgen einer fachlich sinnvollen Reihenfolge.
- Jedes Feld besitzt ein sichtbares Label.
- Pflichtfelder werden verständlich gekennzeichnet.
- Platzhalter ersetzen keine Labels.
- Hilfe- und Formattexte stehen am zugehörigen Feld.
- Validierungsfehler stehen am betroffenen Feld; eine Zusammenfassung darf ergänzen.
- Benutzereingaben bleiben nach behebbaren Fehlern erhalten.
- Aktionen wie Speichern, Abbrechen, Freigeben und Zurückweisen werden konsistent bezeichnet.
- Während des Speicherns werden Mehrfachauslösungen verhindert und ein Status angezeigt.
- Nach Fehlern wird der Fokus auf den ersten relevanten Fehler oder die Zusammenfassung gesetzt.
- Destruktive oder schwer rückgängig zu machende Vorgänge benötigen eine eindeutige Bestätigung.
- Serverseitige Fehlermeldungen werden fachlich verständlich im Formular abgebildet.

## 9. Dialog oder Detailseite

Ein Dialog ist geeignet, wenn:

- die Aufgabe kurz und fokussiert ist,
- nur wenige überschaubare Eingaben erforderlich sind,
- kein komplexer Kontext dargestellt werden muss,
- die Aktion ohne verschachtelte modale Abläufe abgeschlossen werden kann.

Eine Detailseite ist geeignet, wenn:

- mehrere Abschnitte oder Abhängigkeiten bestehen,
- längere Hilfetexte oder Prüfungen erforderlich sind,
- der Vorgang eine eigene URL oder Wiederaufnahme benötigt,
- viele Felder, Tabellen oder Statusinformationen angezeigt werden.

Dialoge besitzen Titel, eindeutige Aktionen, Fokusbegrenzung und sinnvolles Fokus-Return. Dialoge öffnen keine weitere modale Ebene.

## 10. Feedbackzustände

Jede datenabhängige Ansicht behandelt:

- Laden,
- keine Daten,
- keine Such- oder Filtertreffer,
- Fehler,
- fehlende Berechtigung,
- Erfolg nach einer Aktion,
- gegebenenfalls veraltete oder noch zu synchronisierende Daten.

Regeln:

- Nicht nur einen leeren Bereich anzeigen.
- Ladeindikatoren mit verständlichem Kontext versehen.
- Fehlermeldungen erklären, was passiert ist und welche nächste Aktion möglich ist.
- Technische Details gehören ins Log, nicht in die öffentliche UI.
- Erfolgsmeldungen sind knapp und nennen das Ergebnis.
- Temporäre Toasts ersetzen keine dauerhaft benötigten Informationen.
- Wiederholbare technische Fehler bieten nur dann eine Retry-Aktion, wenn sie sicher und idempotent ist.

## 11. Tabellen und Datenraster

Vor der Umsetzung wird entschieden, ob eine Bibliothekskomponente, eine semantische Tabelle oder eine eigene Komponente am besten passt.

Eine Bibliothekskomponente wird nur eingesetzt, wenn sie ohne unverhältnismäßige Anpassungen erfüllt:

- benötigte Sortierung,
- Filterung,
- Pagination oder Virtualisierung,
- Auswahl und Stapelaktionen,
- Tastaturbedienung,
- zugängliche Beschriftung,
- responsive Darstellung,
- Performance mit der erwarteten Datenmenge.

### Allgemeine Regeln

- Eine fachlich sinnvolle Standardsortierung ist gesetzt.
- Große Datenmengen werden serverseitig paginiert oder anderweitig begrenzt.
- Tabellen erhalten flache Präsentationsmodelle, keine vollständigen Objektgraphen.
- Spalten zeigen fachlich verständliche Bezeichnungen in der Projektsprache.
- Aktive Filter sind erkennbar.
- Ein Zurücksetzen der Filter ist verfügbar, wenn mehrere Filter bestehen.
- Keine Daten und kein Filtertreffer sind unterschiedliche Zustände.
- Aktionen sind ohne erfüllte Vorbedingungen deaktiviert.
- Inline-Aktionen werden auf wenige, häufige und eindeutig erkennbare Aktionen begrenzt.
- Stapelaktionen zeigen Anzahl und Wirkung der Auswahl.
- Checkbox-Auswahl wird verwendet, wenn sie zugänglicher und verständlicher ist als Drag- oder reine Mausauswahl.
- Horizontales Scrollen wird minimiert, aber fachlich notwendige Informationen werden nicht stillschweigend entfernt.
- Auf kleinen Viewports darf eine tabellarische Darstellung in Karten oder eine geeignete Detailansicht überführt werden.

## 12. Buttons und Aktionen

- Buttons verwenden handlungsorientierte Verben in der Projektsprache.
- Primäre Aktion: visuell dominant und eindeutig.
- Sekundäre Aktion: weniger dominant.
- Destruktive Aktion: Error-Semantik und eindeutige Beschriftung.
- Icon-only nur bei etablierten, platzkritischen Aktionen und immer mit zugänglichem Namen sowie Tooltip.
- Touch-Ziele sind ausreichend groß; Richtwert mindestens `44 x 44 px`.
- Keine Aktion ausschließlich per Doppelklick, Rechtsklick, Hover oder Drag anbieten.
- Freigabe- und zahlungsrelevante Aktionen nennen den betroffenen Vorgang und die Konsequenz.

## 13. Medien, Animation und Performance

- Bilder werden in geeigneten Formaten und Größen ausgeliefert.
- Responsive Images, Lazy Loading und feste Abmessungen vermeiden unnötige Datenmenge und Layout Shifts.
- Autoplay-Medien sind stumm, kontrollierbar und besitzen einen sinnvollen Fallback.
- Animationen unterstützen Orientierung oder Verständnis und sind nicht rein dekorativ.
- Bewegungen bleiben dezent und respektieren reduzierte Bewegung.
- Keine große Bibliothek nur für einen kleinen visuellen Effekt einführen.
- Öffentliche Seiten priorisieren Ladezeit, auffindbare Inhalte und stabile Darstellung.

## 14. UI-Abnahme

Vor Abschluss einer UI-Story prüfen:

- [ ] relevante Desktop-, Tablet- und Mobile-Viewports,
- [ ] Tastaturbedienung,
- [ ] sichtbarer Fokus,
- [ ] Zoom,
- [ ] Ladezustand,
- [ ] Leerzustand,
- [ ] kein Such- oder Filtertreffer,
- [ ] Validierungs- und Fehlerzustand,
- [ ] fehlende Berechtigung,
- [ ] lange Texte und große Datenwerte,
- [ ] Kontrast und Statusdarstellung,
- [ ] reduzierte Bewegung,
- [ ] reale oder realistische Datenmenge,
- [ ] vorhandene Themes, sofern mehrere unterstützt werden.

Begründete Abweichungen werden in der Story oder als Designentscheidung dokumentiert.
