using ConsulatTermine.Application.DTOs.Booking;
using ConsulatTermine.Application.Interfaces;
using ConsulatTermine.Application.Interfaces.Booking;
using ConsulatTermine.Application.Localization;
using ConsulatTermine.Domain.Entities;
using ConsulatTermine.Domain.Enums;
using ConsulatTermine.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ConsulatTermine.Infrastructure.Services.Booking;

public class BookingService : IBookingService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly IBookingValidationService _validationService;
    private readonly ISlotAvailabilityService _slotService;
    private readonly IBookingReferenceGenerator _referenceGenerator;
    private readonly IEmailService _emailService;
    private readonly ILogger<BookingService> _logger;

    public BookingService(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        IBookingValidationService validationService,
        ISlotAvailabilityService slotService,
        IBookingReferenceGenerator referenceGenerator,
        IEmailService emailService,
        ILogger<BookingService> logger)
    {
        _contextFactory = contextFactory;
        _validationService = validationService;
        _slotService = slotService;
        _referenceGenerator = referenceGenerator;
        _emailService = emailService;
        _logger = logger;
    }

    // --------------------------------------------------------------------
    // Haupt-Workflow: erstellt eine komplette Mehrpersonen-/Mehrservice-Buchung
    // --------------------------------------------------------------------
    public async Task<string> CreateBookingAsync(CreateBookingRequestDto request)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        // 1) Buchungsreferenz erzeugen
        string bookingRef = _referenceGenerator.GenerateReference();
        request.BookingReference = bookingRef;

        // ✅ NEU: Wir sammeln alle Termine für die Bestätigungs-E-Mail
        var emailAppointments = new List<BookingEmailAppointmentDto>();

        // 🔐 EIN Cancel-Token für die gesamte Buchung
        string cancelToken = Guid.NewGuid().ToString("N");

        // 2) Validierung (Personenregeln, Service-Regeln, Zeitüberschneidungen)
        await _validationService.ValidateBookingRequestAsync(request);

        // 3) Verfügbarkeit prüfen
        await _slotService.ValidateSlotCapacitiesAsync(request);

        // 4) Jetzt ist alles gültig → wir speichern die Termine
        using var trx = await db.Database.BeginTransactionAsync();

        try
        {
            int personIndex = 1;

            // Unbekannte oder fehlende Angaben ergeben die Standardsprache. Die Sprache
            // wird je Termin gespeichert, damit auch eine spaetere Absage in der Sprache
            // hinausgeht, in der gebucht wurde.
            var bookingLanguage = SupportedLanguages.Normalize(request.Language);

            // ---- Hauptbucher zuerst ----
            await CreateAppointmentsForPersonAsync(
                db,
                request.MainPerson,
                bookingRef,
                cancelToken,
                personIndex,
                true,
                bookingLanguage,
                emailAppointments);

            // ---- Begleitpersonen ----
            foreach (var acc in request.AccompanyingPersons)
            {
                personIndex++;
                await CreateAppointmentsForPersonAsync(
                    db,
                    acc,
                    bookingRef,
                    cancelToken,
                    personIndex,
                    false,
                    bookingLanguage,
                    emailAppointments);
            }

            // Alles speichern
            await db.SaveChangesAsync();
            await trx.CommitAsync();

            // 📧 E-Mail asynchron (darf Buchung nicht blockieren)
            _ = Task.Run(async () =>
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(request.MainPerson.Email))
                    {
                        // ✅ FIX: Hier NICHT nochmal Appointments erzeugen!
                        // ✅ Stattdessen: Bestätigungsmail inkl. Terminübersicht senden
                        await _emailService.SendBookingConfirmationAsync(
                            request.MainPerson.Email,
                            request.MainPerson.FullName,
                            bookingRef,
                            cancelToken,
                            emailAppointments,
                            bookingLanguage);
                    }
                }
                catch (Exception ex)
                {
                    // Die Buchung ist gespeichert und bleibt gültig, auch wenn die
                    // Bestätigungsmail scheitert. Der Fehler wird protokolliert.
                    ServiceLog.ConfirmationMailFailed(_logger, ex, bookingRef);
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
    /// <summary>
    /// Legt die Termine einer Person an. Arbeitet im Kontext des Aufrufers, damit alle
    /// Termine einer Buchung gemeinsam gespeichert werden oder gar nicht.
    /// </summary>
    private static async Task CreateAppointmentsForPersonAsync(
        ApplicationDbContext db,
        BookingPersonDto person,
        string bookingRef,
        string cancelToken,
        int personIndex,
        bool isMainPerson,
        string bookingLanguage,
        List<BookingEmailAppointmentDto> emailAppointments)
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
                Language = bookingLanguage,
                CreatedAt = DateTime.UtcNow,

                // 🔐 Cancel-Link-Sicherheit
                CancelToken = cancelToken,
                CancelTokenExpiresAt = cancelDeadlineUtc
            };

            // ✅ NEU: Daten für die E-Mail sammeln
            // Alle gepflegten Sprachfassungen werden mitgenommen: welche davon im
            // Schreiben erscheint, entscheidet erst der Versand anhand der Sprache
            // der Buchung.
            var serviceNames = db.Services
                .Where(s => s.Id == serviceSlot.ServiceId)
                .Select(s => new { s.Name, s.NameEnglish, s.NameArabic })
                .First();

            emailAppointments.Add(new BookingEmailAppointmentDto
            {
                PersonFullName = person.FullName,
                ServiceName = serviceNames.Name,
                ServiceNameEnglish = serviceNames.NameEnglish,
                ServiceNameArabic = serviceNames.NameArabic,
                DateTime = serviceSlot.SlotTime
            });

            db.Appointments.Add(appointment);
        }

        await Task.CompletedTask;
    }
}
