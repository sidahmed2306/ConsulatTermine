using ConsulatTermine.Application.Interfaces;
using ConsulatTermine.Domain.Entities;
using ConsulatTermine.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class EmployeeAssignmentService : IEmployeeAssignmentService
{
    private readonly ApplicationDbContext _db;

    public EmployeeAssignmentService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<EmployeeServiceAssignment>> GetAllAssignmentsAsync()
    {
        return await _db.EmployeeServiceAssignments
            .Include(a => a.Employee)
            .Include(a => a.Service)
            .ToListAsync();
    }

    public async Task<List<EmployeeServiceAssignment>> GetAssignmentsByEmployeeAsync(int employeeId)
    {
        return await _db.EmployeeServiceAssignments
            .Include(a => a.Service)
            .Where(a => a.EmployeeId == employeeId)
            .ToListAsync();
    }

    public async Task<bool> AddAssignmentAsync(int employeeId, int serviceId)
    {
        bool exists = await _db.EmployeeServiceAssignments
            .AnyAsync(a => a.EmployeeId == employeeId && a.ServiceId == serviceId);

        if (exists) return false;

        _db.EmployeeServiceAssignments.Add(new EmployeeServiceAssignment
        {
            EmployeeId = employeeId,
            ServiceId = serviceId
        });

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveAssignmentAsync(int employeeId, int serviceId)
    {
        var entity = await _db.EmployeeServiceAssignments
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.ServiceId == serviceId);

        if (entity == null) return false;

        _db.EmployeeServiceAssignments.Remove(entity);
        await _db.SaveChangesAsync();

        return true;
    }
}
