using ConsulatTermine.Application.DTOs.Booking;
using ConsulatTermine.Application.Interfaces.Booking;
using ConsulatTermine.Domain.Entities;
using ConsulatTermine.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConsulatTermine.Infrastructure.Services.Booking
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _db;
        private readonly IBookingValidationService _validationService;
        private readonly ISlotAvailabilityService _slotService;
        private readonly IBookingReferenceGenerator _referenceGenerator;

        public BookingService(
            ApplicationDbContext db,
            IBookingValidationService validationService,
            ISlotAvailabilityService slotService,
            IBookingReferenceGenerator referenceGenerator)
        {
            _db = db;
            _validationService = validationService;
            _slotService = slotService;
            _referenceGenerator = referenceGenerator;
        }

        // --------------------------------------------------------------------
        // Haupt-Workflow: erstellt eine komplette Mehrpersonen-/Mehrservice-Buchung
        // --------------------------------------------------------------------
        public async Task<string> CreateBookingAsync(CreateBookingRequestDto request)
        {
            // 1) Buchungsreferenz erzeugen
            string bookingRef = _referenceGenerator.GenerateReference();
            request.BookingReference = bookingRef;

            // 2) Validierung (Personenregeln, Service-Regeln, Zeitüberschneidungen)
            await _validationService.ValidateBookingRequestAsync(request);

            // 3) Verfügbarkeit prüfen
            await _slotService.ValidateSlotCapacitiesAsync(request);

            // 4) Jetzt ist alles gültig → wir speichern die Termine
            using var trx = await _db.Database.BeginTransactionAsync();

            try
            {
                int personIndex = 1;

                // ---- Hauptbucher zuerst ----
                await CreateAppointmentsForPersonAsync(
                    request.MainPerson,
                    bookingRef,
                    personIndex,
                    isMainPerson: true
                );

                // ---- Begleitpersonen ----
                foreach (var acc in request.AccompanyingPersons)
                {
                    personIndex++;
                    await CreateAppointmentsForPersonAsync(
                        acc,
                        bookingRef,
                        personIndex,
                        isMainPerson: false
                    );
                }

                // Alles speichern
                await _db.SaveChangesAsync();

                await trx.CommitAsync();
            }
            catch
            {
                await trx.RollbackAsync();
                throw;
            }

            return bookingRef;
        }

        // --------------------------------------------------------------------
        // Erzeugt Appointments für eine Person (mehrere Services → mehrere Appointments)
        // --------------------------------------------------------------------
        private async Task CreateAppointmentsForPersonAsync(
            BookingPersonDto person,
            string bookingRef,
            int personIndex,
            bool isMainPerson)
        {
            foreach (var serviceSlot in person.ServiceSlots)
            {
                var appointment = new Appointment
                {
                    FullName = person.FullName,
                    Email = isMainPerson ? person.Email ?? "" : "",
                    Date = serviceSlot.SlotTime,
                    ServiceId = serviceSlot.ServiceId,
                    Status = Domain.Enums.AppointmentStatus.Booked,
                    BookingReference = bookingRef,
                    PersonIndex = personIndex,
                    IsMainPerson = isMainPerson,
                    CreatedAt = DateTime.UtcNow
                };

                _db.Appointments.Add(appointment);
            }
        }
    }
}
