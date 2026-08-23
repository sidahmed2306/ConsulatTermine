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

public class ServiceService : IServiceService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly IEmployeeAuthorization _authorization;

    public ServiceService(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        IEmployeeAuthorization authorization)
    {
        _contextFactory = contextFactory;
        _authorization = authorization;
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
        // Serverseitige Autorisierung: gilt unabhaengig davon, ob die Oberflaeche
        // das zugehoerige Bedienelement ueberhaupt anbietet.
        await _authorization.RequireMinimumRoleAsync(EmployeeRole.Admin);

        await using var db = await _contextFactory.CreateDbContextAsync();
        var entity = new Service
        {
            Name = dto.Name,
            NameEnglish = dto.NameEnglish,
            NameArabic = dto.NameArabic,
            Description = dto.Description,
            DescriptionEnglish = dto.DescriptionEnglish,
            DescriptionArabic = dto.DescriptionArabic,
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
        // Serverseitige Autorisierung: gilt unabhaengig davon, ob die Oberflaeche
        // das zugehoerige Bedienelement ueberhaupt anbietet.
        await _authorization.RequireMinimumRoleAsync(EmployeeRole.Admin);

        await using var db = await _contextFactory.CreateDbContextAsync();
        var entity = await db.Services.FindAsync(id);

        if (entity == null)
        {
            throw new BusinessRuleViolationException(BusinessMessages.Get("ServiceNotFound"));
        }

        entity.Name = dto.Name;
        entity.NameEnglish = dto.NameEnglish;
        entity.NameArabic = dto.NameArabic;
        entity.Description = dto.Description;
        entity.DescriptionEnglish = dto.DescriptionEnglish;
        entity.DescriptionArabic = dto.DescriptionArabic;
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
        // Serverseitige Autorisierung: gilt unabhaengig davon, ob die Oberflaeche
        // das zugehoerige Bedienelement ueberhaupt anbietet.
        await _authorization.RequireMinimumRoleAsync(EmployeeRole.Admin);

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
            NameEnglish = service.NameEnglish,
            NameArabic = service.NameArabic,
            Description = service.Description,
            DescriptionEnglish = service.DescriptionEnglish,
            DescriptionArabic = service.DescriptionArabic,
            CapacityPerSlot = service.CapacityPerSlot,
            Floor = service.Floor,
            SlotDurationMinutes = service.SlotDurationMinutes
        };

        return dto;
    }




}
