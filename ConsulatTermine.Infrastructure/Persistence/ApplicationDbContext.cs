using ConsulatTermine.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConsulatTermine.Infrastructure.Persistence;

/// <summary>
/// Datenbankkontext der Anwendung.
/// Die Zuordnung der Entitaeten liegt in je einer <see cref="IEntityTypeConfiguration{TEntity}"/>
/// unter <c>Persistence/Mappings</c>, damit dieser Typ nicht zur Sammelstelle wird
/// (harness/profile.md Abschnitt 11).
///
/// Wird ueber <c>IDbContextFactory</c> erzeugt, nicht als Scoped Service: ein
/// Blazor-Server-Circuit lebt so lange wie die Registerkarte des Benutzers.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Service> Services => Set<Service>();

    public DbSet<WorkingHours> WorkingHours => Set<WorkingHours>();

    public DbSet<ServiceDayOverride> ServiceDayOverrides => Set<ServiceDayOverride>();

    public DbSet<Holiday> Holidays => Set<Holiday>();

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<EmployeeServiceAssignment> EmployeeServiceAssignments => Set<EmployeeServiceAssignment>();

    public DbSet<Appointment> Appointments => Set<Appointment>();

    public DbSet<WorkingSchedulePlan> WorkingSchedulePlans => Set<WorkingSchedulePlan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
