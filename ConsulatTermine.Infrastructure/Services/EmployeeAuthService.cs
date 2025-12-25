using ConsulatTermine.Application.DTOs;
using ConsulatTermine.Application.Interfaces;
using ConsulatTermine.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConsulatTermine.Infrastructure.Services;

public class EmployeeAuthService : IEmployeeAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public EmployeeAuthService(ApplicationDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
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
            return false;

        employee.TemporaryPassword = newPassword; // später: Hash
        employee.MustChangePassword = false;

        await _context.SaveChangesAsync();

        // -------------------------------------------------
        // BLOCK A4 – E-Mail 2: Passwort geändert + Login-Link
        // -------------------------------------------------
        var loginLink = "http://localhost:5262/employee/login";

        await _emailService.SendEmployeePasswordChangedConfirmationEmailAsync(
            toEmail: employee.Email,
            fullName: $"{employee.FirstName} {employee.LastName}",
            loginLink: loginLink
        );

        return true;
    }
}
