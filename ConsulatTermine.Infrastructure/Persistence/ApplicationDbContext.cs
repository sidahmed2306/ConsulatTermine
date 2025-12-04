using ConsulatTermine.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConsulatTermine.Infrastructure.Persistence;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // n:n über Join-Entity EmployeeServiceAssignment
        modelBuilder.Entity<EmployeeServiceAssignment>()
            .HasKey(x => new { x.EmployeeId, x.ServiceId });

        modelBuilder.Entity<EmployeeServiceAssignment>()
            .HasOne(x => x.Employee)
            .WithMany(e => e.AssignedServices)
            .HasForeignKey(x => x.EmployeeId);

        modelBuilder.Entity<EmployeeServiceAssignment>()
            .HasOne(x => x.Service)
            .WithMany(s => s.AssignedEmployees)
            .HasForeignKey(x => x.ServiceId);
    }
}
