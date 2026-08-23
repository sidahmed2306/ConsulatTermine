using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ConsulatTermine.Infrastructure.Persistence;

/// <summary>
/// Erzeugt den Kontext fuer die EF-Core-Werkzeuge (<c>dotnet ef migrations</c>).
/// Migrationen gehoeren zum Projekt, das die Daten besitzt; die Werkzeuge brauchen dafuer
/// keinen Zugriff auf das Webprojekt und keine echte Datenbank.
///
/// Die Verbindungszeichenfolge dient ausschliesslich dazu, den SQL-Server-Provider zu
/// waehlen. Sie wird beim Erzeugen einer Migration nicht geoeffnet und enthaelt daher
/// bewusst keine Zugangsdaten. Fuer <c>database update</c> wird sie ueber die
/// Umgebungsvariable CONSULATTERMINE_CONNECTIONSTRING gesetzt.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    private const string ConnectionStringVariable = "CONSULATTERMINE_CONNECTIONSTRING";

    private const string DesignTimePlaceholder =
        "Server=(localdb)\\MSSQLLocalDB;Database=ConsulatTermineDesignTime;Trusted_Connection=True;";

    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable(ConnectionStringVariable) ?? DesignTimePlaceholder;

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new ApplicationDbContext(options);
    }
}
