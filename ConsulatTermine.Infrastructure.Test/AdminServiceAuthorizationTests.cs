using ConsulatTermine.Application.DTOs;
using ConsulatTermine.Application.Exceptions;
using ConsulatTermine.Application.Security;
using ConsulatTermine.Domain.Enums;
using ConsulatTermine.Infrastructure.Services;
using NSubstitute;

namespace ConsulatTermine.Infrastructure.Test;

/// <summary>
/// Prueft, dass die zustandsaendernden Anwendungsfaelle der Verwaltung serverseitig
/// autorisiert werden, auch wenn sie unter Umgehung der Oberflaeche aufgerufen werden.
///
/// Lesende Methoden sind bewusst nicht autorisiert: die oeffentliche Terminbuchung
/// liest Services, Arbeitszeiten und Ausnahmen ohne Anmeldung.
/// </summary>
public sealed class AdminServiceAuthorizationTests
{
    private static IEmployeeAuthorization AuthorizationFor(CurrentEmployee? current)
    {
        var authorization = Substitute.For<IEmployeeAuthorization>();

        authorization.RequireMinimumRoleAsync(Arg.Any<EmployeeRole>()).Returns(call =>
        {
            var minimum = call.Arg<EmployeeRole>();
            if (current is null || current.Role < minimum)
            {
                throw new NotAuthorizedException();
            }

            return Task.FromResult(current);
        });

        return authorization;
    }

    private static CurrentEmployee AsRole(EmployeeRole role) => new(1, "CDZ-001", role);

    [Theory]
    [InlineData(null)]
    [InlineData(EmployeeRole.Employee)]
    [InlineData(EmployeeRole.ServiceChef)]
    public async Task ServiceService_AenderungenSindAdministratorenVorbehalten(EmployeeRole? role)
    {
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        var service = new ServiceService(
            database,
            AuthorizationFor(role is null ? null : AsRole(role.Value)));

        var dto = new ServiceDto { Name = "Pass", SlotDurationMinutes = 30 };

        await Assert.ThrowsAsync<NotAuthorizedException>(() => service.CreateServiceAsync(dto));
        await Assert.ThrowsAsync<NotAuthorizedException>(() => service.UpdateServiceAsync(1, dto));
        await Assert.ThrowsAsync<NotAuthorizedException>(() => service.DeleteServiceAsync(1));
    }

    [Fact]
    public async Task ServiceService_LesenIstOhneAnmeldungMoeglich()
    {
        // Die oeffentliche Terminbuchung listet die angebotenen Services.
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        var service = new ServiceService(database, AuthorizationFor(current: null));

        Assert.Empty(await service.GetAllServicesAsync());
    }

    [Theory]
    [InlineData(null)]
    [InlineData(EmployeeRole.Employee)]
    public async Task EmployeeAssignmentService_ZuweisungenErfordernMindestensServiceChef(EmployeeRole? role)
    {
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        var service = new EmployeeAssignmentService(
            database,
            AuthorizationFor(role is null ? null : AsRole(role.Value)));

        await Assert.ThrowsAsync<NotAuthorizedException>(() => service.AddAssignmentAsync(1, 1));
        await Assert.ThrowsAsync<NotAuthorizedException>(() => service.RemoveAssignmentAsync(1, 1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(EmployeeRole.Employee)]
    public async Task WorkingHoursService_AenderungenErfordernMindestensServiceChef(EmployeeRole? role)
    {
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        var service = new WorkingHoursService(
            database,
            AuthorizationFor(role is null ? null : AsRole(role.Value)));

        await Assert.ThrowsAsync<NotAuthorizedException>(() => service.DeleteAsync(1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(EmployeeRole.Employee)]
    public async Task ServiceDayOverrideService_AenderungenErfordernMindestensServiceChef(EmployeeRole? role)
    {
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        var service = new ServiceDayOverrideService(
            database,
            AuthorizationFor(role is null ? null : AsRole(role.Value)));

        await Assert.ThrowsAsync<NotAuthorizedException>(() => service.DeleteAsync(1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(EmployeeRole.Employee)]
    public async Task WorkingSchedulePlanService_AenderungenErfordernMindestensServiceChef(EmployeeRole? role)
    {
        await using var database = await TestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        var service = new WorkingSchedulePlanService(
            database,
            AuthorizationFor(role is null ? null : AsRole(role.Value)));

        await Assert.ThrowsAsync<NotAuthorizedException>(() => service.DeleteAsync(1));
        await Assert.ThrowsAsync<NotAuthorizedException>(() => service.SetActiveAsync(1));
    }
}
