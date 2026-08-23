using ConsulatTermine.Application.DTOs.Booking;
using ConsulatTermine.Application.Exceptions;
using ConsulatTermine.Application.Interfaces.Booking;

namespace ConsulatTermine.Infrastructure.Services.Booking;

public class BookingValidationService : IBookingValidationService
{
    // Mindestpuffer zwischen zwei Services derselben Person (30 Minuten)
    private static readonly TimeSpan _serviceGap = TimeSpan.FromMinutes(30);

    public async Task ValidateBookingRequestAsync(CreateBookingRequestDto request)
    {
        ValidateMainPerson(request);

        ValidatePersonsBasicRules(request);

        ValidateServiceSlotCounts(request);

        ValidateMultiPersonSlotConsistency(request);

        await Task.CompletedTask;
    }

    // ------------------------------------------------------------
    // 1) Hauptbucher validieren
    // ------------------------------------------------------------
    private static void ValidateMainPerson(CreateBookingRequestDto request)
    {
        if (request.MainPerson == null)
        {
            throw new BusinessRuleViolationException("Es muss eine Hauptperson angegeben werden.");
        }

        if (string.IsNullOrWhiteSpace(request.MainPerson.FullName))
        {
            throw new BusinessRuleViolationException("Der Name der Hauptperson ist erforderlich.");
        }

        if (string.IsNullOrWhiteSpace(request.MainPerson.Email))
        {
            throw new BusinessRuleViolationException("Die E-Mail-Adresse der Hauptperson ist erforderlich.");
        }
    }

    // ------------------------------------------------------------
    // 2) Allgemeine Personenregeln prüfen
    // ------------------------------------------------------------
    private static void ValidatePersonsBasicRules(CreateBookingRequestDto request)
    {
        var persons = GetAllPersons(request);

        if (persons.Count == 0)
        {
            throw new BusinessRuleViolationException("Eine Buchung muss mindestens eine Person enthalten.");
        }

        foreach (var p in persons)
        {
            if (string.IsNullOrWhiteSpace(p.FullName))
            {
                throw new BusinessRuleViolationException("Für jede Person ist ein Name erforderlich.");
            }

            if (p.ServiceSlots.Count == 0)
            {
                throw new BusinessRuleViolationException($"Für {p.FullName} muss mindestens ein Termin gewählt werden.");
            }
        }
    }

    // ------------------------------------------------------------
    // 3) Prüfen, ob die Anzahl Slots korrekt zu Personen passt
    // Beispiele:
    // 1 Person → pro Service genau 1 Slot
    // 3 Personen/1 Service → genau 3 Slots
    // 4 Personen/2 Services → Summe der Slots = 4 pro Service
    // ------------------------------------------------------------
    private static void ValidateServiceSlotCounts(CreateBookingRequestDto request)
    {
        var persons = GetAllPersons(request);

        var groupedByService = persons
            .SelectMany(p => p.ServiceSlots.Select(s => new { p, s }))
            .GroupBy(x => x.s.ServiceId);

        foreach (var group in groupedByService)
        {
            var totalSlots = group.Count();

            // Distinct auf der Person: ohne das zaehlte der Ausdruck die Eintraege der
            // Gruppe und war damit immer gleich totalSlots, die Pruefung lief also leer.
            // Fachlich waehlt jede Person einen Service hoechstens einmal.
            var personsUsingService = group.Select(x => x.p).Distinct().Count();

            if (totalSlots != personsUsingService)
            {
                throw new BusinessRuleViolationException(
                    "Für einen Service wurden mehr Termine gewählt als Personen daran teilnehmen.");
            }

        }
    }

    // ------------------------------------------------------------
    // 4) Jede Person darf ihre Services nur mit Mindestabstand belegen.
    //
    //    Gemessen wird von Startzeit zu Startzeit: der Abstand deckt damit die Dauer
    //    des vorangehenden Termins und die Wegezeit zwischen zwei Schaltern ab.
    //    Beispiel: Pass 08:45, Visa fruehestens 09:15.
    //
    // ------------------------------------------------------------
    private static void ValidateMultiPersonSlotConsistency(CreateBookingRequestDto request)
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

                if (next.SlotTime < current.SlotTime + _serviceGap)
                {
                    throw new BusinessRuleViolationException(
                        $"Zwischen zwei Terminen von {p.FullName} müssen mindestens "
                        + $"{_serviceGap.TotalMinutes:0} Minuten liegen.");
                }
            }
        }
    }

    // ------------------------------------------------------------
    // Helper
    // ------------------------------------------------------------
    private static List<BookingPersonDto> GetAllPersons(CreateBookingRequestDto request)
    {
        var list = new List<BookingPersonDto> { request.MainPerson };
        list.AddRange(request.AccompanyingPersons);
        return list;
    }
}
