# Sensible Konfiguration (Secrets)

Die echten Werte für E-Mail und Datenbank stehen **nicht** in `appsettings.json`, damit sie nicht ins Repository gelangen.

## Lokale Entwicklung: User Secrets

Im Projektordner **ConsulatTermine.UI** ausführen:

```bash
cd ConsulatTermine.UI
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=ConsulatTermineDb;User Id=sa;Password=DEIN_PASSWORT;TrustServerCertificate=True;"
dotnet user-secrets set "Email:SmtpServer" "smtp.gmail.com"
dotnet user-secrets set "Email:Port" "587"
dotnet user-secrets set "Email:UseSsl" "true"
dotnet user-secrets set "Email:Username" "deine-email@gmail.com"
dotnet user-secrets set "Email:Password" "dein-app-passwort"
dotnet user-secrets set "Email:FromEmail" "deine-email@gmail.com"
dotnet user-secrets set "Email:FromName" "Konsulat – Terminservice"
```

Oder alle auf einmal aus einer lokalen Datei (diese Datei **nicht** committen):

```bash
dotnet user-secrets set --file ../meine-secrets.json
```

## Produktion

- **Azure:** Application Settings / Key Vault
- **Docker:** Umgebungsvariablen oder geheime Volumes
- **IIS / Server:** Umgebungsvariablen oder eine lokale `appsettings.Production.json` außerhalb des Deployments

Die Keys heißen z. B. `ConnectionStrings__DefaultConnection`, `Email__Password` (Doppelunterstrich statt `:` in Umgebungsvariablen).
