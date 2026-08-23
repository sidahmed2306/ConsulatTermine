using ConsulatTermine.Application.Exceptions;
using ConsulatTermine.Application.Interfaces;
using ConsulatTermine.Domain.Entities;
using ConsulatTermine.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConsulatTermine.Infrastructure.Services;

public class EmployeeAssignmentService : IEmployeeAssignmentService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public EmployeeAssignmentService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<EmployeeServiceAssignment>> GetAllAssignmentsAsync()
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        return await db.EmployeeServiceAssignments
            .Include(a => a.Employee)
            .Include(a => a.Service)
            .ToListAsync();
    }

    public async Task<List<EmployeeServiceAssignment>> GetAssignmentsByEmployeeAsync(int employeeId)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        return await db.EmployeeServiceAssignments
            .Include(a => a.Service)
            .Where(a => a.EmployeeId == employeeId)
            .ToListAsync();
    }

    public async Task<bool> AddAssignmentAsync(int employeeId, int serviceId)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        bool exists = await db.EmployeeServiceAssignments
            .AnyAsync(a => a.EmployeeId == employeeId && a.ServiceId == serviceId);

        if (exists)
        {
            return false;
        }

        db.EmployeeServiceAssignments.Add(new EmployeeServiceAssignment
        {
            EmployeeId = employeeId,
            ServiceId = serviceId
        });

        var service = await db.Services
            .Include(s => s.AssignedEmployees)
            .FirstOrDefaultAsync(s => s.Id == serviceId);

        if (service == null)
        {
            throw new BusinessRuleViolationException("Service nicht gefunden");
        }

        // EINZIGE Stelle, die CapacityPerSlot ändert
        service.CapacityPerSlot = service.AssignedEmployees.Count;

        await db.SaveChangesAsync();
        return true;
    }


    public async Task<bool> RemoveAssignmentAsync(int employeeId, int serviceId)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var assignment = await db.EmployeeServiceAssignments
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.ServiceId == serviceId);

        if (assignment == null)
        {
            return false;
        }

        db.EmployeeServiceAssignments.Remove(assignment);

        var service = await db.Services
            .Include(s => s.AssignedEmployees)
            .FirstOrDefaultAsync(s => s.Id == serviceId);

        if (service == null)
        {
            throw new BusinessRuleViolationException("Service nicht gefunden");
        }

        service.CapacityPerSlot = Math.Max(0, service.AssignedEmployees.Count - 1);

        await db.SaveChangesAsync();
        return true;
    }

}
