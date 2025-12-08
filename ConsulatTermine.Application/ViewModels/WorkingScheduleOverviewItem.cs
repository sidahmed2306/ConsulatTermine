using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsulatTermine.Application.ViewModels
{
    /// <summary>
    /// Übersicht pro Service: alle Jahrespläne inklusive Kapazitäten.
    /// Wird direkt im Admin-UI angezeigt.
    /// </summary>
    public class WorkingScheduleOverviewItem
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;

        /// <summary>
        /// Standard-Kapazität pro Slot aus Service.CapacityPerSlot.
        /// </summary>
        public int DefaultCapacityPerSlot { get; set; }

        /// <summary>
        /// Slot-Dauer (Minuten) aus Service.SlotDurationMinutes.
        /// </summary>
        public int SlotDurationMinutes { get; set; }

        /// <summary>
        /// Anzahl der Mitarbeiter, die diesem Service zugeordnet sind.
        /// </summary>
        public int EmployeeCount { get; set; }

        /// <summary>
        /// Jahrespläne (Variante A: gruppiert pro Jahr).
        /// </summary>
        public List<WorkingScheduleYearPlan> YearPlans { get; set; } = new();
    }

    /// <summary>
    /// Ein Plan für ein bestimmtes Jahr eines Services.
    /// Beinhaltet Monate, reguläre Öffnungszeiten, Ausnahmen und Kapazitätsdaten.
    /// </summary>
    public class WorkingScheduleYearPlan
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;

        public int Year { get; set; }

        /// <summary>
        /// Alle Monate (1–12), in denen es Overrides in diesem Jahr gibt.
        /// </summary>
        public List<int> Months { get; set; } = new();

        /// <summary>
        /// Reguläre Öffnungszeiten pro Wochentag (aus WorkingHours).
        /// </summary>
        public List<RegularOpeningInfo> RegularHours { get; set; } = new();

        /// <summary>
        /// Wöchentliche Ausnahmen (Pattern), z. B. "jeden Samstag geschlossen".
        /// </summary>
        public List<WeeklyOverridePatternInfo> WeeklyOverrides { get; set; } = new();

        /// <summary>
        /// Datumsspezifische Ausnahmen, z. B. einzelne Feiertage.
        /// </summary>
        public List<DateOverrideInfo> DateOverrides { get; set; } = new();

        /// <summary>
        /// Standard-Kapazität pro Slot in diesem Jahr (aus Service).
        /// </summary>
        public int DefaultCapacityPerSlot { get; set; }

        /// <summary>
        /// Slot-Dauer in Minuten (aus Service).
        /// </summary>
        public int SlotDurationMinutes { get; set; }

        /// <summary>
        /// Anzahl Mitarbeiter, die diesem Service zugeordnet sind.
        /// </summary>
        public int EmployeeCount { get; set; }

        /// <summary>
        /// Abgeleitet: alle Tage, an denen regulär geöffnet ist.
        /// Praktisch für Mapping zurück auf WorkingScheduleRequestDto.
        /// </summary>
        public HashSet<DayOfWeek> RegularDays =>
            RegularHours.Select(r => r.Day).ToHashSet();

        /// <summary>
        /// Abgeleitet: gemeinsame Startzeit aller regulären Tage (falls konsistent).
        /// Wird für das Befüllen von WorkingScheduleRequestDto verwendet.
        /// </summary>
        public TimeSpan? RegularStartTime =>
            RegularHours.FirstOrDefault()?.StartTime;

        /// <summary>
        /// Abgeleitet: gemeinsame Endzeit aller regulären Tage (falls konsistent).
        /// </summary>
        public TimeSpan? RegularEndTime =>
            RegularHours.FirstOrDefault()?.EndTime;
    }

    /// <summary>
    /// Darstellung einer regulären Öffnungszeit an einem bestimmten Wochentag.
    /// Entspricht einem Eintrag aus WorkingHours.
    /// </summary>
    public class RegularOpeningInfo
    {
        public DayOfWeek Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public override string ToString()
            => $"{Day}: {StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
    }

    /// <summary>
    /// Aggregiertes Muster für eine wöchentliche Ausnahme:
    /// z. B. "jeden Samstag geschlossen" oder "jeden Freitag 09–12 Uhr mit anderer Kapazität".
    /// </summary>
    public class WeeklyOverridePatternInfo
    {
        public DayOfWeek Day { get; set; }

        /// <summary>
        /// True, wenn der Tag im Muster grundsätzlich geschlossen ist.
        /// </summary>
        public bool IsClosed { get; set; }

        /// <summary>
        /// Abweichende Startzeit für diesen Wochentag (falls nicht geschlossen).
        /// </summary>
        public TimeSpan? StartTime { get; set; }

        /// <summary>
        /// Abweichende Endzeit für diesen Wochentag (falls nicht geschlossen).
        /// </summary>
        public TimeSpan? EndTime { get; set; }

        /// <summary>
        /// Optionale Kapazitäts-Override pro Slot für diesen Wochentag.
        /// </summary>
        public int? CapacityPerSlotOverride { get; set; }

        /// <summary>
        /// Liste aller konkreten Daten in diesem Jahr, die zu diesem Pattern gehören.
        /// Wird für Anzeige/Summary verwendet.
        /// </summary>
        public List<DateTime> AffectedDates { get; set; } = new();
    }

    /// <summary>
    /// Konkrete Ausnahme für ein einzelnes Datum.
    /// Entspricht einem konkreten ServiceDayOverride.
    /// </summary>
    public class DateOverrideInfo
    {
        public DateTime Date { get; set; }

        public bool IsClosed { get; set; }

        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }

        public int? CapacityPerSlotOverride { get; set; }

        public override string ToString()
        {
            if (IsClosed)
                return $"{Date:dd.MM.yyyy}: Geschlossen";

            var timePart = (StartTime.HasValue && EndTime.HasValue)
                ? $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}"
                : "Zeiten nicht definiert";

            var cap = CapacityPerSlotOverride.HasValue
                ? $" (Kapazität: {CapacityPerSlotOverride})"
                : string.Empty;

            return $"{Date:dd.MM.yyyy}: {timePart}{cap}";
        }
    }
}
