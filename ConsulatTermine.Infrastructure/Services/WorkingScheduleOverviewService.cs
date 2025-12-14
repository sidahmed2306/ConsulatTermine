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
        //    -> hier verknüpfen wir WeeklyPatterns und DateOverrides richtig
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
                    SlotDurationMinutes = service.SlotDurationMinutes,
                    EmployeeCount = service.AssignedEmployees?.Count ?? 0
                };

                // Monate berechnen (basierend auf DayOverrides)
               // Monate aus REGULAEREN Arbeitszeiten ermitteln
var months = new List<int>();

if (service.WorkingHours.Any())
{
    // Wir verwenden Monate, die im Request abgelegt wurden.
    // RegularHours existieren pro Wochentag – daher holen wir die Monate
    // anhand aller Overrides (DateOverrides & WeeklyPatterns)
    months = service.DayOverrides
        .Where(o => o.Date.Year == year)
        .Select(o => o.Date.Month)
        .Distinct()
        .OrderBy(m => m)
        .ToList();
}
else
{
    // Falls keine WorkingHours vorhanden (unwahrscheinlich),
    // extrahieren wir die Monate aus DateOverrides nur
    months = service.DayOverrides
        .Where(o => o.Date.Year == year)
        .Select(o => o.Date.Month)
        .Distinct()
        .OrderBy(m => m)
        .ToList();
}

yearPlan.Months = months;


                // Reguläre Öffnungszeiten aus WorkingHours
                yearPlan.RegularHours = ExtractRegularHours(service);

                // 1) Wöchentliche Muster-Aggregation
                var weeklyPatterns = ExtractWeeklyPatterns(service, year);
                yearPlan.WeeklyOverrides = weeklyPatterns;

                // 2) Alle Daten, die zu solchen WeeklyPatterns gehören, sammeln
                var weeklyPatternDates = weeklyPatterns
                    .SelectMany(p => p.AffectedDates ?? new List<DateTime>())
                    .Select(d => d.Date)
                    .Distinct()
                    .ToHashSet();

                // 3) Datumsausnahmen: nur Tage, die NICHT zu einem WeeklyPattern gehören
                yearPlan.DateOverrides = ExtractDateOverrides(
                    service,
                    year,
                    yearPlan.Months);

                result.Add(yearPlan);
            }

            return result;
        }

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
    // 1) Alle weekly-overrides laden (egal wie viele Daten es gibt)
    var weeklyOverrides = service.DayOverrides
        .Where(o => o.IsWeeklyOverride && o.Date.Year == year)
        .ToList();

    // 2) Gruppieren nach WeeklyDay
    var groups = weeklyOverrides
        .GroupBy(o => o.WeeklyDay)
        .ToList();

    var result = new List<WeeklyOverridePatternInfo>();

    foreach (var group in groups)
    {
        var first = group.First();   // alle Overrides im Group haben gleiche Werte

        var item = new WeeklyOverridePatternInfo
        {
            Day = first.WeeklyDay!.Value,
            IsClosed = first.IsClosed,
            StartTime = first.StartTime,
            EndTime = first.EndTime,
            CapacityPerSlotOverride = first.CapacityPerSlotOverride,

            // Optional: diese Liste kann leer bleiben
            AffectedDates = new List<DateTime>()
        };

        result.Add(item);
    }

    return result;
}




        // --------------------------------------------------------------------
        // 7) Einzelne Datumsausnahmen (NICHT weekly)
        //    -> Jahr + ausgewählte Monate + konsolidiert pro Datum
        //    -> excl. aller Dates, die schon in WeeklyPatterns verwendet werden
        // --------------------------------------------------------------------
       private List<DateOverrideInfo> ExtractDateOverrides(
    Service service,
    int year,
    IEnumerable<int> planMonths)
{
    // 1) Nur Datums-Ausnahmen -> IsWeeklyOverride = false
    var dateOverrides = service.DayOverrides
        .Where(o =>
            !o.IsWeeklyOverride &&              // <-- nur DateOverrides
            o.Date.Year == year &&
            planMonths.Contains(o.Date.Month))  // nur gewählte Monate
        .ToList();

    // 2) Gruppieren pro Tag (falls mehrere Overrides am selben Datum)
    var grouped = dateOverrides
        .GroupBy(o => o.Date.Date)
        .Select(g =>
        {
            var first = g.First();

            return new DateOverrideInfo
            {
                Date = first.Date,
                IsClosed = first.IsClosed,
                StartTime = first.IsClosed ? null : first.StartTime,
                EndTime = first.IsClosed ? null : first.EndTime,
                CapacityPerSlotOverride = first.CapacityPerSlotOverride
            };
        })
        .OrderBy(d => d.Date)
        .ToList();

    return grouped;
}

    }
}
