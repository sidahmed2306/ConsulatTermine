using ConsulatTermine.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConsulatTermine.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // ----------------------------------------------------
    // DbSets
    // ----------------------------------------------------
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

        // ====================================================
        // Employee <-> Service (n:n)
        // ====================================================
        modelBuilder.Entity<EmployeeServiceAssignment>()
            .HasKey(x => new { x.EmployeeId, x.ServiceId });

        modelBuilder.Entity<EmployeeServiceAssignment>()
            .HasOne(x => x.Employee)
            .WithMany(e => e.AssignedServices)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EmployeeServiceAssignment>()
            .HasOne(x => x.Service)
            .WithMany(s => s.AssignedEmployees)
            .HasForeignKey(x => x.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Appointment>()
    .HasOne(a => a.CurrentEmployee)
    .WithMany()
    .HasForeignKey(a => a.CurrentEmployeeId)
    .OnDelete(DeleteBehavior.SetNull);



        // ====================================================
        // WorkingSchedulePlan (Plan-Header)
        // ====================================================
        modelBuilder.Entity<WorkingSchedulePlan>(e =>
        {
            e.ToTable("WorkingSchedulePlans");
            e.HasKey(x => x.Id);

            e.HasOne(x => x.Service)
             .WithMany()
             .HasForeignKey(x => x.ServiceId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(x => x.IsActive)
             .HasDefaultValue(true);

            e.HasIndex(x => new { x.ServiceId, x.IsActive });
            e.HasIndex(x => new { x.ServiceId, x.ValidFromDate, x.ValidToDate });
        });


        // ====================================================
        // WorkingHours  -> Plan (CASCADE)
        // ====================================================
        modelBuilder.Entity<WorkingHours>(e =>
        {
            e.HasOne(x => x.WorkingSchedulePlan)
             .WithMany()
             .HasForeignKey(x => x.WorkingSchedulePlanId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => new { x.ServiceId, x.WorkingSchedulePlanId, x.Day });
        });

        modelBuilder.Entity<WorkingHours>()
    .HasOne(w => w.Service)
    .WithMany(s=>s.WorkingHours)
    .HasForeignKey(w => w.ServiceId)
    .OnDelete(DeleteBehavior.Restrict); // ⬅️ WICHTIG



        // ====================================================
        // ServiceDayOverride
        // ====================================================

        // 1️⃣ Override -> Service  (NO CASCADE ❗)
       modelBuilder.Entity<ServiceDayOverride>()
    .HasOne(o => o.Service)
    .WithMany(s => s.DayOverrides) // 👈 explizite Navigation
    .HasForeignKey(o => o.ServiceId)
    .OnDelete(DeleteBehavior.Restrict);


        // 2️⃣ Override -> Plan (CASCADE ✅)
        modelBuilder.Entity<ServiceDayOverride>(e =>
        {
            e.HasOne(x => x.WorkingSchedulePlan)
             .WithMany()
             .HasForeignKey(x => x.WorkingSchedulePlanId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => new { x.ServiceId, x.WorkingSchedulePlanId, x.Date });
        });
    }
}
