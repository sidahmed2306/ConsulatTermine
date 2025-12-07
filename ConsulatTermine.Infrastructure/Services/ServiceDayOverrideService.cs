using ConsulatTermine.Application.Interfaces;
using ConsulatTermine.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ConsulatTermine.Infrastructure.Persistence
{
    public class ServiceDayOverrideService : IServiceDayOverrideService
    {
        private readonly ApplicationDbContext _db;

        public ServiceDayOverrideService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<ServiceDayOverride>> GetAllAsync()
        {
            return await _db.ServiceDayOverrides
                            .Include(o => o.Service)
                            .ToListAsync();
        }

        public async Task<List<ServiceDayOverride>> GetByServiceAsync(int serviceId)
        {
            return await _db.ServiceDayOverrides
                            .Where(o => o.ServiceId == serviceId)
                            .Include(o => o.Service)
                            .ToListAsync();
        }

        public async Task<ServiceDayOverride?> GetByIdAsync(int id)
        {
            return await _db.ServiceDayOverrides.FindAsync(id);
        }

        public async Task<ServiceDayOverride> CreateAsync(ServiceDayOverride model)
        {
            _db.ServiceDayOverrides.Add(model);
            await _db.SaveChangesAsync();
            return model;
        }

        public async Task<ServiceDayOverride> UpdateAsync(int id, ServiceDayOverride model)
        {
            var existing = await _db.ServiceDayOverrides.FindAsync(id);
            if (existing == null) throw new Exception("Override nicht gefunden.");

            existing.Date = model.Date;
            existing.StartTime = model.StartTime;
            existing.EndTime = model.EndTime;
            existing.CapacityPerSlotOverride = model.CapacityPerSlotOverride;
            existing.ServiceId = model.ServiceId;

            await _db.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _db.ServiceDayOverrides.FindAsync(id);
            if (existing == null) return false;

            _db.ServiceDayOverrides.Remove(existing);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
