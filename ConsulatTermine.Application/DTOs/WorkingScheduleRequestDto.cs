using System;
using System.Collections.Generic;

namespace ConsulatTermine.Application.DTOs
{
    public class WorkingScheduleRequestDto
    {
        public int ServiceId { get; set; }

        public int Year { get; set; }

        // Beispiel: {1,2,3,4} für Januar–April
        public HashSet<int> Months { get; set; } = new();


        // Welche Wochentage sollen regulär offen sein?
        public HashSet<DayOfWeek> RegularDays { get; set; } = new();

        // Reguläre Öffnungszeiten
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }

        // Ausnahmen: Wochentage (z.B. jeden Samstag halbtags)
        public List<WeeklyOverrideDto> WeeklyOverrides { get; set; } = new();

        // Ausnahmen für einzelne Datumsangaben
        public List<DateOverrideDto> DateOverrides { get; set; } = new();
    }

    public class WeeklyOverrideDto
    {
        public DayOfWeek Day { get; set; }

        public bool IsClosed { get; set; }

        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }

        public int? CapacityPerSlotOverride { get; set; }
    }

    public class DateOverrideDto
    {
        public DateTime? Date { get; set; }

        public bool IsClosed { get; set; }

        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }

        public int? CapacityPerSlotOverride { get; set; }
    }
}
