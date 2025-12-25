using ConsulatTermine.Application.DTOs.Booking;
using ConsulatTermine.Application.Interfaces;
using ConsulatTermine.Application.Interfaces.Booking;
using ConsulatTermine.Domain.Entities;
using ConsulatTermine.Domain.Enums;
using ConsulatTermine.Infrastructure.Persistence;

namespace ConsulatTermine.Infrastructure.Services.Booking
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _db;
        private readonly IBookingValidationService _validationService;
        private readonly ISlotAvailabilityService _slotService;
        private readonly IBookingReferenceGenerator _referenceGenerator;
        private readonly IEmailService _emailService;

        public BookingService(
            ApplicationDbContext db,
            IBookingValidationService validationService,
            ISlotAvailabilityService slotService,
            IBookingReferenceGenerator referenceGenerator,
            IEmailService emailService)
        {
            _db = db;
            _validationService = validationService;
            _slotService = slotService;
            _referenceGenerator = referenceGenerator;
            _emailService = emailService;
        }

        // --------------------------------------------------------------------
        // Haupt-Workflow: erstellt eine komplette Mehrpersonen-/Mehrservice-Buchung
        // --------------------------------------------------------------------
        public async Task<string> CreateBookingAsync(CreateBookingRequestDto request)
        {
            // 1) Buchungsreferenz erzeugen
            string bookingRef = _referenceGenerator.GenerateReference();
            request.BookingReference = bookingRef;

            // 🔐 EIN Cancel-Token für die gesamte Buchung
            string cancelToken = Guid.NewGuid().ToString("N");

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
                    cancelToken,
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
                        cancelToken,
                        personIndex,
                        isMainPerson: false
                    );
                }

                // Alles speichern
                await _db.SaveChangesAsync();
                await trx.CommitAsync();

                // 📧 E-Mail asynchron (darf Buchung nicht blockieren)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(request.MainPerson.Email))
                        {
                            await _emailService.SendBookingConfirmationAsync(
                                request.MainPerson.Email,
                                request.MainPerson.FullName,
                                bookingRef, cancelToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("EMAIL ERROR (ignored):");
                        Console.WriteLine(ex.ToString());
                    }
                });
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
            string cancelToken,
            int personIndex,
            bool isMainPerson)
        {
            foreach (var serviceSlot in person.ServiceSlots)
            {
                // 🔐 Absage nur bis 24h vor Termin erlaubt
                var cancelDeadlineUtc = serviceSlot.SlotTime
                    .AddHours(-24)
                    .ToUniversalTime();

                if (cancelDeadlineUtc < DateTime.UtcNow)
                {
                    cancelDeadlineUtc = DateTime.UtcNow;
                }

                var appointment = new Appointment
                {
                    FullName = person.FullName,
                    Email = isMainPerson ? person.Email ?? string.Empty : string.Empty,
                    PhoneNumber = person.PhoneNumber ?? string.Empty,
                    DateOfBirth = person.DateOfBirth,

                    Date = serviceSlot.SlotTime,
                    ServiceId = serviceSlot.ServiceId,
                    Status = AppointmentStatus.Booked,

                    BookingReference = bookingRef,
                    PersonIndex = personIndex,
                    IsMainPerson = isMainPerson,
                    CreatedAt = DateTime.UtcNow,

                    // 🔐 Cancel-Link-Sicherheit
                    CancelToken = cancelToken,
                    CancelTokenExpiresAt = cancelDeadlineUtc
                };

                _db.Appointments.Add(appointment);
            }

            await Task.CompletedTask;
        }
    }
}
