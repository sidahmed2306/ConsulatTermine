using ConsulatTermine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConsulatTermine.Infrastructure.Persistence.Mappings;

public sealed class WorkingHoursMapping : IEntityTypeConfiguration<WorkingHours>
{
    public void Configure(EntityTypeBuilder<WorkingHours> builder)
    {
        builder.ToTable("WorkingHours");

        builder.HasKey(hours => hours.Id);

        builder.HasOne(hours => hours.WorkingSchedulePlan)
            .WithMany()
            .HasForeignKey(hours => hours.WorkingSchedulePlanId)
            .OnDelete(DeleteBehavior.Cascade);

        // Bewusst Restrict: Arbeitszeiten haengen fachlich am Plan, nicht am Service.
        // Ein Loeschen des Service darf die Historie nicht stillschweigend mitreissen.
        builder.HasOne(hours => hours.Service)
            .WithMany(service => service.WorkingHours)
            .HasForeignKey(hours => hours.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(hours => new { hours.ServiceId, hours.WorkingSchedulePlanId, hours.Day });
    }
}
