# Sicherheits-, Authentifizierungs- und Autorisierungsregeln

Dieses Dokument definiert verbindliche Sicherheitsregeln für öffentliche Funktionen, Endkundenkonten, interne Mitarbeiterbereiche und technische Systemzugriffe. Es gilt stack-unabhängig; die konkreten Framework-Mechanismen stehen in `harness/profile.md`.

## 1. Grundsatz

Sicherheit wird nicht ausschließlich im UI umgesetzt. Jede geschützte Aktion wird serverseitig autorisiert und validiert.

Es gelten Least Privilege, sichere Voreinstellungen, Trennung von Verantwortlichkeiten und nachvollziehbare Freigaben.

Keine eigene Kryptografie, kein eigenes Passwortformat, kein selbst erfundenes Tokenformat und keine eigene Signaturprüfung entwickeln, wenn etablierte Framework- oder Providermechanismen verfügbar sind.

## 2. Identitätstypen

Mindestens folgende Identitätstypen werden unterschieden, sofern sie im Projekt vorkommen:

### Anonyme Besucher

- dürfen nur explizit öffentliche Inhalte und Aktionen verwenden,
- erhalten keinen Zugriff auf interne oder kundenspezifische Daten,
- sehen geschützte Informationen nur entsprechend der fachlichen Spec.

### Endkunden oder externe Benutzer

- greifen nur auf das eigene Konto und die eigenen Vorgänge zu,
- werden über einen freigegebenen Identity-Mechanismus authentifiziert,
- dürfen keine fremden Konto-, Preis-, Zahlungs- oder Vorgangsdaten abrufen.

### Interne Mitarbeiter

- verwenden einen freigegebenen Unternehmens-Identity-Provider oder eine andere dokumentierte Mitarbeiteranmeldung,
- erhalten fachliche Policies entsprechend ihrer Aufgabe,
- dürfen kritische Aktionen nur mit minimal erforderlichen Rechten durchführen.

### Technische Systeme

- externe Systeme erhalten eigene technische Identitäten,
- technische Identitäten werden nicht als normale Benutzerkonten wiederverwendet,
- jede Integration erhält nur die für ihren Zweck notwendigen Scopes und Berechtigungen.

Der konkrete Identity Provider ist keine Harness-Vorgabe. Er wird nach Analyse der vorhandenen Systeme und Anforderungen entschieden und in `PROJECT_CONTEXT.md` dokumentiert.

## 3. Authentifizierung

- Die Authentifizierungsmechanismen des eingesetzten Frameworks und freigegebene Provider verwenden.
- Öffentliche und geschützte Routen werden bewusst konfiguriert.
- Interne Bereiche sind standardmäßig geschützt.
- Kundenbereiche erfordern eine verifizierte Identität.
- Anmeldung, Abmeldung, Session-Ablauf und erneute Anmeldung besitzen definierte UI-Zustände.
- Multi-Faktor-Authentifizierung wird für privilegierte Zugriffe berücksichtigt, sofern der Provider dies unterstützt oder die Sicherheitsbewertung es verlangt.
- Authentifizierungsfehler offenbaren nicht, ob ein bestimmtes Konto existiert.

## 4. Autorisierung

Autorisierung erfolgt policy-basiert.

Beispielhafte Policy-Benennung:

```text
<Bereich>.View
<Bereich>.Approve
<Bereich>.Retry
Administration.ManageUsers
```

Diese Namen sind reine Formatbeispiele. Tatsächliche Policies werden aus freigegebenen Anforderungen übernommen und nicht vom Agenten erfunden. Die im Projekt gültigen Policies stehen in `PROJECT_CONTEXT.md` oder den Specs.

### UI

- Die Autorisierungsmechanismen des Frameworks steuern Zugriff, Sichtbarkeit und Benutzerführung.
- Ein fehlender Menüeintrag oder ausgeblendeter Button ist keine Sicherheitsgrenze.
- Fehlende Berechtigung wird verständlich angezeigt.

### Backend

- Jede geschützte Zustandsänderung wird serverseitig autorisiert.
- Direkte API-, Anwendungsfall- oder Hintergrundaufrufe dürfen UI-Prüfungen nicht umgehen.
- Objektbezogene Berechtigungen werden geprüft, wenn ein Benutzer nur bestimmte Kunden, Mandanten, Aufträge oder Standorte bearbeiten darf.
- Status und Berechtigung werden gemeinsam geprüft; eine Berechtigung allein erlaubt keinen fachlich ungültigen Statusübergang.

## 5. Freigaben und Vier-Augen-Prinzip

- Fachlich sensible Freigaben werden als eigener, nachvollziehbarer Anwendungsfall modelliert.
- Freigebender Benutzer, Zeitpunkt, Ausgangsstatus, Zielstatus und relevante Begründung werden auditiert.
- Wenn ein Vier-Augen-Prinzip erforderlich ist, darf Ersteller und Freigeber nicht dieselbe Person sein.
- Automatisierte Folgeschritte starten erst nach erfolgreicher und autorisierter Freigabe.
- Wiederholte Freigabeaufrufe dürfen keine doppelten Vorgänge, Zahlungsanforderungen oder Benachrichtigungen auslösen.

## 6. Service-to-Service-Sicherheit

- Bevorzugt OAuth2 Client Credentials, mTLS oder vom Anbieter vorgesehene sichere Verfahren verwenden.
- API-Keys nur verwenden, wenn der externe Dienst dies erfordert und eine sichere Verwaltung ermöglicht.
- Credentials pro Integration und Umgebung trennen.
- Secrets regelmäßig rotierbar halten.
- Berechtigungen auf konkrete Endpunkte und Aktionen begrenzen.
- Technische Identitäten besitzen keine interaktive Anmeldung, sofern nicht zwingend erforderlich.

## 7. Webhooks

- Signatur, Secret, Zertifikat oder einen gleichwertigen Herkunftsnachweis prüfen.
- Zeitstempel und Replay-Schutz berücksichtigen, sofern der Anbieter dies unterstützt.
- Payload-Größe und Content-Type begrenzen.
- Eingehende Webhooks serverseitig validieren.
- Webhook-Ereignisse idempotent verarbeiten.
- Sensible Payloads nicht vollständig loggen.
- Unbekannte Eventtypen sicher ablehnen oder kontrolliert ignorieren.

## 8. Secrets und Konfiguration

- Keine Secrets im Repository, in Markdown-Dateien, Beispielkonfigurationen, Screenshots, Logs oder exportierten Integrationsdefinitionen.
- Lokale Entwicklung verwendet den in `harness/profile.md` genannten lokalen Secret-Mechanismus.
- Nichtproduktive und produktive Secrets sind getrennt.
- Secrets werden über typisierte Konfiguration gebunden, aber niemals als Klartext ausgegeben.
- Fehlende Secrets führen zu einem klaren Konfigurationsfehler.
- `.env`, lokale Secret-Dateien und benutzerspezifische Settings werden über `.gitignore` ausgeschlossen.

## 9. Personenbezogene, Zahlungs- und Kundendaten

- Nur für den Prozess notwendige Daten erheben und verarbeiten.
- Zahlungsinstrumente und vollständige Bankdaten werden nicht unnötig in der eigenen Anwendung gespeichert.
- Zahlungsdaten werden bevorzugt durch einen dafür vorgesehenen Payment Provider verarbeitet.
- Logs, Fehlermeldungen, Testdaten und Telemetrie enthalten keine unnötigen personenbezogenen oder finanziellen Daten.
- Datenzugriffe und Exporte werden nach Schutzbedarf autorisiert und auditiert.
- Aufbewahrung, Löschung und Auskunftsfähigkeit werden fachlich und rechtlich geklärt.

## 10. Fehler und Benutzerfeedback

- Öffentliche Fehlermeldungen enthalten keine Stack Traces, Secrets, internen IDs, Providerantworten oder Systempfade.
- Authentifizierungs- und Autorisierungsfehler werden nicht mit technischen Details erklärt.
- Technische Diagnoseinformationen stehen nur in geschützten Logs.
- Sicherheitsrelevante Auffälligkeiten werden mit geeigneter Schwere protokolliert und überwacht.

## 11. Tests

Mindestens testen:

- anonyme Zugriffe auf geschützte Funktionen,
- angemeldete Benutzer ohne erforderliche Policy,
- Benutzer mit zulässiger Policy,
- objektbezogene Berechtigungen,
- direkte serverseitige Aufrufe ohne UI,
- ungültige Statusübergänge,
- doppelte Freigaben,
- ungültige oder manipulierte Webhook-Signaturen,
- fehlende oder abgelaufene technische Credentials,
- Ausschluss sensibler Informationen aus Fehlermeldungen.

## 12. Architekturentscheidungen

Ein ADR ist erforderlich, wenn insbesondere:

- ein neuer Identity Provider eingeführt wird,
- Kunden- und Mitarbeiteridentitäten zusammengeführt werden,
- ein neues Token- oder Service-to-Service-Verfahren eingeführt wird,
- eine neue Rollen-, Mandanten- oder Berechtigungsstruktur entsteht,
- Zahlungs- oder besonders schützenswerte Daten neu gespeichert werden.
