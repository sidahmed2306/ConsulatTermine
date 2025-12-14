using ConsulatTermine.Application.DTOs.Booking;
using ConsulatTermine.Application.Interfaces.Booking;

namespace ConsulatTermine.Infrastructure.Services.Booking
{
    public class BookingValidationService : IBookingValidationService
    {
        // Mindestpuffer zwischen zwei Services derselben Person (30 Minuten)
        private static readonly TimeSpan ServiceGap = TimeSpan.FromMinutes(30);

        public async Task ValidateBookingRequestAsync(CreateBookingRequestDto request)
        {
            ValidateMainPerson(request);

            ValidatePersonsBasicRules(request);

            ValidateServiceSlotCounts(request);

            ValidateNoSlotOverlapForSinglePerson(request);

            ValidateMultiPersonSlotConsistency(request);

            await Task.CompletedTask;
        }

        // ------------------------------------------------------------
        // 1) Hauptbucher validieren
        // ------------------------------------------------------------
        private void ValidateMainPerson(CreateBookingRequestDto request)
        {
            if (request.MainPerson == null)
                throw new Exception("MainPerson is required.");

            if (string.IsNullOrWhiteSpace(request.MainPerson.FullName))
                throw new Exception("MainPerson.FullName is required.");

            if (string.IsNullOrWhiteSpace(request.MainPerson.Email))
                throw new Exception("MainPerson.Email is required.");
        }

        // ------------------------------------------------------------
        // 2) Allgemeine Personenregeln prüfen
        // ------------------------------------------------------------
        private void ValidatePersonsBasicRules(CreateBookingRequestDto request)
        {
            var persons = GetAllPersons(request);

            if (!persons.Any())
                throw new Exception("Booking must contain at least one person.");

            foreach (var p in persons)
            {
                if (string.IsNullOrWhiteSpace(p.FullName))
                    throw new Exception("Every person must have a valid FullName.");

                if (!p.ServiceSlots.Any())
                    throw new Exception($"Person {p.FullName} must select at least one service slot.");
            }
        }

        // ------------------------------------------------------------
        // 3) Prüfen, ob die Anzahl Slots korrekt zu Personen passt
        // Beispiele:
        // 1 Person → pro Service genau 1 Slot
        // 3 Personen/1 Service → genau 3 Slots
        // 4 Personen/2 Services → Summe der Slots = 4 pro Service
        // ------------------------------------------------------------
        private void ValidateServiceSlotCounts(CreateBookingRequestDto request)
        {
            var persons = GetAllPersons(request);

            var groupedByService = persons
                .SelectMany(p => p.ServiceSlots.Select(s => new { p, s }))
                .GroupBy(x => x.s.ServiceId);

            foreach (var group in groupedByService)
            {
                int serviceId = group.Key;

                int totalSlots = group.Count();
                int totalPersonsUsingService = group.Select(x => x.p).Count();

                if (totalSlots != totalPersonsUsingService)
                {
                    throw new Exception(
                        $"Service {serviceId} has {totalSlots} slots but {totalPersonsUsingService} persons selected it.");
                }
            }
        }

        // ------------------------------------------------------------
        // 4) Einzelperson → mehrere Services:
        //    Die Slots dürfen sich nicht überschneiden (Startzeit),
        //    und sie müssen durch mindestens 30 Minuten getrennt sein.
        //
        // Beispiel:
        // Pass 08:45 → dauert 20–30 Minuten
        // Visa frühestens ab 09:15 / 09:30
        // ------------------------------------------------------------
        private void ValidateNoSlotOverlapForSinglePerson(CreateBookingRequestDto request)
        {
            var persons = GetAllPersons(request);
            if (persons.Count != 1)
                return;

            var single = persons.First();

            var slots = single.ServiceSlots
                .OrderBy(s => s.SlotTime)
                .ToList();

            for (int i = 0; i < slots.Count - 1; i++)
            {
                var current = slots[i];
                var next = slots[i + 1];

                if (next.SlotTime < current.SlotTime + ServiceGap)
                {
                    throw new Exception(
                        $"Service slots for the same person must be at least {ServiceGap.TotalMinutes} minutes apart.");
                }
            }
        }

        // ------------------------------------------------------------
        // 5) Mehrere Personen → mehrere Services:
        //    Prüfen, dass jede Person pro Service genau 1 Slot hat
        //    (dies wurde oben geprüft), aber zusätzlich:
        //
        //    A) SLOTS DÜRFEN SICH NICHT ZWISCHEN SERVICES FÜR DIE GLEICHE PERSON ÜBERLAPPEN
        //    B) ZEITLÜCKE WIRD EINGEHALTEN
        //
        // ------------------------------------------------------------
        private void ValidateMultiPersonSlotConsistency(CreateBookingRequestDto request)
        {
            var persons = GetAllPersons(request);

            foreach (var p in persons)
            {
                var slots = p.ServiceSlots
                    .OrderBy(s => s.SlotTime)
                    .ToList();

                for (int i = 0; i < slots.Count - 1; i++)
                {
                    var current = slots[i];
                    var next = slots[i + 1];

                    if (next.SlotTime < current.SlotTime + ServiceGap)
                    {
                        throw new Exception(
                            $"Person '{p.FullName}' has overlapping or too-close service slots.");
                    }
                }
            }
        }

        // ------------------------------------------------------------
        // Helper
        // ------------------------------------------------------------
        private List<BookingPersonDto> GetAllPersons(CreateBookingRequestDto request)
        {
            var list = new List<BookingPersonDto> { request.MainPerson };
            list.AddRange(request.AccompanyingPersons);
            return list;
        }
    }
}
