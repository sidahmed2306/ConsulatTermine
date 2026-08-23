using ConsulatTermine.Application.Configuration;
using ConsulatTermine.Application.DTOs;
using ConsulatTermine.Application.Exceptions;
using ConsulatTermine.Application.Interfaces;
using ConsulatTermine.Application.Security;
using ConsulatTermine.Domain.Entities;
using ConsulatTermine.Domain.Enums;
using ConsulatTermine.Infrastructure.Persistence;
using ConsulatTermine.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConsulatTermine.Infrastructure.Services;

/// <summary>
/// Verwaltung der Mitarbeiterstammdaten.
/// Anlegen, Aendern und Loeschen sind ServiceChef und Admin vorbehalten; die Pruefung
/// erfolgt serverseitig und unabhaengig davon, was die UI anbietet.
/// </summary>
public sealed class EmployeeService : IEmployeeService
{
    /// <summary>Praefix der systemseitig vergebenen Mitarbeiterkennung.</summary>
    private const string EmployeeCodePrefix = "CDZ";

    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly IEmailService _emailService;
    private readonly IEmployeeAuthorization _authorization;
    private readonly IPasswordHasher<Employee> _passwordHasher;
    private readonly ApplicationOptions _applicationOptions;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        IEmailService emailService,
        IEmployeeAuthorization authorization,
        IPasswordHasher<Employee> passwordHasher,
        IOptions<ApplicationOptions> applicationOptions,
        TimeProvider timeProvider,
        ILogger<EmployeeService> logger)
    {
        _contextFactory = contextFactory;
        _emailService = emailService;
        _authorization = authorization;
        _passwordHasher = passwordHasher;
        _applicationOptions = applicationOptions.Value;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<List<Employee>> GetAllEmployeesAsync(CancellationToken cancellationToken = default)
    {
        await _authorization.RequireMinimumRoleAsync(EmployeeRole.ServiceChef);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Employees
            .AsNoTracking()
            .Include(e => e.AssignedServices)
                .ThenInclude(a => a.Service)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Employee?> GetEmployeeByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var current = await _authorization.RequireEmployeeAsync();

        // Ein einfacher Mitarbeiter darf nur den eigenen Datensatz lesen.
        if (current.Role < EmployeeRole.ServiceChef && current.EmployeeId != id)
        {
            throw new NotAuthorizedException();
        }

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.Employees
            .AsNoTracking()
            .Include(e => e.AssignedServices)
                .ThenInclude(a => a.Service)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Employee> CreateEmployeeAsync(
        EmployeeDto dto,
        CancellationToken cancellationToken = default)
    {
        await _authorization.RequireMinimumRoleAsync(EmployeeRole.ServiceChef);

        ArgumentNullException.ThrowIfNull(dto);

        if (string.IsNullOrWhiteSpace(dto.FirstName))
        {
            throw new BusinessRuleViolationException("Der Vorname ist erforderlich.");
        }

        if (string.IsNullOrWhiteSpace(dto.LastName))
        {
            throw new BusinessRuleViolationException("Der Nachname ist erforderlich.");
        }

        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            throw new BusinessRuleViolationException("Die E-Mail-Adresse ist erforderlich.");
        }

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var normalizedEmail = dto.Email.Trim();

        // EF Core uebersetzt den Vergleich in eine Abfrage mit der Collation der Spalte.
        // Die Datenbank ist case-insensitive konfiguriert; ToLower() waere hier nur ein
        // Aufruf, der jeden Index unbrauchbar macht.
        var emailExists = await context.Employees
            .AnyAsync(e => e.Email == normalizedEmail, cancellationToken);

        if (emailExists)
        {
            throw new BusinessRuleViolationException(
                "Ein Mitarbeiter mit dieser E-Mail-Adresse existiert bereits.");
        }

        var initialPassword = InitialPasswordGenerator.Generate();

        var employee = new Employee
        {
            EmployeeCode = await GenerateNextEmployeeCodeAsync(context, cancellationToken),
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = normalizedEmail,
            DateOfBirth = dto.DateOfBirth,
            Role = dto.Role,
            IsActive = true,
            MustChangePassword = true,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime
        };

        employee.PasswordHash = _passwordHasher.HashPassword(employee, initialPassword);

        context.Employees.Add(employee);
        await context.SaveChangesAsync(cancellationToken);

        ServiceLog.EmployeeCreated(_logger, employee.Id, employee.EmployeeCode);

        await _emailService.SendEmployeeWelcomeEmailAsync(
            toEmail: employee.Email,
            fullName: FullNameOf(employee),
            employeeCode: employee.EmployeeCode,
            temporaryPassword: initialPassword,
            changePasswordLink: BuildLink("employee/login"));

        return employee;
    }

    public async Task<Employee> UpdateEmployeeAsync(
        int id,
        EmployeeDto dto,
        CancellationToken cancellationToken = default)
    {
        var current = await _authorization.RequireMinimumRoleAsync(EmployeeRole.ServiceChef);

        ArgumentNullException.ThrowIfNull(dto);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var employee = await context.Employees.FindAsync([id], cancellationToken)
            ?? throw new BusinessRuleViolationException("Der Mitarbeiter wurde nicht gefunden.");

        // Nur ein Admin darf Rollen vergeben. Sonst koennte sich ein ServiceChef selbst
        // oder andere zum Admin befoerdern.
        if (employee.Role != dto.Role && current.Role < EmployeeRole.Admin)
        {
            throw new NotAuthorizedException("Nur ein Administrator darf Rollen aendern.");
        }

        employee.FirstName = dto.FirstName.Trim();
        employee.LastName = dto.LastName.Trim();
        employee.Email = dto.Email.Trim();
        employee.DateOfBirth = dto.DateOfBirth;
        employee.Role = dto.Role;

        // Die Kennung ist systemseitig vergeben und bleibt unveraendert.
        await context.SaveChangesAsync(cancellationToken);

        ServiceLog.EmployeeUpdated(_logger, employee.Id);
        return employee;
    }

    public async Task<bool> DeleteEmployeeAsync(int id, CancellationToken cancellationToken = default)
    {
        var current = await _authorization.RequireMinimumRoleAsync(EmployeeRole.Admin);

        if (current.EmployeeId == id)
        {
            throw new BusinessRuleViolationException(
                "Das eigene Konto kann nicht geloescht werden.");
        }

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var employee = await context.Employees.FindAsync([id], cancellationToken);
        if (employee is null)
        {
            return false;
        }

        // Der letzte aktive Administrator darf nicht verschwinden, sonst ist die
        // Anwendung nicht mehr verwaltbar.
        if (employee.Role == EmployeeRole.Admin)
        {
            var remainingAdmins = await context.Employees
                .CountAsync(e => e.Role == EmployeeRole.Admin && e.IsActive && e.Id != id, cancellationToken);

            if (remainingAdmins == 0)
            {
                throw new BusinessRuleViolationException(
                    "Der letzte Administrator kann nicht geloescht werden.");
            }
        }

        context.Employees.Remove(employee);
        await context.SaveChangesAsync(cancellationToken);

        ServiceLog.EmployeeDeleted(_logger, id);
        return true;
    }

    /// <summary>
    /// Legt beim ersten Start einen Administrator an, damit die Anwendung ueberhaupt
    /// bedienbar ist. Laeuft ohne angemeldeten Benutzer und wird deshalb nicht autorisiert;
    /// der Aufruf erfolgt ausschliesslich aus dem Anwendungsstart.
    /// </summary>
    public async Task EnsureInitialAdminAsync(
        string adminEmail,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(adminEmail);

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var adminExists = await context.Employees
            .AnyAsync(e => e.Role == EmployeeRole.Admin, cancellationToken);

        if (adminExists)
        {
            return;
        }

        var initialPassword = InitialPasswordGenerator.Generate();

        var admin = new Employee
        {
            FirstName = "System",
            LastName = "Administrator",
            Email = adminEmail.Trim(),
            EmployeeCode = $"{EmployeeCodePrefix}-001",
            Role = EmployeeRole.Admin,
            MustChangePassword = true,
            IsActive = true,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime
        };

        admin.PasswordHash = _passwordHasher.HashPassword(admin, initialPassword);

        context.Employees.Add(admin);
        await context.SaveChangesAsync(cancellationToken);

        ServiceLog.InitialAdminCreated(_logger, admin.EmployeeCode);

        await _emailService.SendEmployeeWelcomeEmailAsync(
            toEmail: admin.Email,
            fullName: FullNameOf(admin),
            employeeCode: admin.EmployeeCode,
            temporaryPassword: initialPassword,
            changePasswordLink: BuildLink("employee/login"));
    }

    /// <summary>
    /// Bildet die naechste freie Kennung im Format CDZ-001.
    /// Die Eindeutigkeit sichert zusaetzlich ein Unique-Index auf <c>EmployeeCode</c>:
    /// bei zwei gleichzeitigen Anlagen scheitert die zweite an der Datenbank statt eine
    /// doppelte Kennung zu vergeben.
    /// </summary>
    private static async Task<string> GenerateNextEmployeeCodeAsync(
        ApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        var codes = await context.Employees
            .AsNoTracking()
            .Where(e => e.EmployeeCode.StartsWith(EmployeeCodePrefix))
            .Select(e => e.EmployeeCode)
            .ToListAsync(cancellationToken);

        var highest = codes
            .Select(code => code.Split('-'))
            .Where(parts => parts.Length == 2 && int.TryParse(parts[1], out _))
            .Select(parts => int.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture))
            .DefaultIfEmpty(0)
            .Max();

        return $"{EmployeeCodePrefix}-{highest + 1:D3}";
    }

    private string BuildLink(string relativePath)
    {
        return $"{_applicationOptions.BaseUrl.TrimEnd('/')}/{relativePath}";
    }

    private static string FullNameOf(Employee employee)
    {
        return $"{employee.FirstName} {employee.LastName}";
    }
}
