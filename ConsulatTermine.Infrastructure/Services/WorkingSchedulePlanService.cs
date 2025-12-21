using ConsulatTermine.Application.DTOs.WorkingSchedulePlan;
using ConsulatTermine.Application.Interfaces;
using ConsulatTermine.Domain.Entities;
using ConsulatTermine.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConsulatTermine.Infrastructure.Services;

public class WorkingSchedulePlanService : IWorkingSchedulePlanService
{
    private readonly ApplicationDbContext _db;

    public WorkingSchedulePlanService(ApplicationDbContext db)
    {
        _db = db;
    }

    // ----------------------------------------------------
    // CREATE / UPDATE
    // ----------------------------------------------------
    public async Task<WorkingSchedulePlanDto> SaveAsync(WorkingSchedulePlanDto dto)
    {
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

            _db.WorkingSchedulePlans.Add(entity);
        }
        else
        {
            // UPDATE
            entity = await _db.WorkingSchedulePlans
                .FirstOrDefaultAsync(x => x.Id == dto.Id)
                ?? throw new Exception("WorkingSchedulePlan not found.");

            entity.ValidFromDate = dto.ValidFromDate;
            entity.ValidToDate = dto.ValidToDate;
            entity.IsActive = dto.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
        }

        // 🔴 Business-Regel:
        // Wenn dieser Plan aktiv ist → alle anderen für diesen Service deaktivieren
        if (entity.IsActive)
        {
            await DeactivateOtherPlansAsync(entity);
        }

        await _db.SaveChangesAsync();

        return MapToDto(entity);
    }

    // ----------------------------------------------------
    // READ
    // ----------------------------------------------------
    public async Task<List<WorkingSchedulePlanDto>> GetByServiceAsync(int serviceId)
    {
        return await _db.WorkingSchedulePlans
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
        var entity = await _db.WorkingSchedulePlans.FindAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    // ----------------------------------------------------
    // DELETE
    // ----------------------------------------------------
    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _db.WorkingSchedulePlans.FindAsync(id);
        if (entity == null)
            return false;

        _db.WorkingSchedulePlans.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    // ----------------------------------------------------
    // SET ACTIVE
    // ----------------------------------------------------
    public async Task<bool> SetActiveAsync(int id)
    {
        var entity = await _db.WorkingSchedulePlans.FindAsync(id);
        if (entity == null)
            return false;

        entity.IsActive = true;
        entity.UpdatedAt = DateTime.UtcNow;

        await DeactivateOtherPlansAsync(entity);

        await _db.SaveChangesAsync();
        return true;
    }

    // ----------------------------------------------------
    // HELPER
    // ----------------------------------------------------
    private async Task DeactivateOtherPlansAsync(WorkingSchedulePlan activePlan)
    {
        var others = await _db.WorkingSchedulePlans
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
