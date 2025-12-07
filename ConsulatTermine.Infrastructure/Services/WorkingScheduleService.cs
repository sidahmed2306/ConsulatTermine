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
            var serviceExists = await _context.Services.AnyAsync(s => s.Id == request.ServiceId);
            if (!serviceExists)
                return false;

            // REGELN ERZEUGEN
            foreach (var month in request.Months)
            {
                var daysInMonth = DateTime.DaysInMonth(request.Year, month);

                for (int day = 1; day <= daysInMonth; day++)
                {
                    var date = new DateTime(request.Year, month, day);
                    var dow = date.DayOfWeek;

                    if (request.RegularDays.Contains(dow))
                    {
                        var wh = new WorkingHours
                        {
                            ServiceId = request.ServiceId,
                            Day = dow,
                            StartTime = request.StartTime!.Value,
                            EndTime = request.EndTime!.Value
                        };

                        _context.WorkingHours.Add(wh);
                    }
                }
            }

            // WÖCHENTLICHE AUSNAHMEN
            foreach (var w in request.WeeklyOverrides)
            {
                foreach (var month in request.Months)
                {
                    var daysInMonth = DateTime.DaysInMonth(request.Year, month);

                    for (int day = 1; day <= daysInMonth; day++)
                    {
                        var date = new DateTime(request.Year, month, day);

                        if (date.DayOfWeek == w.Day)
                        {
                            var ov = new ServiceDayOverride
                            {
                                ServiceId = request.ServiceId,
                                Date = date,
                                IsClosed = w.IsClosed,
                                StartTime = w.StartTime,
                                EndTime = w.EndTime,
                                CapacityPerSlotOverride = w.CapacityPerSlotOverride
                            };

                            _context.ServiceDayOverrides.Add(ov);
                        }
                    }
                }
            }

            // AUSNAHMEN FÜR EINZELNE TAGE
            foreach (var o in request.DateOverrides)
            {
                var ov = new ServiceDayOverride
                {
                    ServiceId = request.ServiceId,
                    Date = o.Date!.Value,
                    IsClosed = o.IsClosed,
                    StartTime = o.StartTime,
                    EndTime = o.EndTime,
                    CapacityPerSlotOverride = o.CapacityPerSlotOverride
                };

                _context.ServiceDayOverrides.Add(ov);
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
