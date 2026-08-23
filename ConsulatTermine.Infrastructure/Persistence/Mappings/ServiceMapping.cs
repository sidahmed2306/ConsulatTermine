using ConsulatTermine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConsulatTermine.Infrastructure.Persistence.Mappings;

public sealed class ServiceMapping : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("Services");

        builder.HasKey(service => service.Id);

        builder.Property(service => service.Name)
            .HasMaxLength(200)
            .IsRequired();

        // Die Uebersetzungen sind bewusst optional: ein Service bleibt buchbar,
        // solange nur die deutsche Bezeichnung gepflegt ist.
        builder.Property(service => service.NameEnglish)
            .HasMaxLength(200);

        builder.Property(service => service.NameArabic)
            .HasMaxLength(200);

        builder.Property(service => service.Description)
            .HasMaxLength(2000);

        builder.Property(service => service.DescriptionEnglish)
            .HasMaxLength(2000);

        builder.Property(service => service.DescriptionArabic)
            .HasMaxLength(2000);

        builder.Property(service => service.Floor)
            .HasMaxLength(50);
    }
}
