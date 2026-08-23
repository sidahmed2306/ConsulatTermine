using ConsulatTermine.Application.Exceptions;
using ConsulatTermine.Application.Interfaces;
using ConsulatTermine.Domain.Entities;
using ConsulatTermine.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConsulatTermine.Infrastructure.Services;

public class WorkingHoursService : IWorkingHoursService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public WorkingHoursService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<WorkingHours>> GetAllAsync()
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        return await db.WorkingHours.Include(w => w.Service).ToListAsync();
    }

    public async Task<List<WorkingHours>> GetByServiceAsync(int serviceId)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        return await db.WorkingHours
            .Where(w => w.ServiceId == serviceId)
            .Include(w => w.Service)
            .ToListAsync();
    }

    public async Task<WorkingHours?> GetByIdAsync(int id)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        return await db.WorkingHours.FindAsync(id);
    }

    public async Task<WorkingHours> CreateAsync(WorkingHours model)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        db.WorkingHours.Add(model);
        await db.SaveChangesAsync();
        return model;
    }

    public async Task<WorkingHours> UpdateAsync(int id, WorkingHours model)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var entity = await db.WorkingHours.FindAsync(id);
        if (entity == null)
        {
            throw new BusinessRuleViolationException("Die Arbeitszeit wurde nicht gefunden.");
        }

        entity.ServiceId = model.ServiceId;
        entity.Day = model.Day;
        entity.StartTime = model.StartTime;
        entity.EndTime = model.EndTime;

        await db.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var entity = await db.WorkingHours.FindAsync(id);
        if (entity == null)
        {
            return false;
        }

        db.WorkingHours.Remove(entity);
        await db.SaveChangesAsync();
        return true;
    }
}
