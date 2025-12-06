using ConsulatTermine.Application.Interfaces;
using ConsulatTermine.Domain.Entities;
using ConsulatTermine.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class WorkingHoursService : IWorkingHoursService
{
    private readonly ApplicationDbContext _db;

    public WorkingHoursService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<WorkingHours>> GetAllAsync()
    {
        return await _db.WorkingHours.Include(w => w.Service).ToListAsync();
    }

    public async Task<List<WorkingHours>> GetByServiceAsync(int serviceId)
    {
        return await _db.WorkingHours
            .Where(w => w.ServiceId == serviceId)
            .Include(w => w.Service)
            .ToListAsync();
    }

    public async Task<WorkingHours?> GetByIdAsync(int id)
    {
        return await _db.WorkingHours.FindAsync(id);
    }

    public async Task<WorkingHours> CreateAsync(WorkingHours model)
    {
        _db.WorkingHours.Add(model);
        await _db.SaveChangesAsync();
        return model;
    }

    public async Task<WorkingHours> UpdateAsync(int id, WorkingHours model)
    {
        var entity = await _db.WorkingHours.FindAsync(id);
        if (entity == null) throw new Exception("WorkingHours not found.");

        entity.ServiceId = model.ServiceId;
        entity.Day = model.Day;
        entity.StartTime = model.StartTime;
        entity.EndTime = model.EndTime;

        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.WorkingHours.FindAsync(id);
        if (entity == null) return false;

        _db.WorkingHours.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }
}
