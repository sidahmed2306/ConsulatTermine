using ConsulatTermine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConsulatTermine.Infrastructure.Persistence.Mappings;

public sealed class ServiceDayOverrideMapping : IEntityTypeConfiguration<ServiceDayOverride>
{
    public void Configure(EntityTypeBuilder<ServiceDayOverride> builder)
    {
        builder.ToTable("ServiceDayOverrides");

        builder.HasKey(dayOverride => dayOverride.Id);

        // Restrict wie bei WorkingHours: Ausnahmen sind Teil der Plan-Historie.
        builder.HasOne(dayOverride => dayOverride.Service)
            .WithMany(service => service.DayOverrides)
            .HasForeignKey(dayOverride => dayOverride.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(dayOverride => dayOverride.WorkingSchedulePlan)
            .WithMany()
            .HasForeignKey(dayOverride => dayOverride.WorkingSchedulePlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(dayOverride =>
            new { dayOverride.ServiceId, dayOverride.WorkingSchedulePlanId, dayOverride.Date });
    }
}
