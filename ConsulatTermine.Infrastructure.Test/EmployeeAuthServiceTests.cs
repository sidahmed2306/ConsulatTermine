using ConsulatTermine.Application.Configuration;
using ConsulatTermine.Application.Interfaces;
using ConsulatTermine.Domain.Entities;
using ConsulatTermine.Domain.Enums;
using ConsulatTermine.Infrastructure.Security;
using ConsulatTermine.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace ConsulatTermine.Infrastructure.Test;

/// <summary>
/// Tests der Mitarbeiteranmeldung. Schwerpunkt sind die Sicherheitsregeln aus
/// harness/security.md: keine Klartext-Passwoerter, keine Rueckschluesse auf die
/// Existenz eines Kontos, Begrenzung von Fehlversuchen, Token nur als Hash.
/// </summary>
public sealed class EmployeeAuthServiceTests
{
    private const string ValidPassword = "Ein-Gutes-Passwort-42";

    private static readonly EmployeeLoginOptions LoginOptions = new()
    {
        MaxFailedAttempts = 3,
        LockoutDuration = TimeSpan.FromMinutes(15),
        PasswordResetTokenLifetime = TimeSpan.FromHours(1)
    };

    private static (EmployeeAuthService Service, IEmailService Email, FakeTimeProvider Time)
        CreateService(TestDatabase database)
    {
        var email = Substitute.For<IEmailService>();
        var time = new FakeTimeProvider(new DateTimeOffset(2026, 8, 23, 10, 0, 0, TimeSpan.Zero));

        var service = new EmployeeAuthService(
            database,
            email,
            new PasswordHasher<Employee>(),
            Options.Create(new ApplicationOptions { BaseUrl = "https://termine.example.org" }),
            Options.Create(LoginOptions),
            time,
            NullLogger<EmployeeAuthService>.Instance);

        return (service, email, time);
    }

    private static async Task<Employee> SeedEmployeeAsync(
        TestDatabase database,
        string code = "CDZ-001",
        bool isActive = true,
        string password = ValidPassword)
    {
        var employee = new Employee
        {
            EmployeeCode = code,
            FirstName = "Amina",
            LastName = "Benali",
            Email = $"{code.ToLowerInvariant()}@example.org",
            Role = EmployeeRole.Employee,
            IsActive = isActive,
            MustChangePassword = false
        };

        employee.PasswordHash = new PasswordHasher<Employee>().HashPassword(employee, password);

        await using var context = database.CreateDbContext();
        context.Employees.Add(employee);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        return employee;
    }

    [Fact]
    public async Task LoginAsync_MitRichtigemPasswort_MeldetDenMitarbeiterAn()
    {
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        var seeded = await SeedEmployeeAsync(database);
        var (service, _, _) = CreateService(database);

        var result = await service.LoginAsync("CDZ-001", ValidPassword, TestContext.Current.CancellationToken);

        Assert.True(result.Success);
        Assert.Equal(seeded.Id, result.EmployeeId);
        Assert.Equal(EmployeeRole.Employee, result.Role);
    }

    [Fact]
    public async Task LoginAsync_MitFuehrendenLeerzeichenInDerKennung_MeldetTrotzdemAn()
    {
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        await SeedEmployeeAsync(database);
        var (service, _, _) = CreateService(database);

        var result = await service.LoginAsync("  CDZ-001  ", ValidPassword, TestContext.Current.CancellationToken);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task LoginAsync_MitFalschemPasswort_ScheitertOhneHinweisAufDasKonto()
    {
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        await SeedEmployeeAsync(database);
        var (service, _, _) = CreateService(database);

        var result = await service.LoginAsync("CDZ-001", "falsch", TestContext.Current.CancellationToken);

        Assert.False(result.Success);
        Assert.Null(result.EmployeeId);
    }

    [Fact]
    public async Task LoginAsync_MeldetBeiUnbekannterKennungDieselbeMeldungWieBeiFalschemPasswort()
    {
        // Sonst liesse sich ueber die Anmeldemaske ermitteln, welche Kennungen es gibt.
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        await SeedEmployeeAsync(database);
        var (service, _, _) = CreateService(database);

        var wrongPassword = await service.LoginAsync("CDZ-001", "falsch", TestContext.Current.CancellationToken);
        var unknownCode = await service.LoginAsync("CDZ-999", "falsch", TestContext.Current.CancellationToken);
        var inactive = await service.LoginAsync("CDZ-002", ValidPassword, TestContext.Current.CancellationToken);

        Assert.Equal(wrongPassword.ErrorMessage, unknownCode.ErrorMessage);
        Assert.Equal(wrongPassword.ErrorMessage, inactive.ErrorMessage);
    }

    [Fact]
    public async Task LoginAsync_BeiDeaktiviertemKonto_Scheitert()
    {
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        await SeedEmployeeAsync(database, isActive: false);
        var (service, _, _) = CreateService(database);

        var result = await service.LoginAsync("CDZ-001", ValidPassword, TestContext.Current.CancellationToken);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task LoginAsync_NachZuVielenFehlversuchen_SperrtDasKontoAuchFuerDasRichtigePasswort()
    {
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        await SeedEmployeeAsync(database);
        var (service, _, _) = CreateService(database);

        for (var attempt = 0; attempt < LoginOptions.MaxFailedAttempts; attempt++)
        {
            await service.LoginAsync("CDZ-001", "falsch", TestContext.Current.CancellationToken);
        }

        var afterLockout = await service.LoginAsync("CDZ-001", ValidPassword, TestContext.Current.CancellationToken);

        Assert.False(afterLockout.Success);
    }

    [Fact]
    public async Task LoginAsync_NachAblaufDerSperre_IstDieAnmeldungWiederMoeglich()
    {
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        await SeedEmployeeAsync(database);
        var (service, _, time) = CreateService(database);

        for (var attempt = 0; attempt < LoginOptions.MaxFailedAttempts; attempt++)
        {
            await service.LoginAsync("CDZ-001", "falsch", TestContext.Current.CancellationToken);
        }

        time.Advance(LoginOptions.LockoutDuration + TimeSpan.FromMinutes(1));

        var result = await service.LoginAsync("CDZ-001", ValidPassword, TestContext.Current.CancellationToken);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task LoginAsync_NachErfolgreicherAnmeldung_SindDieFehlversucheZurueckgesetzt()
    {
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        await SeedEmployeeAsync(database);
        var (service, _, _) = CreateService(database);

        await service.LoginAsync("CDZ-001", "falsch", TestContext.Current.CancellationToken);
        await service.LoginAsync("CDZ-001", ValidPassword, TestContext.Current.CancellationToken);

        await using var context = database.CreateDbContext();
        var employee = await context.Employees.SingleAsync(TestContext.Current.CancellationToken);

        Assert.Equal(0, employee.FailedLoginAttempts);
        Assert.Null(employee.LockoutEndsAt);
    }

    [Fact]
    public async Task ChangePasswordAsync_SpeichertDasPasswortNiemalsImKlartext()
    {
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        var seeded = await SeedEmployeeAsync(database);
        var (service, _, _) = CreateService(database);

        const string newPassword = "Neues-Passwort-2026";
        await service.ChangePasswordAsync(seeded.Id, newPassword, TestContext.Current.CancellationToken);

        await using var context = database.CreateDbContext();
        var employee = await context.Employees.SingleAsync(TestContext.Current.CancellationToken);

        Assert.NotNull(employee.PasswordHash);
        Assert.DoesNotContain(newPassword, employee.PasswordHash, StringComparison.Ordinal);
        Assert.False(employee.MustChangePassword);
    }

    [Fact]
    public async Task ChangePasswordAsync_ErlaubtDieAnmeldungMitDemNeuenUndNichtMehrMitDemAltenPasswort()
    {
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        var seeded = await SeedEmployeeAsync(database);
        var (service, _, _) = CreateService(database);

        const string newPassword = "Neues-Passwort-2026";
        await service.ChangePasswordAsync(seeded.Id, newPassword, TestContext.Current.CancellationToken);

        Assert.True((await service.LoginAsync("CDZ-001", newPassword, TestContext.Current.CancellationToken)).Success);
        Assert.False((await service.LoginAsync("CDZ-001", ValidPassword, TestContext.Current.CancellationToken)).Success);
    }

    [Fact]
    public async Task RequestPasswordResetAsync_MeldetAuchBeiUnbekannterAdresseErfolg()
    {
        // Sonst liesse sich ermitteln, welche Adressen im System bekannt sind.
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        var (service, email, _) = CreateService(database);

        var result = await service.RequestPasswordResetAsync("niemand@example.org", TestContext.Current.CancellationToken);

        Assert.True(result);
        await email.DidNotReceiveWithAnyArgs()
            .SendEmployeePasswordResetEmailAsync(default!, default!, default!);
    }

    [Fact]
    public async Task RequestPasswordResetAsync_SpeichertNurDenHashDesTokens()
    {
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        var seeded = await SeedEmployeeAsync(database);
        var (service, email, _) = CreateService(database);

        string? sentLink = null;
        await email.SendEmployeePasswordResetEmailAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Do<string>(link => sentLink = link));

        await service.RequestPasswordResetAsync(seeded.Email, TestContext.Current.CancellationToken);

        await using var context = database.CreateDbContext();
        var employee = await context.Employees.SingleAsync(TestContext.Current.CancellationToken);

        Assert.NotNull(employee.PasswordResetTokenHash);
        Assert.NotNull(sentLink);
        // Der Klartext des Tokens steckt im Link, nicht in der Datenbank.
        Assert.DoesNotContain(employee.PasswordResetTokenHash!, sentLink!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ResetPasswordWithTokenAsync_MitGueltigemToken_SetztDasPasswortUndEntwertetDenLink()
    {
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        var seeded = await SeedEmployeeAsync(database);
        var (service, _, _) = CreateService(database);

        var (token, hash) = PasswordResetToken.Create();
        await using (var context = database.CreateDbContext())
        {
            var employee = await context.Employees.SingleAsync(TestContext.Current.CancellationToken);
            employee.PasswordResetTokenHash = hash;
            employee.PasswordResetTokenExpiresAt = new DateTime(2026, 8, 23, 11, 0, 0, DateTimeKind.Utc);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        const string newPassword = "Zurueckgesetzt-2026";
        Assert.True(await service.ResetPasswordWithTokenAsync(token, newPassword, TestContext.Current.CancellationToken));

        // Ein zweiter Aufruf mit demselben Link muss scheitern.
        Assert.False(await service.ResetPasswordWithTokenAsync(token, "Noch-Ein-Passwort", TestContext.Current.CancellationToken));
        Assert.True((await service.LoginAsync("CDZ-001", newPassword, TestContext.Current.CancellationToken)).Success);

        _ = seeded;
    }

    [Fact]
    public async Task ResetPasswordWithTokenAsync_MitAbgelaufenemToken_WirdAbgelehnt()
    {
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        await SeedEmployeeAsync(database);
        var (service, _, time) = CreateService(database);

        var (token, hash) = PasswordResetToken.Create();
        await using (var context = database.CreateDbContext())
        {
            var employee = await context.Employees.SingleAsync(TestContext.Current.CancellationToken);
            employee.PasswordResetTokenHash = hash;
            employee.PasswordResetTokenExpiresAt = time.GetUtcNow().UtcDateTime.AddMinutes(30);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        time.Advance(TimeSpan.FromHours(2));

        Assert.False(await service.ResetPasswordWithTokenAsync(token, "Zu-Spaet-2026", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ResetPasswordWithTokenAsync_MitUnbekanntemToken_WirdAbgelehnt()
    {
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        await SeedEmployeeAsync(database);
        var (service, _, _) = CreateService(database);

        Assert.False(await service.ResetPasswordWithTokenAsync("erfundenes-token", "Neu-2026", TestContext.Current.CancellationToken));
    }
}
