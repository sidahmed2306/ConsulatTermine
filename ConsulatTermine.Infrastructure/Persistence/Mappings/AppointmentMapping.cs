using ConsulatTermine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConsulatTermine.Infrastructure.Persistence.Mappings;

public sealed class AppointmentMapping : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");

        builder.HasKey(appointment => appointment.Id);

        builder.Property(appointment => appointment.FullName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(appointment => appointment.Email)
            .HasMaxLength(256);

        builder.Property(appointment => appointment.PhoneNumber)
            .HasMaxLength(50);

        builder.Property(appointment => appointment.BookingReference)
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(appointment => appointment.CancelToken)
            .HasMaxLength(64);

        // Kulturname im Format xx-YY. Bestandsdaten ohne Sprache erhalten Deutsch,
        // die bisher einzige Sprache des Schriftverkehrs.
        builder.Property(appointment => appointment.Language)
            .HasMaxLength(10)
            .IsRequired()
            .HasDefaultValue("de-DE");

        builder.HasOne(appointment => appointment.CurrentEmployee)
            .WithMany()
            .HasForeignKey(appointment => appointment.CurrentEmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        // Alle Termine einer Buchung teilen die Referenz. Der Absage-Link schlaegt
        // ueber genau diese Kombination nach.
        builder.HasIndex(appointment => appointment.BookingReference);

        // Traegt die Belegungsabfrage der Terminauswahl.
        builder.HasIndex(appointment => new { appointment.ServiceId, appointment.Date });

        // Traegt die Wartezimmer-Anzeige.
        builder.HasIndex(appointment => new { appointment.Status, appointment.IsVisibleInWaitingRoom });
    }
}
