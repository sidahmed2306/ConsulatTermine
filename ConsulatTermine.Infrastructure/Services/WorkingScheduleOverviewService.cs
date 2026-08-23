using ConsulatTermine.Application.Interfaces;
using ConsulatTermine.Application.ViewModels;
using ConsulatTermine.Domain.Entities;
using ConsulatTermine.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConsulatTermine.Infrastructure.Services;

public class WorkingScheduleOverviewService : IWorkingScheduleOverviewService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public WorkingScheduleOverviewService(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        IServiceService serviceService) // bleibt im ctor, auch wenn wir es hier nicht nutzen
    {
        _contextFactory = contextFactory;
    }

    // --------------------------------------------------------------------
    // 1) Alle Services mit vollständiger Übersicht
    // --------------------------------------------------------------------
    public async Task<List<WorkingScheduleOverviewItem>> GetOverviewAsync()
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        // Services (Basisdaten + Mitarbeiter)
        var services = await db.Services
            .AsNoTracking()
            .Include(s => s.AssignedEmployees)
                .ThenInclude(a => a.Employee)
            .OrderBy(s => s.Name)
            .ToListAsync();

        if (services.Count == 0)
        {
            return new List<WorkingScheduleOverviewItem>();
        }

        var serviceIds = services.Select(s => s.Id).ToList();

        // Pläne (Quelle der Wahrheit)
        var plans = await db.WorkingSchedulePlans
            .AsNoTracking()
            .Where(p => serviceIds.Contains(p.ServiceId))
            .OrderByDescending(p => p.ValidFromDate)
            .ToListAsync();

        // WorkingHours (plan-gebunden)
        var workingHours = await db.WorkingHours
            .AsNoTracking()
            .Where(w => serviceIds.Contains(w.ServiceId))
            .ToListAsync();

        // Overrides (plan-gebunden, optional)
        var overrides = await db.ServiceDayOverrides
            .AsNoTracking()
            .Where(o => serviceIds.Contains(o.ServiceId))
            .ToListAsync();

        // Build
        return services
            .Select(s => BuildOverviewForService(s, plans, workingHours, overrides))
            .ToList();
    }

    // --------------------------------------------------------------------
    // 2) Einzelner Service
    // --------------------------------------------------------------------
    public async Task<WorkingScheduleOverviewItem?> GetByServiceIdAsync(int serviceId)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var service = await db.Services
            .AsNoTracking()
            .Include(s => s.AssignedEmployees)
                .ThenInclude(a => a.Employee)
            .FirstOrDefaultAsync(s => s.Id == serviceId);

        if (service == null)
        {
            return null;
        }

        var plans = await db.WorkingSchedulePlans
            .AsNoTracking()
            .Where(p => p.ServiceId == serviceId)
            .OrderByDescending(p => p.ValidFromDate)
            .ToListAsync();

        var workingHours = await db.WorkingHours
            .AsNoTracking()
            .Where(w => w.ServiceId == serviceId)
            .ToListAsync();

        var overrides = await db.ServiceDayOverrides
            .AsNoTracking()
            .Where(o => o.ServiceId == serviceId)
            .ToListAsync();

        return BuildOverviewForService(service, plans, workingHours, overrides);
    }

    // --------------------------------------------------------------------
    // 3) Jahresplan löschen (PROFESSIONELL: Plan-Header löschen)
    // --------------------------------------------------------------------
    public async Task<bool> DeleteYearAsync(int serviceId, int year)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        // Wir löschen den/die Plan/Pläne des Jahres.
        // (Wenn du fachlich garantiert nur 1 Plan pro Jahr hast, ist das perfekt.)
        var plans = await db.WorkingSchedulePlans
            .Where(p =>
                p.ServiceId == serviceId &&
                p.ValidFromDate.Year == year)
            .ToListAsync();

        if (!plans.Any())
        {
            return true;
        }

        db.WorkingSchedulePlans.RemoveRange(plans);

        // Cascade (laut deinem DbContext):
        // Plan -> WorkingHours (CASCADE)
        // Plan -> ServiceDayOverrides (CASCADE)
        await db.SaveChangesAsync();
        return true;
    }

    // --------------------------------------------------------------------
    // Kern: vollständige Übersicht für einen Service erzeugen
    // --------------------------------------------------------------------
    private WorkingScheduleOverviewItem BuildOverviewForService(
        Service service,
        List<WorkingSchedulePlan> allPlans,
        List<WorkingHours> allWorkingHours,
        List<ServiceDayOverride> allOverrides)
    {
        var item = new WorkingScheduleOverviewItem
        {
            ServiceId = service.Id,
            ServiceName = service.Name,
            SlotDurationMinutes = service.SlotDurationMinutes,
            EmployeeCount = service.AssignedEmployees?.Count ?? 0,
            DefaultCapacityPerSlot = service.CapacityPerSlot ?? 0
        };

        var plans = allPlans
            .Where(p => p.ServiceId == service.Id)
            .OrderByDescending(p => p.ValidFromDate)
            .ToList();

        var yearPlans = BuildYearPlansFromPlans(
            service,
            plans,
            allWorkingHours,
            allOverrides);

        item.YearPlans.AddRange(yearPlans);
        return item;
    }

    // --------------------------------------------------------------------
    // NEU: YearPlans basierend auf WorkingSchedulePlan (nicht Overrides!)
    // --------------------------------------------------------------------
    private List<WorkingScheduleYearPlan> BuildYearPlansFromPlans(
        Service service,
        List<WorkingSchedulePlan> plans,
        List<WorkingHours> allWorkingHours,
        List<ServiceDayOverride> allOverrides)
    {
        var result = new List<WorkingScheduleYearPlan>();

        foreach (var plan in plans)
        {
            var yearPlan = new WorkingScheduleYearPlan
            {
                ServiceId = service.Id,
                WorkingSchedulePlanId = plan.Id,
                ServiceName = service.Name,

                // Wir nehmen das Jahr vom Start (dein UI arbeitet mit Jahr-Auswahl)
                Year = plan.ValidFromDate.Year,

                SlotDurationMinutes = service.SlotDurationMinutes,
                EmployeeCount = service.AssignedEmployees?.Count ?? 0,
                DefaultCapacityPerSlot = service.CapacityPerSlot ?? 0
            };

            // Monate aus Plan-Zeitraum ableiten
            yearPlan.Months = GetMonthsFromRange(plan.ValidFromDate, plan.ValidToDate);

            // Reguläre Öffnungszeiten NUR für diesen Plan
            yearPlan.RegularHours = allWorkingHours
                .Where(w =>
                    w.ServiceId == service.Id &&
                    w.WorkingSchedulePlanId == plan.Id)
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

            // Overrides NUR für diesen Plan
            var planOverrides = allOverrides
                .Where(o =>
                    o.ServiceId == service.Id &&
                    o.WorkingSchedulePlanId == plan.Id)
                .ToList();

            // Weekly Pattern Aggregation (optional)
            yearPlan.WeeklyOverrides = ExtractWeeklyPatterns(planOverrides);

            // Date Overrides (optional)
            yearPlan.DateOverrides = ExtractDateOverrides(planOverrides);

            result.Add(yearPlan);
        }

        return result;
    }

    // --------------------------------------------------------------------
    // Months aus DateOnly Range ableiten
    // --------------------------------------------------------------------
    private static List<int> GetMonthsFromRange(DateOnly from, DateOnly to)
    {
        var months = new HashSet<int>();

        var cursor = new DateOnly(from.Year, from.Month, 1);
        var end = new DateOnly(to.Year, to.Month, 1);

        while (cursor <= end)
        {
            months.Add(cursor.Month);
            cursor = cursor.AddMonths(1);
        }

        return months.OrderBy(m => m).ToList();
    }

    // --------------------------------------------------------------------
    // Weekly Patterns: gruppiert nach WeeklyDay (optional)
    // --------------------------------------------------------------------
    private static List<WeeklyOverridePatternInfo> ExtractWeeklyPatterns(List<ServiceDayOverride> overrides)
    {
        return overrides
            .Where(o => o.IsWeeklyOverride && o.WeeklyDay.HasValue)
            .GroupBy(o => o.WeeklyDay!.Value)
            .Select(g =>
            {
                var first = g.First();

                return new WeeklyOverridePatternInfo
                {
                    Day = first.WeeklyDay!.Value,
                    IsClosed = first.IsClosed,
                    StartTime = first.StartTime,
                    EndTime = first.EndTime,
                    CapacityPerSlotOverride = first.CapacityPerSlotOverride,
                    AffectedDates = g
                        .Select(x => x.Date.Date)
                        .Distinct()
                        .OrderBy(d => d)
                        .ToList()
                };
            })
            .OrderBy(x => x.Day)
            .ToList();
    }

    // --------------------------------------------------------------------
    // Date Overrides: nur IsWeeklyOverride == false (optional)
    // --------------------------------------------------------------------
    private static List<DateOverrideInfo> ExtractDateOverrides(List<ServiceDayOverride> overrides)
    {
        return overrides
            .Where(o => !o.IsWeeklyOverride)
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
    }
}
