using ConsulatTermine.Application.Interfaces;
using ConsulatTermine.Application.DTOs;
using ConsulatTermine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using ConsulatTermine.Infrastructure.Persistence;
using ConsulatTermine.Application.ViewModels;

namespace ConsulatTermine.Infrastructure.Services
{
    public class ServiceService : IServiceService
    {
        private readonly ApplicationDbContext _context;

        public ServiceService(ApplicationDbContext context)
        {
            _context = context;
        }

        // -------------------------------------------------------------
        // GET ALL SERVICES
        // -------------------------------------------------------------
        public async Task<List<Service>> GetAllServicesAsync()
{
    return await _context.Services
        .Include(s => s.AssignedEmployees)
            .ThenInclude(a => a.Employee)
        .AsNoTracking()
        .OrderBy(s => s.Name)
        .ToListAsync();
}


        // -------------------------------------------------------------
        // GET SERVICE BY ID (inkl. WorkingHours, Overrides, Employees)
        // -------------------------------------------------------------
        public async Task<Service?> GetServiceByIdAsync(int id)
        {
            return await _context.Services
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
            var entity = new Service
            {
                Name = dto.Name,
                Description = dto.Description,
                CapacityPerSlot = dto.CapacityPerSlot,
                SlotDurationMinutes = dto.SlotDurationMinutes
            };

            _context.Services.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        // -------------------------------------------------------------
        // UPDATE SERVICE
        // -------------------------------------------------------------
        public async Task<Service> UpdateServiceAsync(int id, ServiceDto dto)
        {
            var entity = await _context.Services.FindAsync(id);

            if (entity == null)
                throw new Exception("Service not found");

            entity.Name = dto.Name;
            entity.Description = dto.Description;
            entity.CapacityPerSlot = dto.CapacityPerSlot;
            entity.SlotDurationMinutes = dto.SlotDurationMinutes;

            await _context.SaveChangesAsync();
            return entity;
        }

        // -------------------------------------------------------------
        // DELETE SERVICE
        // -------------------------------------------------------------
        public async Task<bool> DeleteServiceAsync(int id)
        {
            var entity = await _context.Services.FindAsync(id);
            if (entity == null)
                return false;

            _context.Services.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

public async Task<List<SlotViewModel>> GetAvailableSlotsForServiceAsync(int serviceId, DateOnly date)
{
    // 1️⃣ Aktiven Plan prüfen
    var activePlan = await _context.WorkingSchedulePlans
        .AsNoTracking()
        .FirstOrDefaultAsync(p =>
            p.ServiceId == serviceId &&
            p.IsActive &&
            p.ValidFromDate <= date &&
            p.ValidToDate >= date);

    if (activePlan == null)
        return new List<SlotViewModel>(); // ❌ kein gültiger Plan → keine Slots

    // 2️⃣ Service laden (Basisdaten)
    var service = await _context.Services
        .AsNoTracking()
        .Include(s => s.AssignedEmployees)
        .FirstOrDefaultAsync(s => s.Id == serviceId);

    if (service == null)
        return new List<SlotViewModel>();

    // 3️⃣ WorkingHours NUR für diesen Plan
    var workingHours = await _context.WorkingHours
        .Where(w =>
            w.ServiceId == serviceId &&
            w.WorkingSchedulePlanId == activePlan.Id)
        .ToListAsync();

    // 4️⃣ Overrides NUR für diesen Plan
    var overrides = await _context.ServiceDayOverrides
        .Where(o =>
            o.ServiceId == serviceId &&
            o.WorkingSchedulePlanId == activePlan.Id)
        .ToListAsync();

    // 5️⃣ Bestehende Termine
    var existingAppointments = await _context.Appointments
        .Where(a =>
            a.ServiceId == serviceId &&
            a.Date.Date == date.ToDateTime(TimeOnly.MinValue).Date)
        .ToListAsync();

    // 6️⃣ Slot-Berechnung
    var freeSlots = AppointmentCalculator.GetAvailableSlots(
        service,
        date.ToDateTime(TimeOnly.MinValue).Date,
        workingHours,
        overrides,
        existingAppointments);

    // 7️⃣ Mapping → ViewModel
    return freeSlots
        .Select(kvp => new SlotViewModel
        {
            DateTime = date.ToDateTime(TimeOnly.FromTimeSpan(kvp.Key.Start)),
            FreeSlots = kvp.Value,
            BookedSlots = 0
        })
        .ToList();
}

public async Task<List<AvailableSlotDto>> GetAvailableSlotDtosAsync(
    int serviceId,
    DateTime date)
{
    var slots = await GetAvailableSlotsForServiceAsync(
        serviceId,
        DateOnly.FromDateTime(date));

    return slots
        .Select(s => new AvailableSlotDto
        {
            SlotStart = s.DateTime,
            FreeCapacity = s.FreeSlots
        })
        .OrderBy(x => x.SlotStart)
        .ToList();
}



public async Task<ServiceDto> GetByIdAsync(int id)
{
    // Lade den Service aus der Datenbank
    var service = await _context.Services
        .FirstOrDefaultAsync(s => s.Id == id);

    if (service == null)
        return null!; // oder wirf eine Ausnahme, je nachdem wie du Fehler handhaben willst

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
}
