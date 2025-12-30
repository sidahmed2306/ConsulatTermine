using ConsulatTermine.Application.DTOs;
using ConsulatTermine.Application.Interfaces;
using ConsulatTermine.Domain.Entities;
using ConsulatTermine.Domain.Enums;
using ConsulatTermine.Infrastructure.Persistence;
using ConsulatTermine.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace ConsulatTermine.Infrastructure.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ApplicationDbContext _context;

       private readonly IEmailService _emailService;

public EmployeeService(
    ApplicationDbContext context,
    IEmailService emailService)
{
    _context = context;
    _emailService = emailService;
}

        // -------------------------------------------------------------
        // GET ALL EMPLOYEES (inkl. Service Assignments)
        // -------------------------------------------------------------
        public async Task<List<Employee>> GetAllEmployeesAsync()
        {
            return await _context.Employees
                .Include(e => e.AssignedServices)
                    .ThenInclude(a => a.Service)
                .AsNoTracking()
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();
        }

        // -------------------------------------------------------------
        // GET EMPLOYEE BY ID
        // -------------------------------------------------------------
        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            return await _context.Employees
                .Include(e => e.AssignedServices)
                    .ThenInclude(a => a.Service)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        // -------------------------------------------------------------
        // CREATE EMPLOYEE (mit Kennung: CDZ-001, CDZ-002, ...)
        // -------------------------------------------------------------
       public async Task<Employee> CreateEmployeeAsync(EmployeeDto dto)
{
    // -------------------------------------------------
    // 0) Basis-Validierung (fachlich, minimal)
    // -------------------------------------------------
    if (string.IsNullOrWhiteSpace(dto.FirstName))
        throw new Exception("FirstName is required.");

    if (string.IsNullOrWhiteSpace(dto.LastName))
        throw new Exception("LastName is required.");

    if (string.IsNullOrWhiteSpace(dto.Email))
        throw new Exception("Email is required.");

    // -------------------------------------------------
    // 1) Nächste Mitarbeiter-Kennung generieren
    // Format: CDZ-001, CDZ-002, ...
    // -------------------------------------------------
    var lastCode = await _context.Employees
        .AsNoTracking()
        .OrderByDescending(e => e.Id)
        .Select(e => e.EmployeeCode)
        .FirstOrDefaultAsync();

    int nextNumber = 1;

    if (!string.IsNullOrWhiteSpace(lastCode))
    {
        var parts = lastCode.Split('-');
        if (parts.Length == 2 && int.TryParse(parts[1], out var parsed))
        {
            nextNumber = parsed + 1;
        }
    }

    string employeeCode = $"CDZ-{nextNumber:D3}";

    // -------------------------------------------------
    // 2) Einmal-Passwort generieren
    // (temporär gespeichert, später durch Identity ersetzt)
    // -------------------------------------------------
    string temporaryPassword = TemporaryPasswordGenerator.Generate();

    // -------------------------------------------------
    // 3) Employee Entity erstellen
    // -------------------------------------------------
    var employee = new Employee
    {
        EmployeeCode = employeeCode,

        FirstName = dto.FirstName.Trim(),
        LastName = dto.LastName.Trim(),
        Email = dto.Email.Trim(),
        DateOfBirth = dto.DateOfBirth,
        Role = dto.Role,

        // Status
        IsActive = true,

        // Sicherheit
        TemporaryPassword = temporaryPassword,
        MustChangePassword = true,

        // Vorbereitung für späteres Identity-Mapping
        IdentityUserId = null,

        // Meta
        CreatedAt = DateTime.UtcNow
    };

    // -------------------------------------------------
    // 4) Persistieren
    // -------------------------------------------------
   _context.Employees.Add(employee);
await _context.SaveChangesAsync();

// -------------------------------------------------
// BLOCK A3 – Willkommens-E-Mail (E-Mail 1)
// -------------------------------------------------

var changePasswordLink =
    $"http://localhost:5262/employee/change-password/{employee.Id}";

await _emailService.SendEmployeeWelcomeEmailAsync(
    toEmail: employee.Email,
    fullName: $"{employee.FirstName} {employee.LastName}",
    employeeCode: employee.EmployeeCode,
    temporaryPassword: temporaryPassword,
    changePasswordLink: changePasswordLink
);

return employee;

}


        // -------------------------------------------------------------
        // UPDATE EMPLOYEE
        // -------------------------------------------------------------
        public async Task<Employee> UpdateEmployeeAsync(int id, EmployeeDto dto)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
                throw new Exception("Employee not found");

            employee.FirstName = dto.FirstName;
            employee.LastName = dto.LastName;
            employee.Email = dto.Email;
            employee.DateOfBirth = dto.DateOfBirth;

            // EmployeeCode wird NICHT geändert (systemseitige Kennung)
            await _context.SaveChangesAsync();
            return employee;
        }

        // -------------------------------------------------------------
        // DELETE EMPLOYEE
        // -------------------------------------------------------------
        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
                return false;

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return true;
        }

        // -------------------------------------------------------------
        // ASSIGN SERVICE TO EMPLOYEE
        // -------------------------------------------------------------
        public async Task AssignServiceAsync(int employeeId, int serviceId)
        {
            var exists = await _context.EmployeeServiceAssignments
                .AnyAsync(a => a.EmployeeId == employeeId && a.ServiceId == serviceId);

            if (!exists)
            {
                _context.EmployeeServiceAssignments.Add(new EmployeeServiceAssignment
                {
                    EmployeeId = employeeId,
                    ServiceId = serviceId
                });

                await _context.SaveChangesAsync();
            }
        }

        // -------------------------------------------------------------
        // REMOVE SERVICE FROM EMPLOYEE
        // -------------------------------------------------------------
        public async Task RemoveServiceAsync(int employeeId, int serviceId)
        {
            var assignment = await _context.EmployeeServiceAssignments
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.ServiceId == serviceId);

            if (assignment != null)
            {
                _context.EmployeeServiceAssignments.Remove(assignment);
                await _context.SaveChangesAsync();
            }
        }
 public async Task EnsureInitialAdminAsync()
{
    var adminExists = await _context.Employees
        .AnyAsync(e => e.Role == EmployeeRole.Admin);

    if (adminExists)
        return;

    var tempPassword = "Admin123!"; // später austauschbar

    var admin = new Employee
    {
        FirstName = "System",
        LastName = "Administrator",
        Email = "sidahmedbc1@gmail.com",
        EmployeeCode = "ADMIN-001",
        Role = EmployeeRole.Admin,

        TemporaryPassword = tempPassword,
        MustChangePassword = true,
        IsActive = true
    };

    _context.Employees.Add(admin);
    await _context.SaveChangesAsync();

    var changePasswordLink =
        "http://localhost:5262/employee/change-password/" + admin.Id;

    await _emailService.SendEmployeeWelcomeEmailAsync(
        toEmail: admin.Email,
        fullName: $"{admin.FirstName} {admin.LastName}",
        employeeCode: admin.EmployeeCode,
        temporaryPassword: tempPassword,
        changePasswordLink: changePasswordLink
    );
}




    }
}


