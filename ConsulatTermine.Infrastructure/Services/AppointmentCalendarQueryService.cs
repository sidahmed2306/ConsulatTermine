using ConsulatTermine.Application.Interfaces;
using ConsulatTermine.Domain.Entities;
using ConsulatTermine.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConsulatTermine.Infrastructure.Services
{
    public class AppointmentCalendarQueryService : IAppointmentCalendarQueryService
    {
        private readonly ApplicationDbContext _context;

        public AppointmentCalendarQueryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<DateOnly, bool>> GetBookableDaysAsync(
            int serviceId,
            int year,
            int month)
        {
            var result = new Dictionary<DateOnly, bool>();

            // 1️⃣ Aktiven Plan laden
            var plan = await _context.WorkingSchedulePlans
                .AsNoTracking()
                .FirstOrDefaultAsync(p =>
                    p.ServiceId == serviceId &&
                    p.IsActive);

            if (plan == null)
                return result; // ❌ kein Plan → nichts buchbar

            var monthStart = new DateOnly(year, month, 1);
            var monthEnd = new DateOnly(
                year,
                month,
                DateTime.DaysInMonth(year, month));

            // 2️⃣ Monat liegt komplett außerhalb des Plan-Zeitraums
            if (monthEnd < plan.ValidFromDate || monthStart > plan.ValidToDate)
                return result;

            // 3️⃣ Plan-gebundene Öffnungszeiten & Overrides laden
            var workingHours = await _context.WorkingHours
                .AsNoTracking()
                .Where(w =>
                    w.ServiceId == serviceId &&
                    w.WorkingSchedulePlanId == plan.Id)
                .ToListAsync();

            var overrides = await _context.ServiceDayOverrides
                .AsNoTracking()
                .Where(o =>
                    o.ServiceId == serviceId &&
                    o.WorkingSchedulePlanId == plan.Id)
                .ToListAsync();

            // 4️⃣ Monat iterieren
            for (var day = monthStart; day <= monthEnd; day = day.AddDays(1))
            {
                // außerhalb Plan-Zeitraum → nicht buchbar
                if (day < plan.ValidFromDate || day > plan.ValidToDate)
                {
                    result[day] = false;
                    continue;
                }

                var date = day.ToDateTime(TimeOnly.MinValue);

                // 4.1 Datums-Override?
                var overrideDay = overrides
                    .FirstOrDefault(o => o.Date.Date == date.Date);

                if (overrideDay != null)
                {
                    result[day] = !overrideDay.IsClosed;
                    continue;
                }

                // 4.2 Regulärer Wochentag?
                var hasRegularOpening = workingHours
                    .Any(w => w.Day == date.DayOfWeek);

                result[day] = hasRegularOpening;
            }

            return result;
        }
    }
}
