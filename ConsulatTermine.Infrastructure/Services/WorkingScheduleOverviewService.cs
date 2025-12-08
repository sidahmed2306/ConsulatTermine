using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsulatTermine.Application.Interfaces;
using ConsulatTermine.Application.ViewModels;
using ConsulatTermine.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using ConsulatTermine.Infrastructure.Persistence;

namespace ConsulatTermine.Infrastructure.Services
{
    public class WorkingScheduleOverviewService : IWorkingScheduleOverviewService
    {
        private readonly ApplicationDbContext _db;
        private readonly IServiceService _serviceService;

        public WorkingScheduleOverviewService(
            ApplicationDbContext db,
            IServiceService serviceService)
        {
            _db = db;
            _serviceService = serviceService;
        }

        // --------------------------------------------------------------------
        // 1) Alle Services mit vollständiger Übersicht
        // --------------------------------------------------------------------
        public async Task<List<WorkingScheduleOverviewItem>> GetOverviewAsync()
        {
            var services = await _db.Services
                .Include(s => s.WorkingHours)
                .Include(s => s.DayOverrides)
                .Include(s => s.AssignedEmployees)
                    .ThenInclude(a => a.Employee)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return services.Select(BuildOverviewForService).ToList();
        }

        // --------------------------------------------------------------------
        // 2) Einzelner Service
        // --------------------------------------------------------------------
        public async Task<WorkingScheduleOverviewItem?> GetByServiceIdAsync(int serviceId)
        {
            var service = await _db.Services
                .Include(s => s.WorkingHours)
                .Include(s => s.DayOverrides)
                .Include(s => s.AssignedEmployees)
                    .ThenInclude(a => a.Employee)
                .FirstOrDefaultAsync(s => s.Id == serviceId);

            if (service == null)
                return null;

            return BuildOverviewForService(service);
        }

        // --------------------------------------------------------------------
        // 3) Jahresplan löschen
        // --------------------------------------------------------------------
        public async Task<bool> DeleteYearAsync(int serviceId, int year)
        {
            var toDelete = await _db.ServiceDayOverrides
                .Where(o => o.ServiceId == serviceId && o.Date.Year == year)
                .ToListAsync();

            if (toDelete.Any())
                _db.ServiceDayOverrides.RemoveRange(toDelete);

            await _db.SaveChangesAsync();
            return true;
        }

        // --------------------------------------------------------------------
        // 🧠 Kern: vollständige Übersicht für einen Service erzeugen
        // --------------------------------------------------------------------
        private WorkingScheduleOverviewItem BuildOverviewForService(Service service)
        {
            var item = new WorkingScheduleOverviewItem
            {
                ServiceId = service.Id,
                ServiceName = service.Name,
                DefaultCapacityPerSlot = service.CapacityPerSlot,
                SlotDurationMinutes = service.SlotDurationMinutes,
                EmployeeCount = service.AssignedEmployees?.Count ?? 0
            };

            // Jahresgruppen erkennen
            var yearPlans = BuildYearPlans(service);
            item.YearPlans.AddRange(yearPlans);

            return item;
        }

        // --------------------------------------------------------------------
        // 4) Jahrespläne auf Basis der Overrides erstellen
        // --------------------------------------------------------------------
        private List<WorkingScheduleYearPlan> BuildYearPlans(Service service)
        {
            var result = new List<WorkingScheduleYearPlan>();

            // Welche Jahre kommen in Overrides vor?
            var years = service.DayOverrides
                .Select(x => x.Date.Year)
                .Distinct()
                .OrderBy(y => y)
                .ToList();

            foreach (var year in years)
            {
                var yearPlan = new WorkingScheduleYearPlan
                {
                    ServiceId = service.Id,
                    ServiceName = service.Name,
                    Year = year,
                    DefaultCapacityPerSlot = service.CapacityPerSlot,
                    SlotDurationMinutes = service.SlotDurationMinutes,
                    EmployeeCount = service.AssignedEmployees?.Count ?? 0
                };

                // Monate berechnen
                yearPlan.Months = service.DayOverrides
                    .Where(o => o.Date.Year == year)
                    .Select(o => o.Date.Month)
                    .Distinct()
                    .OrderBy(m => m)
                    .ToList();

                // Reguläre Öffnungszeiten aus WorkingHours
                yearPlan.RegularHours = ExtractRegularHours(service);

                // Wöchentliche Muster-Aggregation
                yearPlan.WeeklyOverrides = ExtractWeeklyPatterns(service, year);

                // Datumsausnahmen
                yearPlan.DateOverrides = ExtractDateOverrides(service, year, yearPlan.Months);


                result.Add(yearPlan);
            }

            return result;
        }

        // --------------------------------------------------------------------
        // 5) Reguläre Öffnungszeiten aus WorkingHours extrahieren
        // --------------------------------------------------------------------
       // --------------------------------------------------------------------
// 5) Reguläre Öffnungszeiten aus WorkingHours extrahieren (je Wochentag nur einmal)
// --------------------------------------------------------------------
private List<RegularOpeningInfo> ExtractRegularHours(Service service)
{
    return service.WorkingHours
        .GroupBy(w => w.Day)
        .Select(g => g.First())
        .OrderBy(w => w.Day)
        .Select(w => new RegularOpeningInfo
        {
            Day = w.Day,
            StartTime = w.StartTime,
            EndTime = w.EndTime
        })
        .ToList();
}


        // --------------------------------------------------------------------
        // 6) Weekly Pattern erkennen
        // --------------------------------------------------------------------
        private List<WeeklyOverridePatternInfo> ExtractWeeklyPatterns(Service service, int year)
        {
            var overrides = service.DayOverrides
                .Where(o => o.Date.Year == year)
                .ToList();

            // Gruppieren nach DayOfWeek
            var groups = overrides
                .GroupBy(o => o.Date.DayOfWeek)
                .ToList();

            var patterns = new List<WeeklyOverridePatternInfo>();

            foreach (var g in groups)
            {
                // Prüfen, ob alle Overrides des Wochen-Tags identisch sind
                bool allClosed = g.All(o => o.IsClosed);

                bool consistentTimes =
                    g.All(o => !o.IsClosed)
                    && g.Select(o => o.StartTime).Distinct().Count() == 1
                    && g.Select(o => o.EndTime).Distinct().Count() == 1;

                bool consistentCapacity =
                    g.All(o => o.CapacityPerSlotOverride == g.First().CapacityPerSlotOverride);

                if (!allClosed && !consistentTimes)
                {
                    // nicht konsistent = KEIN weekly pattern → wird einzeln in DateOverrides angezeigt
                    continue;
                }

                var first = g.First();

                var p = new WeeklyOverridePatternInfo
                {
                    Day = g.Key,
                    IsClosed = allClosed,
                    StartTime = allClosed ? null : first.StartTime,
                    EndTime = allClosed ? null : first.EndTime,
                    CapacityPerSlotOverride = first.CapacityPerSlotOverride
                };

                // alle echten betroffenen Daten
                p.AffectedDates = g.Select(x => x.Date).OrderBy(d => d).ToList();

                patterns.Add(p);
            }

            return patterns;
        }

        // --------------------------------------------------------------------
        // 7) Einzelne Datumsausnahmen (NICHT weekly)
        // --------------------------------------------------------------------
        private List<DateOverrideInfo> ExtractDateOverrides(Service service, int year, IEnumerable<int> planMonths)
{
    // Filter auf das Jahr + Monate, die im Plan enthalten sind
    var overrides = service.DayOverrides
        .Where(o => o.Date.Year == year && planMonths.Contains(o.Date.Month))
        .ToList();

    // Gruppieren pro Datum und nur einen Eintrag pro Tag auswählen
    var grouped = overrides
        .GroupBy(o => o.Date.Date)
        .Select(g =>
        {
            // Wahl: ersten Eintrag nehmen; alternativ könnte man Regeln definieren
            var first = g.OrderBy(o => o.Date).First();

            return new DateOverrideInfo
            {
                Date = first.Date.Date,
                IsClosed = first.IsClosed,
                StartTime = first.StartTime,
                EndTime = first.EndTime,
                CapacityPerSlotOverride = first.CapacityPerSlotOverride
            };
        })
        .OrderBy(d => d.Date)
        .ToList();

    return grouped;
}

    }
}
