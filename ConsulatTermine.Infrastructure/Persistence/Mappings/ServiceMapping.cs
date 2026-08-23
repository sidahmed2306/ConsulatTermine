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

        builder.Property(service => service.Description)
            .HasMaxLength(2000);

        builder.Property(service => service.Floor)
            .HasMaxLength(50);
    }
}
