using ConsulatTermine.Application.DTOs;
using ConsulatTermine.Application.Interfaces;
using ConsulatTermine.Domain.Entities;
using ConsulatTermine.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConsulatTermine.Infrastructure.Services
{
    public class WorkingScheduleService : IWorkingScheduleService
    {
        private readonly ApplicationDbContext _context;

        public WorkingScheduleService(ApplicationDbContext context)
        {
            _context = context;
        }

public async Task<bool> GenerateScheduleAsync(WorkingScheduleRequestDto request)
{
    // Service inkl. vorhandener Daten laden
    var service = await _context.Services
        .Include(s => s.WorkingHours)
        .Include(s => s.DayOverrides)
        .FirstOrDefaultAsync(s => s.Id == request.ServiceId);

    if (service == null)
        return false;

    var year = request.Year;

    // ------------------------------
    // 1) ALTE DATEN LÖSCHEN
    // ------------------------------

    // Alle WorkingHours dieses Services entfernen
    var whToDelete = service.WorkingHours.ToList();
    if (whToDelete.Any())
        _context.WorkingHours.RemoveRange(whToDelete);

    // Alle Overrides dieses Jahres für diesen Service entfernen
    var ovToDelete = service.DayOverrides
        .Where(o => o.Date.Year == year)
        .ToList();
    if (ovToDelete.Any())
        _context.ServiceDayOverrides.RemoveRange(ovToDelete);

    await _context.SaveChangesAsync();

    // ------------------------------
    // 2) REGELMÄSSIGE ÖFFNUNGSZEITEN
    //    -> EIN Eintrag pro Wochentag
    // ------------------------------
    if (!request.StartTime.HasValue || !request.EndTime.HasValue)
        throw new Exception("Start- und Endzeit müssen gesetzt sein.");

    foreach (var day in request.RegularDays)
    {
        _context.WorkingHours.Add(new WorkingHours
        {
            ServiceId = request.ServiceId,
            Day = day,
            StartTime = request.StartTime.Value,
            EndTime = request.EndTime.Value
        });
    }

    // ------------------------------
    // 3) WÖCHENTLICHE AUSNAHMEN
    //    -> Ein Override pro echtem Datum (z.B. jeder Samstag),
    //       gekennzeichnet mit IsWeeklyOverride = true
    // ------------------------------
    // 3) WÖCHENTLICHE AUSNAHMEN
if (request.Months != null && request.Months.Any())
{
    foreach (var w in request.WeeklyOverrides)
    {
        foreach (var month in request.Months)
        {
            var daysInMonth = DateTime.DaysInMonth(year, month);

            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(year, month, day);

                if (date.DayOfWeek != w.Day)
                    continue;

                _context.ServiceDayOverrides.Add(new ServiceDayOverride
                {
                    ServiceId = request.ServiceId,
                    Date = date,
                    IsWeeklyOverride = true,
                    WeeklyDay = w.Day,   // <── WICHTIG! FEHLTE BEI DIR
                    IsClosed = w.IsClosed,
                    StartTime = w.IsClosed ? null : w.StartTime,
                    EndTime = w.IsClosed ? null : w.EndTime,
                    CapacityPerSlotOverride = w.CapacityPerSlotOverride
                });
            }
        }
    }
}


    // ------------------------------
    // 4) DATUMSSPEZIFISCHE AUSNAHMEN
    //    -> überschreiben ggf. WeeklyOverrides an diesem Tag
    // ------------------------------
    foreach (var d in request.DateOverrides)
    {
        if (!d.Date.HasValue)
            continue;

        var date = d.Date.Value.Date;

        // Nur das konfigurierte Jahr berücksichtigen
        if (date.Year != year)
            continue;

        // Falls für dieses Datum bereits Weekly-Overrides existieren:
        // diese zuerst entfernen, damit die Datumsausnahme gewinnt.
        var existingForDate = _context.ServiceDayOverrides
            .Where(o => o.ServiceId == request.ServiceId && o.Date == date)
            .ToList();

        if (existingForDate.Any())
            _context.ServiceDayOverrides.RemoveRange(existingForDate);

        _context.ServiceDayOverrides.Add(new ServiceDayOverride
        {
            ServiceId = request.ServiceId,
            Date = date,
            IsWeeklyOverride = false,
            IsClosed = d.IsClosed,
            StartTime = d.IsClosed ? null : d.StartTime,
            EndTime = d.IsClosed ? null : d.EndTime,
            CapacityPerSlotOverride = d.CapacityPerSlotOverride
        });
    }

    // ------------------------------
    // 5) SPEICHERN
    // ------------------------------
    await _context.SaveChangesAsync();
    return true;
}




    }
}
