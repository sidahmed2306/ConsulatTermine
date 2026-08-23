using ConsulatTermine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConsulatTermine.Infrastructure.Persistence.Mappings;

public sealed class WorkingSchedulePlanMapping : IEntityTypeConfiguration<WorkingSchedulePlan>
{
    public void Configure(EntityTypeBuilder<WorkingSchedulePlan> builder)
    {
        builder.ToTable("WorkingSchedulePlans");

        builder.HasKey(plan => plan.Id);

        builder.HasOne(plan => plan.Service)
            .WithMany()
            .HasForeignKey(plan => plan.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(plan => plan.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(plan => new { plan.ServiceId, plan.IsActive });
        builder.HasIndex(plan => new { plan.ServiceId, plan.ValidFromDate, plan.ValidToDate });
    }
}
