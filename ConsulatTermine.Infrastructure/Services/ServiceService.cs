using ConsulatTermine.Application.DTOs;
using ConsulatTermine.Application.Exceptions;
using ConsulatTermine.Application.Interfaces;
using ConsulatTermine.Domain.Entities;
using ConsulatTermine.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConsulatTermine.Infrastructure.Services;

public class ServiceService : IServiceService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public ServiceService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    // -------------------------------------------------------------
    // GET ALL SERVICES
    // -------------------------------------------------------------
    public async Task<List<Service>> GetAllServicesAsync()
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        return await db.Services
            .Include(s => s.AssignedEmployees)
                .ThenInclude(a => a.Employee)
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync();
    }


    public async Task<List<Service>> GetServicesForEmployeeAsync(int employeeId)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        return await db.EmployeeServiceAssignments
            .Where(a => a.EmployeeId == employeeId)
            .Select(a => a.Service!) // <-- WICHTIG
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync();
    }


    // -------------------------------------------------------------
    // GET SERVICE BY ID (inkl. WorkingHours, Overrides, Employees)
    // -------------------------------------------------------------
    public async Task<Service?> GetServiceByIdAsync(int id)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        return await db.Services
            .Include(s => s.WorkingHours)
            .Include(s => s.DayOverrides)
            .Include(s => s.AssignedEmployees)
                .ThenInclude(a => a.Employee)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    // -------------------------------------------------------------
    // CREATE SERVICE
    // -------------------------------------------------------------
    public async Task<Service> CreateServiceAsync(ServiceDto dto)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var entity = new Service
        {
            Name = dto.Name,
            Description = dto.Description,
            Floor = dto.Floor,
            SlotDurationMinutes = dto.SlotDurationMinutes
        };

        db.Services.Add(entity);
        await db.SaveChangesAsync();
        return entity;
    }

    // -------------------------------------------------------------
    // UPDATE SERVICE
    // -------------------------------------------------------------
    public async Task<Service> UpdateServiceAsync(int id, ServiceDto dto)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var entity = await db.Services.FindAsync(id);

        if (entity == null)
        {
            throw new BusinessRuleViolationException("Der Service wurde nicht gefunden.");
        }

        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.CapacityPerSlot = dto.CapacityPerSlot;
        entity.Floor = dto.Floor;
        entity.SlotDurationMinutes = dto.SlotDurationMinutes;

        await db.SaveChangesAsync();
        return entity;
    }

    // -------------------------------------------------------------
    // DELETE SERVICE
    // -------------------------------------------------------------
    public async Task<bool> DeleteServiceAsync(int id)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var entity = await db.Services.FindAsync(id);
        if (entity == null)
        {
            return false;
        }

        db.Services.Remove(entity);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<ServiceDto> GetByIdAsync(int id)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        // Lade den Service aus der Datenbank
        var service = await db.Services
            .FirstOrDefaultAsync(s => s.Id == id);

        if (service == null)
        {
            return null!; // oder wirf eine Ausnahme, je nachdem wie du Fehler handhaben willst
        }

        // Mappe zur DTO
        var dto = new ServiceDto
        {
            Id = service.Id,
            Name = service.Name,
            Description = service.Description,
            CapacityPerSlot = service.CapacityPerSlot,
            SlotDurationMinutes = service.SlotDurationMinutes
        };

        return dto;
    }




}
