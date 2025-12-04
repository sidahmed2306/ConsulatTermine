using ConsulatTermine.Application.Interfaces;
using ConsulatTermine.Application.DTOs;
using ConsulatTermine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using ConsulatTermine.Infrastructure.Persistence;

namespace ConsulatTermine.Infrastructure.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ApplicationDbContext _context;

        public EmployeeService(ApplicationDbContext context)
        {
            _context = context;
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
                .OrderBy(e => e.FullName)
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
        // CREATE EMPLOYEE
        // -------------------------------------------------------------
        public async Task<Employee> CreateEmployeeAsync(EmployeeDto dto)
        {
            var employee = new Employee
            {
                FullName = dto.FullName,
                Email = dto.Email,
                IdentityUserId = null
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

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

            employee.FullName = dto.FullName;
            employee.Email = dto.Email;

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
    }
}
