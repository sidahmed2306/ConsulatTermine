using ConsulatTermine.Application.Interfaces;
using ConsulatTermine.Application.DTOs;
using ConsulatTermine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using ConsulatTermine.Infrastructure.Persistence;

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
    }
}
