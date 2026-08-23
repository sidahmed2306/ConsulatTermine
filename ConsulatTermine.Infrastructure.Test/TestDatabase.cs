using ConsulatTermine.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ConsulatTermine.Infrastructure.Test;

/// <summary>
/// Isolierte relationale Testdatenbank auf Basis von SQLite in-memory.
///
/// Bewusst nicht der InMemory-Provider von EF Core: der ist nicht relational und
/// ignoriert Fremdschluessel, eindeutige Indizes und Laengenbegrenzungen. Genau die
/// sollen hier aber geprueft werden (harness/profile.md Abschnitt 13).
///
/// Die Verbindung bleibt offen, solange die Instanz lebt: SQLite verwirft eine
/// In-Memory-Datenbank, sobald die letzte Verbindung darauf geschlossen wird.
/// </summary>
public sealed class TestDatabase : IAsyncDisposable, IDbContextFactory<ApplicationDbContext>
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<ApplicationDbContext> _options;

    private TestDatabase(SqliteConnection connection, DbContextOptions<ApplicationDbContext> options)
    {
        _connection = connection;
        _options = options;
    }

    public static async Task<TestDatabase> CreateAsync(CancellationToken cancellationToken = default)
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync(cancellationToken);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        await using (var context = new ApplicationDbContext(options))
        {
            await context.Database.EnsureCreatedAsync(cancellationToken);
        }

        return new TestDatabase(connection, options);
    }

    public ApplicationDbContext CreateDbContext() => new(_options);

    public Task<ApplicationDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(CreateDbContext());

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
