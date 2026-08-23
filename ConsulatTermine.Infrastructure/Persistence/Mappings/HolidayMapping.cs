using ConsulatTermine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConsulatTermine.Infrastructure.Persistence.Mappings;

public sealed class HolidayMapping : IEntityTypeConfiguration<Holiday>
{
    public void Configure(EntityTypeBuilder<Holiday> builder)
    {
        builder.ToTable("Holidays");

        builder.HasKey(holiday => holiday.Id);

        builder.Property(holiday => holiday.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(holiday => holiday.Date);
    }
}
