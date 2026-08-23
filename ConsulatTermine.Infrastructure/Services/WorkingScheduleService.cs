using ConsulatTermine.Application.DTOs;
using ConsulatTermine.Application.DTOs.WorkingSchedulePlan;
using ConsulatTermine.Application.Interfaces;
using ConsulatTermine.Domain.Entities;
using ConsulatTermine.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConsulatTermine.Infrastructure.Services;

public class WorkingScheduleService : IWorkingScheduleService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly IWorkingSchedulePlanService _planService;

    public WorkingScheduleService(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        IWorkingSchedulePlanService planService)
    {
        _contextFactory = contextFactory;
        _planService = planService;
    }

    public async Task<bool> GenerateScheduleAsync(WorkingScheduleRequestDto request)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        // -------------------------------------------------
        // 0) VALIDIERUNG
        // -------------------------------------------------
        if (!request.Months.Any())
        {
            throw new Exception("Es müssen Monate ausgewählt sein.");
        }

        if (!request.StartTime.HasValue || !request.EndTime.HasValue)
        {
            throw new Exception("Start- und Endzeit müssen gesetzt sein.");
        }

        if (request.StartTime >= request.EndTime)
        {
            throw new Exception("Startzeit muss vor Endzeit liegen.");
        }

        // -------------------------------------------------
        // 1) SERVICE LADEN
        // -------------------------------------------------
        var service = await db.Services
            .Include(s => s.WorkingHours)
            .Include(s => s.DayOverrides)
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId);

        if (service == null)
        {
            return false;
        }

        // -------------------------------------------------
        // 2) PLAN (HEADER) SPEICHERN
        // -------------------------------------------------
        var fromMonth = request.Months.Min();
        var toMonth = request.Months.Max();
        var planDto = new WorkingSchedulePlanDto
        {
            Id = request.WorkingSchedulePlanId ?? 0, // ✅ ENTSCHEIDEND
            ServiceId = request.ServiceId,
            ValidFromDate = new DateOnly(request.Year, fromMonth, 1),
            ValidToDate = new DateOnly(
                request.Year,
                toMonth,
                DateTime.DaysInMonth(request.Year, toMonth)),
            IsActive = true
        };


        var savedPlan = await _planService.SaveAsync(planDto);

        // -------------------------------------------------
        // 3) ALTE OVERRIDES IM PLAN-ZEITRAUM LÖSCHEN
        // -------------------------------------------------
        var validFrom = savedPlan.ValidFromDate.ToDateTime(TimeOnly.MinValue);
        var validTo = savedPlan.ValidToDate.ToDateTime(TimeOnly.MaxValue);

        var overridesToDelete = service.DayOverrides
            .Where(o =>
                o.Date >= validFrom &&
                o.Date <= validTo)
            .ToList();

        if (overridesToDelete.Any())
        {
            db.ServiceDayOverrides.RemoveRange(overridesToDelete);
        }

        // -------------------------------------------------
        // 4) REGULÄRE ÖFFNUNGSZEITEN (WOCHENTAGSBASIERT)
        // -------------------------------------------------
        // ⚠️ bewusst global – keine Jahresbindung
        db.WorkingHours.RemoveRange(service.WorkingHours);

        foreach (var day in request.RegularDays)
        {
            db.WorkingHours.Add(new WorkingHours
            {
                ServiceId = request.ServiceId,
                Day = day,
                WorkingSchedulePlanId = savedPlan.Id,
                StartTime = request.StartTime.Value,
                EndTime = request.EndTime.Value
            });
        }

        // -------------------------------------------------
        // 5) WÖCHENTLICHE AUSNAHMEN → DATUMSZEILEN
        // -------------------------------------------------
        foreach (var w in request.WeeklyOverrides)
        {
            foreach (var month in request.Months)
            {
                var daysInMonth = DateTime.DaysInMonth(request.Year, month);

                for (int d = 1; d <= daysInMonth; d++)
                {
                    var date = new DateTime(request.Year, month, d);

                    if (date.DayOfWeek != w.Day)
                    {
                        continue;
                    }

                    db.ServiceDayOverrides.Add(new ServiceDayOverride
                    {
                        ServiceId = request.ServiceId,
                        Date = date,
                        WorkingSchedulePlanId = savedPlan.Id,
                        IsWeeklyOverride = true,
                        WeeklyDay = w.Day,
                        IsClosed = w.IsClosed,
                        StartTime = w.IsClosed ? null : w.StartTime,
                        EndTime = w.IsClosed ? null : w.EndTime,
                        CapacityPerSlotOverride = w.CapacityPerSlotOverride
                    });
                }
            }
        }

        // -------------------------------------------------
        // 6) DATUMS-SPEZIFISCHE AUSNAHMEN
        //    → überschreiben Weekly
        // -------------------------------------------------
        foreach (var d in request.DateOverrides)
        {
            if (!d.Date.HasValue)
            {
                continue;
            }

            var date = d.Date.Value.Date;

            if (date < validFrom || date > validTo)
            {
                continue;
            }

            var existing = db.ServiceDayOverrides
                .Where(o =>
                    o.ServiceId == request.ServiceId &&
                    o.Date == date)
                .ToList();

            if (existing.Any())
            {
                db.ServiceDayOverrides.RemoveRange(existing);
            }

            db.ServiceDayOverrides.Add(new ServiceDayOverride
            {
                ServiceId = request.ServiceId,
                Date = date,
                WorkingSchedulePlanId = savedPlan.Id,
                IsWeeklyOverride = false,
                IsClosed = d.IsClosed,
                StartTime = d.IsClosed ? null : d.StartTime,
                EndTime = d.IsClosed ? null : d.EndTime,
                CapacityPerSlotOverride = d.CapacityPerSlotOverride
            });
        }

        // -------------------------------------------------
        // 7) SPEICHERN
        // -------------------------------------------------
        await db.SaveChangesAsync();
        return true;
    }
}
