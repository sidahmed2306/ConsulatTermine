using ConsulatTermine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConsulatTermine.Infrastructure.Persistence.Mappings;

public sealed class EmployeeServiceAssignmentMapping : IEntityTypeConfiguration<EmployeeServiceAssignment>
{
    public void Configure(EntityTypeBuilder<EmployeeServiceAssignment> builder)
    {
        builder.ToTable("EmployeeServiceAssignments");

        builder.HasKey(assignment => new { assignment.EmployeeId, assignment.ServiceId });

        builder.HasOne(assignment => assignment.Employee)
            .WithMany(employee => employee.AssignedServices)
            .HasForeignKey(assignment => assignment.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(assignment => assignment.Service)
            .WithMany(service => service.AssignedEmployees)
            .HasForeignKey(assignment => assignment.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
