using System.Collections.Generic;
using System.Threading.Tasks;
using ConsulatTermine.Application.ViewModels;

namespace ConsulatTermine.Application.Interfaces
{
    public interface IWorkingScheduleOverviewService
    {
        /// <summary>
        /// Liefert eine vollständige Übersicht aller Services 
        /// und deren Jahrespläne (Variante A: gruppiert pro Jahr).
        /// </summary>
        Task<List<WorkingScheduleOverviewItem>> GetOverviewAsync();

        /// <summary>
        /// Liefert die Übersicht für genau einen Service.
        /// </summary>
        Task<WorkingScheduleOverviewItem?> GetByServiceIdAsync(int serviceId);

        /// <summary>
        /// Löscht einen gesamten Jahresplan:
        /// alle WorkingHours und DayOverrides für (ServiceId + Year).
        /// </summary>
        Task<bool> DeleteYearAsync(int serviceId, int year);
    }
}
