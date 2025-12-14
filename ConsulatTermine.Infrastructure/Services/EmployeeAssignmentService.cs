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

    if (exists)
        return false;

    _db.EmployeeServiceAssignments.Add(new EmployeeServiceAssignment
    {
        EmployeeId = employeeId,
        ServiceId = serviceId
    });

    var service = await _db.Services
        .Include(s => s.AssignedEmployees)
        .FirstOrDefaultAsync(s => s.Id == serviceId);

    if (service == null)
        throw new Exception("Service nicht gefunden");

    // EINZIGE Stelle, die CapacityPerSlot ändert
    service.CapacityPerSlot = service.AssignedEmployees.Count;

    await _db.SaveChangesAsync();
    return true;
}


   public async Task<bool> RemoveAssignmentAsync(int employeeId, int serviceId)
{
    var assignment = await _db.EmployeeServiceAssignments
        .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.ServiceId == serviceId);

    if (assignment == null)
        return false;

    _db.EmployeeServiceAssignments.Remove(assignment);

    var service = await _db.Services
        .Include(s => s.AssignedEmployees)
        .FirstOrDefaultAsync(s => s.Id == serviceId);

    if (service == null)
        throw new Exception("Service nicht gefunden");

    service.CapacityPerSlot = Math.Max(0, service.AssignedEmployees.Count - 1);

    await _db.SaveChangesAsync();
    return true;
}

}
