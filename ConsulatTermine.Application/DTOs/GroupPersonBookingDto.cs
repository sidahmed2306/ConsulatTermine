using System;
using System.Collections.Generic;

namespace ConsulatTermine.Application.DTOs
{
    public class GroupBookingDto
    {
        public int ServiceId { get; set; }

        /// <summary>
        /// Anzahl der Personen, die gebucht werden sollen (1–5)
        /// </summary>
        public int TotalPersons { get; set; }

        /// <summary>
        /// Liste aller Personen + deren Slot
        /// </summary>
        public List<PersonBookingDto> Persons { get; set; } = new();
    }

    public class PersonBookingDto
    {
        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// SlotStart entspricht dem vollständigen DateTime des Slots.
        /// Beispiel: 2025-02-20 09:25:00
        /// </summary>
        public DateTime SlotStart { get; set; }
    }
}
