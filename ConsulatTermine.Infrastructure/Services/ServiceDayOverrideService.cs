using ConsulatTermine.Application.Exceptions;
using ConsulatTermine.Application.Interfaces;
using ConsulatTermine.Application.Security;
using ConsulatTermine.Domain.Entities;
using ConsulatTermine.Domain.Enums;
using ConsulatTermine.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConsulatTermine.Infrastructure.Services;

public class ServiceDayOverrideService : IServiceDayOverrideService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly IEmployeeAuthorization _authorization;

    public ServiceDayOverrideService(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        IEmployeeAuthorization authorization)
    {
        _contextFactory = contextFactory;
        _authorization = authorization;
    }

    public async Task<List<ServiceDayOverride>> GetAllAsync()
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        return await db.ServiceDayOverrides
                        .Include(o => o.Service)
                        .ToListAsync();
    }

    public async Task<List<ServiceDayOverride>> GetByServiceAsync(int serviceId)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        return await db.ServiceDayOverrides
                        .Where(o => o.ServiceId == serviceId)
                        .Include(o => o.Service)
                        .ToListAsync();
    }

    public async Task<ServiceDayOverride?> GetByIdAsync(int id)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        return await db.ServiceDayOverrides.FindAsync(id);
    }

    public async Task<ServiceDayOverride> CreateAsync(ServiceDayOverride model)
    {
        // Serverseitige Autorisierung: gilt unabhaengig davon, ob die Oberflaeche
        // das zugehoerige Bedienelement ueberhaupt anbietet.
        await _authorization.RequireMinimumRoleAsync(EmployeeRole.ServiceChef);

        await using var db = await _contextFactory.CreateDbContextAsync();
        db.ServiceDayOverrides.Add(model);
        await db.SaveChangesAsync();
        return model;
    }

    public async Task<ServiceDayOverride> UpdateAsync(int id, ServiceDayOverride model)
    {
        // Serverseitige Autorisierung: gilt unabhaengig davon, ob die Oberflaeche
        // das zugehoerige Bedienelement ueberhaupt anbietet.
        await _authorization.RequireMinimumRoleAsync(EmployeeRole.ServiceChef);

        await using var db = await _contextFactory.CreateDbContextAsync();
        var existing = await db.ServiceDayOverrides.FindAsync(id);
        if (existing == null)
        {
            throw new BusinessRuleViolationException("Override nicht gefunden.");
        }

        existing.Date = model.Date;
        existing.StartTime = model.StartTime;
        existing.EndTime = model.EndTime;
        existing.CapacityPerSlotOverride = model.CapacityPerSlotOverride;
        existing.ServiceId = model.ServiceId;

        await db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        // Serverseitige Autorisierung: gilt unabhaengig davon, ob die Oberflaeche
        // das zugehoerige Bedienelement ueberhaupt anbietet.
        await _authorization.RequireMinimumRoleAsync(EmployeeRole.ServiceChef);

        await using var db = await _contextFactory.CreateDbContextAsync();
        var existing = await db.ServiceDayOverrides.FindAsync(id);
        if (existing == null)
        {
            return false;
        }

        db.ServiceDayOverrides.Remove(existing);
        await db.SaveChangesAsync();
        return true;
    }
}
