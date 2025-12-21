namespace ConsulatTermine.Domain.Entities
{
    public class ServiceDayOverride
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public Service Service { get; set; } = null!;

        public DateTime Date { get; set; }

        /// <summary>
        /// Startzeit für den Tag — wenn null und IsClosed = true → geschlossen
        /// </summary>
        public TimeSpan? StartTime { get; set; }

        /// <summary>
        /// Endzeit für den Tag — wenn null und IsClosed = true → geschlossen
        /// </summary>
        public TimeSpan? EndTime { get; set; }

        /// <summary>
        /// Wenn true → an diesem Tag geschlossen, egal ob Zeiten gesetzt sind
        /// </summary>
        public bool IsClosed { get; set; } = false;

        /// <summary>
        /// Optional: abweichende Kapazität pro Slot an diesem Tag
        /// </summary>
        public int? CapacityPerSlotOverride { get; set; }

         public bool IsWeeklyOverride { get; set; } = false;

          public DayOfWeek? WeeklyDay { get; set; }

        public int WorkingSchedulePlanId { get; set; }
        public WorkingSchedulePlan WorkingSchedulePlan { get; set; } = null!;
        public List<ServiceDayOverride> DayOverrides { get; set; } = new();

    }
}
