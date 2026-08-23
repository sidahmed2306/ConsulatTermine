using ConsulatTermine.Application.DTOs;
using ConsulatTermine.Application.Interfaces;
using ConsulatTermine.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ConsulatTermine.Infrastructure.Services;

public class EmployeeAuthService : IEmployeeAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public EmployeeAuthService(
        ApplicationDbContext context,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _context = context;
        _emailService = emailService;
        _configuration = configuration;
    }

    public async Task<EmployeeLoginResultDto> LoginAsync(string employeeCode, string password)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeCode == employeeCode);

        if (employee == null || !employee.IsActive)
        {
            return new EmployeeLoginResultDto
            {
                Success = false,
                ErrorMessage = "Ungültige Kennung oder Benutzer deaktiviert"
            };
        }

        // Temporäre Passwortprüfung (Übergangsphase)
        if (employee.TemporaryPassword != password)
        {
            return new EmployeeLoginResultDto
            {
                Success = false,
                ErrorMessage = "Ungültiges Passwort"
            };
        }

        return new EmployeeLoginResultDto
        {
            Success = true,
            EmployeeId = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            MustChangePassword = employee.MustChangePassword
        };
    }

    public async Task<bool> ChangePasswordAsync(int employeeId, string newPassword)
    {
        var employee = await _context.Employees.FindAsync(employeeId);
        if (employee == null)
        {
            return false;
        }

        employee.TemporaryPassword = newPassword; // später: Hash
        employee.MustChangePassword = false;

        await _context.SaveChangesAsync();

        // -------------------------------------------------
        // BLOCK A4 – E-Mail 2: Passwort geändert + Login-Link
        // -------------------------------------------------
        var baseUrl = _configuration["AppBaseUrl"] ?? "http://localhost:5262";
        var loginLink = $"{baseUrl.TrimEnd('/')}/employee/login";

        await _emailService.SendEmployeePasswordChangedConfirmationEmailAsync(
            toEmail: employee.Email,
            fullName: $"{employee.FirstName} {employee.LastName}",
            loginLink: loginLink
        );

        return true;
    }

    public async Task<bool> RequestPasswordResetAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return true; // Aus Sicherheitsgründen immer Erfolg melden
        }

        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Email == email.Trim() && e.IsActive);

        if (employee == null)
        {
            return true; // Keine Hinweise geben, ob E-Mail existiert
        }

        var token = Guid.NewGuid().ToString("N");
        employee.PasswordResetToken = token;
        employee.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(1);

        await _context.SaveChangesAsync();

        var baseUrl = _configuration["AppBaseUrl"] ?? "http://localhost:5262";
        var resetLink = $"{baseUrl.TrimEnd('/')}/employee/reset-password?token={token}";

        await _emailService.SendEmployeePasswordResetEmailAsync(
            employee.Email,
            $"{employee.FirstName} {employee.LastName}",
            resetLink);

        return true;
    }

    public async Task<bool> ResetPasswordWithTokenAsync(string token, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(newPassword))
        {
            return false;
        }

        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.PasswordResetToken == token);

        if (employee == null ||
            employee.PasswordResetTokenExpiresAt == null ||
            employee.PasswordResetTokenExpiresAt < DateTime.UtcNow)
        {
            return false;
        }

        employee.TemporaryPassword = newPassword;
        employee.MustChangePassword = false;
        employee.PasswordResetToken = null;
        employee.PasswordResetTokenExpiresAt = null;

        await _context.SaveChangesAsync();

        var baseUrl = _configuration["AppBaseUrl"] ?? "http://localhost:5262";
        var loginLink = $"{baseUrl.TrimEnd('/')}/employee/login";

        await _emailService.SendEmployeePasswordChangedConfirmationEmailAsync(
            employee.Email,
            $"{employee.FirstName} {employee.LastName}",
            loginLink);

        return true;
    }
}
