using ConsulatTermine.Application.DTOs;
using ConsulatTermine.Application.Exceptions;
using ConsulatTermine.Application.Interfaces;
using ConsulatTermine.Application.Resources;
using ConsulatTermine.Application.Security;
using ConsulatTermine.Domain.Entities;
using ConsulatTermine.Domain.Enums;
using ConsulatTermine.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConsulatTermine.Infrastructure.Services;

public class WorkingSchedulePlanService : IWorkingSchedulePlanService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly IEmployeeAuthorization _authorization;

    public WorkingSchedulePlanService(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        IEmployeeAuthorization authorization)
    {
        _contextFactory = contextFactory;
        _authorization = authorization;
    }

    // ----------------------------------------------------
    // CREATE / UPDATE
    // ----------------------------------------------------
    public async Task<WorkingSchedulePlanDto> SaveAsync(WorkingSchedulePlanDto dto)
    {
        // Serverseitige Autorisierung: gilt unabhaengig davon, ob die Oberflaeche
        // das zugehoerige Bedienelement ueberhaupt anbietet.
        await _authorization.RequireMinimumRoleAsync(EmployeeRole.ServiceChef);

        await using var db = await _contextFactory.CreateDbContextAsync();
        WorkingSchedulePlan entity;

        if (dto.Id == 0)
        {
            // CREATE
            entity = new WorkingSchedulePlan
            {
                ServiceId = dto.ServiceId,
                ValidFromDate = dto.ValidFromDate,
                ValidToDate = dto.ValidToDate,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            db.WorkingSchedulePlans.Add(entity);
        }
        else
        {
            // UPDATE
            entity = await db.WorkingSchedulePlans
                .FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new BusinessRuleViolationException(BusinessMessages.Get("SchedulePlanNotFound"));

            entity.ValidFromDate = dto.ValidFromDate;
            entity.ValidToDate = dto.ValidToDate;
            entity.IsActive = dto.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
        }

        // 🔴 Business-Regel:
        // Wenn dieser Plan aktiv ist → alle anderen für diesen Service deaktivieren
        if (entity.IsActive)
        {
            await DeactivateOtherPlansAsync(db, entity);
        }

        await db.SaveChangesAsync();

        return MapToDto(entity);
    }

    // ----------------------------------------------------
    // READ
    // ----------------------------------------------------
    public async Task<List<WorkingSchedulePlanDto>> GetByServiceAsync(int serviceId)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        return await db.WorkingSchedulePlans
    .Where(x => x.ServiceId == serviceId)
    .OrderByDescending(x => x.ValidFromDate)
    .Select(x => new WorkingSchedulePlanDto
    {
        Id = x.Id,
        ServiceId = x.ServiceId,
        ValidFromDate = x.ValidFromDate,
        ValidToDate = x.ValidToDate,
        IsActive = x.IsActive,
        CreatedAt = x.CreatedAt,
        UpdatedAt = x.UpdatedAt
    })
    .ToListAsync();

    }

    public async Task<WorkingSchedulePlanDto?> GetByIdAsync(int id)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var entity = await db.WorkingSchedulePlans.FindAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    // ----------------------------------------------------
    // DELETE
    // ----------------------------------------------------
    public async Task<bool> DeleteAsync(int id)
    {
        // Serverseitige Autorisierung: gilt unabhaengig davon, ob die Oberflaeche
        // das zugehoerige Bedienelement ueberhaupt anbietet.
        await _authorization.RequireMinimumRoleAsync(EmployeeRole.ServiceChef);

        await using var db = await _contextFactory.CreateDbContextAsync();
        var entity = await db.WorkingSchedulePlans.FindAsync(id);
        if (entity == null)
        {
            return false;
        }

        db.WorkingSchedulePlans.Remove(entity);
        await db.SaveChangesAsync();
        return true;
    }

    // ----------------------------------------------------
    // SET ACTIVE
    // ----------------------------------------------------
    public async Task<bool> SetActiveAsync(int id)
    {
        // Serverseitige Autorisierung: gilt unabhaengig davon, ob die Oberflaeche
        // das zugehoerige Bedienelement ueberhaupt anbietet.
        await _authorization.RequireMinimumRoleAsync(EmployeeRole.ServiceChef);

        await using var db = await _contextFactory.CreateDbContextAsync();
        var entity = await db.WorkingSchedulePlans.FindAsync(id);
        if (entity == null)
        {
            return false;
        }

        entity.IsActive = true;
        entity.UpdatedAt = DateTime.UtcNow;

        await DeactivateOtherPlansAsync(db, entity);

        await db.SaveChangesAsync();
        return true;
    }

    // ----------------------------------------------------
    // HELPER
    // ----------------------------------------------------
    /// <summary>
    /// Deaktiviert alle anderen Pläne desselben Service.
    /// Arbeitet bewusst im Kontext des Aufrufers: die Änderungen gehören zu derselben
    /// Transaktion wie das Aktivieren des neuen Plans und werden dort gespeichert.
    /// </summary>
    private static async Task DeactivateOtherPlansAsync(
        ApplicationDbContext db,
        WorkingSchedulePlan activePlan)
    {
        var others = await db.WorkingSchedulePlans
            .Where(x =>
                x.ServiceId == activePlan.ServiceId &&
                x.Id != activePlan.Id &&
                x.IsActive)
            .ToListAsync();

        foreach (var plan in others)
        {
            plan.IsActive = false;
            plan.UpdatedAt = DateTime.UtcNow;
        }
    }

    private static WorkingSchedulePlanDto MapToDto(WorkingSchedulePlan e)
    {
        return new WorkingSchedulePlanDto
        {
            Id = e.Id,
            ServiceId = e.ServiceId,
            ValidFromDate = e.ValidFromDate,
            ValidToDate = e.ValidToDate,
            IsActive = e.IsActive,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt
        };
    }
}
