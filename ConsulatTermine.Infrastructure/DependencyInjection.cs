using ConsulatTermine.Application.Interfaces;
using ConsulatTermine.Application.Interfaces.Booking;
using ConsulatTermine.Domain.Entities;
using ConsulatTermine.Infrastructure.Persistence;
using ConsulatTermine.Infrastructure.Services;
using ConsulatTermine.Infrastructure.Services.Booking;
using ConsulatTermine.Infrastructure.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ConsulatTermine.Infrastructure;

/// <summary>
/// Registriert die Infrastruktur im DI-Container.
/// Die UI kennt dadurch nur diese eine Methode und keine konkreten Implementierungen;
/// fachliche Aufrufe laufen ausschliesslich ueber die Interfaces der Application-Schicht.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        // Blazor Server verwendet IDbContextFactory statt eines Scoped DbContext:
        // ein Circuit lebt so lange wie die Registerkarte des Benutzers, und ein darin
        // gehaltener DbContext wuerde bei parallelen Komponenten-Renderings die Ausnahme
        // "A second operation was started on this context" ausloesen.
        // Siehe harness/profile.md Abschnitt 10.
        services.AddDbContextFactory<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        // PBKDF2 mit Salt und Iterationszahl aus dem ASP.NET-Core-Stack.
        // Eigene Passwortverfahren sind laut harness/security.md Abschnitt 1 unzulaessig.
        services.AddSingleton<IPasswordHasher<Employee>, PasswordHasher<Employee>>();

        services.AddSingleton(TimeProvider.System);

        services.AddScoped<IServiceDayOverrideService, ServiceDayOverrideService>();
        services.AddScoped<IServiceService, ServiceService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IEmployeeAssignmentService, EmployeeAssignmentService>();
        services.AddScoped<IWorkingHoursService, WorkingHoursService>();
        services.AddScoped<IWorkingScheduleService, WorkingScheduleService>();
        services.AddScoped<IWorkingScheduleOverviewService, WorkingScheduleOverviewService>();
        services.AddScoped<IWorkingSchedulePlanService, WorkingSchedulePlanService>();
        services.AddScoped<IAppointmentCalendarQueryService, AppointmentCalendarQueryService>();
        services.AddScoped<IEmployeeAuthService, EmployeeAuthService>();
        services.AddScoped<IEmailService, SmtpEmailService>();

        services.AddScoped<IBookingReferenceGenerator, BookingReferenceGenerator>();
        services.AddScoped<IBookingValidationService, BookingValidationService>();
        services.AddScoped<ISlotAvailabilityService, SlotAvailabilityService>();
        services.AddScoped<IBookingService, BookingService>();

        // Der Wartezimmer-Zustand ist bewusst prozessweit: alle Anzeigetafeln und
        // Mitarbeiterplaetze sehen denselben Aufrufstand.
        services.AddSingleton<IWaitingRoomNotifier, WaitingRoomNotifier>();

        return services;
    }
}
