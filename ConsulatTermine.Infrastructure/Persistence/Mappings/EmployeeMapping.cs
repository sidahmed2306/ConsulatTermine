using ConsulatTermine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConsulatTermine.Infrastructure.Persistence.Mappings;

public sealed class EmployeeMapping : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        builder.HasKey(employee => employee.Id);

        builder.Property(employee => employee.EmployeeCode)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(employee => employee.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(employee => employee.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(employee => employee.Email)
            .HasMaxLength(256)
            .IsRequired();

        // Format von PasswordHasher: Base64 ueber Version, Salt und Subkey.
        builder.Property(employee => employee.PasswordHash)
            .HasMaxLength(256);

        // SHA-256 als Hex: 64 Zeichen.
        builder.Property(employee => employee.PasswordResetTokenHash)
            .HasMaxLength(64);

        // Die Kennung wird systemseitig fortlaufend vergeben. Der eindeutige Index
        // verhindert doppelte Kennungen, wenn zwei Mitarbeiter gleichzeitig angelegt
        // werden: die zweite Anlage scheitert an der Datenbank statt still zu kollidieren.
        builder.HasIndex(employee => employee.EmployeeCode)
            .IsUnique();

        // Die E-Mail-Adresse identifiziert den Mitarbeiter beim Zuruecksetzen des
        // Passworts und muss daher eindeutig sein.
        builder.HasIndex(employee => employee.Email)
            .IsUnique();

        // Der Reset-Token wird bei jedem Aufruf des Links nachgeschlagen.
        builder.HasIndex(employee => employee.PasswordResetTokenHash);
    }
}
