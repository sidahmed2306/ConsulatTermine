using System.Globalization;
using ConsulatTermine.Application.Configuration;
using ConsulatTermine.Application.DTOs;
using ConsulatTermine.Application.Interfaces;
using ConsulatTermine.Application.Localization;
using ConsulatTermine.Application.Resources;
using ConsulatTermine.Domain.Entities;
using ConsulatTermine.Infrastructure.Persistence;
using ConsulatTermine.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConsulatTermine.Infrastructure.Services;

/// <summary>
/// Anmeldung, Passwortwechsel und Passwort-Zuruecksetzung fuer Mitarbeiter.
/// </summary>
public sealed class EmployeeAuthService : IEmployeeAuthService
{
    /// <summary>
    /// Einheitliche Meldung fuer jeden fehlgeschlagenen Anmeldeversuch. Sie unterscheidet
    /// bewusst nicht zwischen unbekannter Kennung, falschem Passwort und deaktiviertem Konto,
    /// damit sich ueber die Anmeldemaske keine gueltigen Kennungen ermitteln lassen.
    /// Siehe harness/security.md Abschnitt 3.
    /// </summary>
    private static string GenericLoginError => BusinessMessages.Get("LoginFailed");

    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly IEmailService _emailService;
    private readonly IPasswordHasher<Employee> _passwordHasher;
    private readonly ApplicationOptions _applicationOptions;
    private readonly EmployeeLoginOptions _loginOptions;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<EmployeeAuthService> _logger;

    public EmployeeAuthService(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        IEmailService emailService,
        IPasswordHasher<Employee> passwordHasher,
        IOptions<ApplicationOptions> applicationOptions,
        IOptions<EmployeeLoginOptions> loginOptions,
        TimeProvider timeProvider,
        ILogger<EmployeeAuthService> logger)
    {
        _contextFactory = contextFactory;
        _emailService = emailService;
        _passwordHasher = passwordHasher;
        _applicationOptions = applicationOptions.Value;
        _loginOptions = loginOptions.Value;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<EmployeeLoginResultDto> LoginAsync(
        string employeeCode,
        string password,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var normalizedCode = employeeCode?.Trim() ?? string.Empty;

        var employee = await context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeCode == normalizedCode, cancellationToken);

        var now = _timeProvider.GetUtcNow().UtcDateTime;

        if (employee is null || !employee.IsActive || employee.PasswordHash is null)
        {
            // Kein fruehes Return ohne Aufwand: der Vergleich laeuft trotzdem gegen einen
            // Dummy-Hash, damit die Antwortzeit keinen Rueckschluss auf die Existenz des
            // Kontos erlaubt.
            _passwordHasher.VerifyHashedPassword(new Employee(), _dummyHash, password ?? string.Empty);
            ServiceLog.LoginRejectedUnknownAccount(_logger);
            return Failed();
        }

        if (employee.LockoutEndsAt is { } lockoutEnd && lockoutEnd > now)
        {
            ServiceLog.LoginRejectedLockedOut(_logger, employee.Id, lockoutEnd);
            return Failed();
        }

        var verification = _passwordHasher.VerifyHashedPassword(
            employee,
            employee.PasswordHash,
            password ?? string.Empty);

        if (verification == PasswordVerificationResult.Failed)
        {
            await RegisterFailedAttemptAsync(context, employee, now, cancellationToken);
            return Failed();
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            // Der gespeicherte Hash stammt aus einem aelteren Verfahren und wird bei
            // dieser Gelegenheit auf das aktuelle Format gehoben.
            employee.PasswordHash = _passwordHasher.HashPassword(employee, password!);
        }

        employee.FailedLoginAttempts = 0;
        employee.LockoutEndsAt = null;
        await context.SaveChangesAsync(cancellationToken);

        ServiceLog.LoginSucceeded(_logger, employee.Id);

        return new EmployeeLoginResultDto
        {
            Success = true,
            EmployeeId = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            Role = employee.Role,
            MustChangePassword = employee.MustChangePassword
        };
    }

    public async Task<bool> ChangePasswordAsync(
        int employeeId,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var employee = await context.Employees.FindAsync([employeeId], cancellationToken);
        if (employee is null)
        {
            return false;
        }

        ApplyNewPassword(employee, newPassword);
        await context.SaveChangesAsync(cancellationToken);

        ServiceLog.PasswordChanged(_logger, employee.Id);

        await SendPasswordChangedMailAsync(employee);
        return true;
    }

    public async Task<bool> RequestPasswordResetAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        // Der Rueckgabewert ist bewusst immer true: er darf nicht verraten, ob die
        // E-Mail-Adresse im System bekannt ist.
        if (string.IsNullOrWhiteSpace(email))
        {
            return true;
        }

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var normalizedEmail = email.Trim();

        var employee = await context.Employees
            .FirstOrDefaultAsync(e => e.Email == normalizedEmail && e.IsActive, cancellationToken);

        if (employee is null)
        {
            ServiceLog.PasswordResetRequestedForUnknownAddress(_logger);
            return true;
        }

        var (token, hash) = PasswordResetToken.Create();

        employee.PasswordResetTokenHash = hash;
        employee.PasswordResetTokenExpiresAt =
            _timeProvider.GetUtcNow().UtcDateTime + _loginOptions.PasswordResetTokenLifetime;

        await context.SaveChangesAsync(cancellationToken);

        var resetLink = BuildLink($"employee/reset-password?token={Uri.EscapeDataString(token)}");

        await _emailService.SendEmployeePasswordResetEmailAsync(
            employee.Email,
            FullNameOf(employee),
            resetLink,
            CurrentLanguage());

        ServiceLog.PasswordResetRequested(_logger, employee.Id);
        return true;
    }

    public async Task<bool> ResetPasswordWithTokenAsync(
        string token,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(newPassword))
        {
            return false;
        }

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var tokenHash = PasswordResetToken.Hash(token);
        var now = _timeProvider.GetUtcNow().UtcDateTime;

        var employee = await context.Employees
            .FirstOrDefaultAsync(
                e => e.PasswordResetTokenHash == tokenHash
                     && e.PasswordResetTokenExpiresAt != null
                     && e.PasswordResetTokenExpiresAt > now,
                cancellationToken);

        if (employee is null)
        {
            ServiceLog.PasswordResetTokenRejected(_logger);
            return false;
        }

        ApplyNewPassword(employee, newPassword);
        await context.SaveChangesAsync(cancellationToken);

        ServiceLog.PasswordReset(_logger, employee.Id);

        await SendPasswordChangedMailAsync(employee);
        return true;
    }

    /// <summary>
    /// Setzt ein neues Passwort und entwertet gleichzeitig einen offenen Reset-Link
    /// sowie eine bestehende Sperre.
    /// </summary>
    private void ApplyNewPassword(Employee employee, string newPassword)
    {
        employee.PasswordHash = _passwordHasher.HashPassword(employee, newPassword);
        employee.MustChangePassword = false;
        employee.PasswordResetTokenHash = null;
        employee.PasswordResetTokenExpiresAt = null;
        employee.FailedLoginAttempts = 0;
        employee.LockoutEndsAt = null;
    }

    private async Task RegisterFailedAttemptAsync(
        ApplicationDbContext context,
        Employee employee,
        DateTime now,
        CancellationToken cancellationToken)
    {
        employee.FailedLoginAttempts++;

        if (employee.FailedLoginAttempts >= _loginOptions.MaxFailedAttempts)
        {
            employee.LockoutEndsAt = now + _loginOptions.LockoutDuration;
            employee.FailedLoginAttempts = 0;

            ServiceLog.AccountLockedOut(_logger, employee.Id, employee.LockoutEndsAt);
        }
        else
        {
            ServiceLog.LoginAttemptFailed(_logger, employee.FailedLoginAttempts, _loginOptions.MaxFailedAttempts, employee.Id);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task SendPasswordChangedMailAsync(Employee employee)
    {
        await _emailService.SendEmployeePasswordChangedConfirmationEmailAsync(
            employee.Email,
            FullNameOf(employee),
            BuildLink("employee/login"),
            CurrentLanguage());
    }

    /// <summary>
    /// Sprache des Schriftverkehrs an Mitarbeiter.
    /// </summary>
    /// <remarks>
    /// Fuer ein Mitarbeiterkonto ist keine Sprachvorliebe hinterlegt. Bis das
    /// entschieden ist (siehe OPEN_DECISIONS Nummer 18), gilt die Sprache der
    /// Sitzung, die den Versand ausloest.
    /// </remarks>
    private static string CurrentLanguage() =>
        SupportedLanguages.Normalize(CultureInfo.CurrentUICulture.Name);

    private string BuildLink(string relativePath)
    {
        return $"{_applicationOptions.BaseUrl.TrimEnd('/')}/{relativePath}";
    }

    private static string FullNameOf(Employee employee)
    {
        return $"{employee.FirstName} {employee.LastName}";
    }

    private static EmployeeLoginResultDto Failed()
    {
        return new EmployeeLoginResultDto
        {
            Success = false,
            ErrorMessage = GenericLoginError
        };
    }

    /// <summary>
    /// Gueltiger Hash eines Zufallswertes. Dient nur dazu, bei unbekannter Kennung dieselbe
    /// Rechenzeit aufzuwenden wie bei einer bekannten.
    /// </summary>
    private static readonly string _dummyHash =
        new PasswordHasher<Employee>().HashPassword(new Employee(), "nicht-verwendetes-passwort");
}
