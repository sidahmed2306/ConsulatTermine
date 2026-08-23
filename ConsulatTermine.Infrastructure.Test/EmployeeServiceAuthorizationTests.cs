using ConsulatTermine.Application.Configuration;
using ConsulatTermine.Application.DTOs;
using ConsulatTermine.Application.Exceptions;
using ConsulatTermine.Application.Interfaces;
using ConsulatTermine.Application.Security;
using ConsulatTermine.Domain.Entities;
using ConsulatTermine.Domain.Enums;
using ConsulatTermine.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace ConsulatTermine.Infrastructure.Test;

/// <summary>
/// Prueft, dass die Mitarbeiterverwaltung serverseitig autorisiert.
///
/// Die Aufrufe gehen bewusst direkt an den Anwendungsfall und nicht ueber die UI:
/// harness/security.md Abschnitt 4 verlangt, dass ein direkter serverseitiger Aufruf
/// die Pruefungen der Oberflaeche nicht umgehen kann.
/// </summary>
public sealed class EmployeeServiceAuthorizationTests
{
    private static EmployeeService CreateService(TestDatabase database, CurrentEmployee? current)
    {
        var authorization = Substitute.For<IEmployeeAuthorization>();
        authorization.GetCurrentEmployeeAsync().Returns(Task.FromResult(current));

        authorization.RequireEmployeeAsync().Returns(_ => current is null
            ? throw new NotAuthorizedException()
            : Task.FromResult(current));

        authorization.RequireMinimumRoleAsync(Arg.Any<EmployeeRole>()).Returns(call =>
        {
            var minimum = call.Arg<EmployeeRole>();
            if (current is null || current.Role < minimum)
            {
                throw new NotAuthorizedException();
            }

            return Task.FromResult(current);
        });

        return new EmployeeService(
            database,
            Substitute.For<IEmailService>(),
            authorization,
            new PasswordHasher<Employee>(),
            Options.Create(new ApplicationOptions { BaseUrl = "https://termine.example.org" }),
            new FakeTimeProvider(new DateTimeOffset(2026, 8, 23, 10, 0, 0, TimeSpan.Zero)),
            NullLogger<EmployeeService>.Instance);
    }

    private static CurrentEmployee Employee(int id = 1) => new(id, "CDZ-001", EmployeeRole.Employee);

    private static CurrentEmployee ServiceChef(int id = 2) => new(id, "CDZ-002", EmployeeRole.ServiceChef);

    private static CurrentEmployee Admin(int id = 3) => new(id, "CDZ-003", EmployeeRole.Admin);

    private static EmployeeDto NewEmployeeDto(EmployeeRole role = EmployeeRole.Employee) => new()
    {
        FirstName = "Karim",
        LastName = "Haddad",
        Email = "karim.haddad@example.org",
        Role = role
    };

    private static async Task<Employee> SeedAsync(
        TestDatabase database,
        EmployeeRole role,
        string code,
        bool isActive = true)
    {
        var employee = new Employee
        {
            EmployeeCode = code,
            FirstName = "Test",
            LastName = code,
            Email = $"{code.ToLowerInvariant()}@example.org",
            Role = role,
            IsActive = isActive
        };

        await using var context = database.CreateDbContext();
        context.Employees.Add(employee);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        return employee;
    }

    [Fact]
    public async Task GetAllEmployeesAsync_OhneAnmeldung_WirdAbgelehnt()
    {
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        var service = CreateService(database, current: null);

        await Assert.ThrowsAsync<NotAuthorizedException>(() => service.GetAllEmployeesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetAllEmployeesAsync_AlsEinfacherMitarbeiter_WirdAbgelehnt()
    {
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        var service = CreateService(database, Employee());

        await Assert.ThrowsAsync<NotAuthorizedException>(() => service.GetAllEmployeesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetAllEmployeesAsync_AlsServiceChef_IstErlaubt()
    {
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        await SeedAsync(database, EmployeeRole.Employee, "CDZ-010");
        var service = CreateService(database, ServiceChef());

        Assert.Single(await service.GetAllEmployeesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetEmployeeByIdAsync_AlsEinfacherMitarbeiter_LiefertNurDenEigenenDatensatz()
    {
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        var own = await SeedAsync(database, EmployeeRole.Employee, "CDZ-010");
        var other = await SeedAsync(database, EmployeeRole.Employee, "CDZ-011");

        var service = CreateService(database, new CurrentEmployee(own.Id, own.EmployeeCode, EmployeeRole.Employee));

        Assert.NotNull(await service.GetEmployeeByIdAsync(own.Id, TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<NotAuthorizedException>(() => service.GetEmployeeByIdAsync(other.Id, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CreateEmployeeAsync_AlsEinfacherMitarbeiter_WirdAbgelehnt()
    {
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        var service = CreateService(database, Employee());

        await Assert.ThrowsAsync<NotAuthorizedException>(() => service.CreateEmployeeAsync(NewEmployeeDto(), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CreateEmployeeAsync_SpeichertNiemalsEinKlartextPasswort()
    {
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        var service = CreateService(database, ServiceChef());

        await service.CreateEmployeeAsync(NewEmployeeDto(), TestContext.Current.CancellationToken);

        await using var context = database.CreateDbContext();
        var created = await context.Employees.SingleAsync(TestContext.Current.CancellationToken);

        Assert.NotNull(created.PasswordHash);
        Assert.True(created.MustChangePassword);
        Assert.StartsWith("CDZ-", created.EmployeeCode, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CreateEmployeeAsync_VergibtFortlaufendeKennungen()
    {
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        var service = CreateService(database, ServiceChef());

        var first = await service.CreateEmployeeAsync(NewEmployeeDto(), TestContext.Current.CancellationToken);
        var second = await service.CreateEmployeeAsync(new EmployeeDto
        {
            FirstName = "Nadia",
            LastName = "Cherif",
            Email = "nadia.cherif@example.org",
            Role = EmployeeRole.Employee
        }, TestContext.Current.CancellationToken);

        Assert.Equal("CDZ-001", first.EmployeeCode);
        Assert.Equal("CDZ-002", second.EmployeeCode);
    }

    [Fact]
    public async Task CreateEmployeeAsync_MitBereitsVergebenerEmail_WirdFachlichAbgewiesen()
    {
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        var service = CreateService(database, ServiceChef());

        await service.CreateEmployeeAsync(NewEmployeeDto(), TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => service.CreateEmployeeAsync(NewEmployeeDto(), TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateEmployeeAsync_OhneVornamen_WirdFachlichAbgewiesen(string firstName)
    {
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        var service = CreateService(database, ServiceChef());

        var dto = NewEmployeeDto();
        dto.FirstName = firstName;

        await Assert.ThrowsAsync<BusinessRuleViolationException>(() => service.CreateEmployeeAsync(dto, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task UpdateEmployeeAsync_AlsServiceChef_DarfDieRolleNichtAendern()
    {
        // Sonst koennte sich ein ServiceChef selbst oder andere zum Administrator machen.
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        var target = await SeedAsync(database, EmployeeRole.Employee, "CDZ-010");
        var service = CreateService(database, ServiceChef());

        var dto = NewEmployeeDto(EmployeeRole.Admin);

        await Assert.ThrowsAsync<NotAuthorizedException>(() => service.UpdateEmployeeAsync(target.Id, dto, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task UpdateEmployeeAsync_AlsServiceChef_DarfStammdatenAendern()
    {
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        var target = await SeedAsync(database, EmployeeRole.Employee, "CDZ-010");
        var service = CreateService(database, ServiceChef());

        var updated = await service.UpdateEmployeeAsync(target.Id, NewEmployeeDto(), TestContext.Current.CancellationToken);

        Assert.Equal("Karim", updated.FirstName);
        Assert.Equal("CDZ-010", updated.EmployeeCode);
    }

    [Fact]
    public async Task UpdateEmployeeAsync_AlsAdmin_DarfDieRolleAendern()
    {
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        var target = await SeedAsync(database, EmployeeRole.Employee, "CDZ-010");
        var service = CreateService(database, Admin());

        var updated = await service.UpdateEmployeeAsync(target.Id, NewEmployeeDto(EmployeeRole.ServiceChef), TestContext.Current.CancellationToken);

        Assert.Equal(EmployeeRole.ServiceChef, updated.Role);
    }

    [Fact]
    public async Task DeleteEmployeeAsync_AlsServiceChef_WirdAbgelehnt()
    {
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        var target = await SeedAsync(database, EmployeeRole.Employee, "CDZ-010");
        var service = CreateService(database, ServiceChef());

        await Assert.ThrowsAsync<NotAuthorizedException>(() => service.DeleteEmployeeAsync(target.Id, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task DeleteEmployeeAsync_DasEigeneKonto_WirdAbgelehnt()
    {
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        var admin = await SeedAsync(database, EmployeeRole.Admin, "CDZ-003");
        var service = CreateService(database, new CurrentEmployee(admin.Id, admin.EmployeeCode, EmployeeRole.Admin));

        await Assert.ThrowsAsync<BusinessRuleViolationException>(() => service.DeleteEmployeeAsync(admin.Id, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task DeleteEmployeeAsync_DenLetztenAdministrator_WirdAbgelehnt()
    {
        // Sonst waere die Anwendung nicht mehr verwaltbar.
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        var onlyAdmin = await SeedAsync(database, EmployeeRole.Admin, "CDZ-003");
        var otherAdminAccount = await SeedAsync(database, EmployeeRole.Admin, "CDZ-004", isActive: false);

        var service = CreateService(database, new CurrentEmployee(99, "CDZ-099", EmployeeRole.Admin));

        // Der zweite Administrator ist deaktiviert und zaehlt nicht.
        await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => service.DeleteEmployeeAsync(onlyAdmin.Id, TestContext.Current.CancellationToken));

        _ = otherAdminAccount;
    }

    [Fact]
    public async Task DeleteEmployeeAsync_AlsAdmin_EntferntEinenAnderenMitarbeiter()
    {
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        var target = await SeedAsync(database, EmployeeRole.Employee, "CDZ-010");
        var service = CreateService(database, Admin());

        Assert.True(await service.DeleteEmployeeAsync(target.Id, TestContext.Current.CancellationToken));

        await using var context = database.CreateDbContext();
        Assert.Empty(context.Employees);
    }

    [Fact]
    public async Task DeleteEmployeeAsync_MitUnbekannterId_LiefertFalse()
    {
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        var service = CreateService(database, Admin());

        Assert.False(await service.DeleteEmployeeAsync(4711, TestContext.Current.CancellationToken));
    }
}
