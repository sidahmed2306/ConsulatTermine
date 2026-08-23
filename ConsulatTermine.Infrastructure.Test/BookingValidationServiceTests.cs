using ConsulatTermine.Application.DTOs.Booking;
using ConsulatTermine.Application.Exceptions;
using ConsulatTermine.Infrastructure.Services.Booking;

namespace ConsulatTermine.Infrastructure.Test;

/// <summary>
/// Tests der Buchungsvalidierung fuer Mehrpersonen- und Mehrservice-Buchungen.
/// </summary>
public sealed class BookingValidationServiceTests
{
    private static readonly DateTime Monday = new(2026, 8, 24, 0, 0, 0, DateTimeKind.Unspecified);

    private readonly BookingValidationService _service = new();

    private static BookingServiceSlotDto Slot(int serviceId, string time) => new()
    {
        ServiceId = serviceId,
        SlotTime = Monday.Add(TimeSpan.Parse(time, System.Globalization.CultureInfo.InvariantCulture))
    };

    private static BookingPersonDto Person(string name, string? email, params BookingServiceSlotDto[] slots) => new()
    {
        FullName = name,
        Email = email,
        ServiceSlots = [.. slots]
    };

    private static CreateBookingRequestDto Request(
        BookingPersonDto? main,
        params BookingPersonDto[] accompanying) => new()
        {
            MainPerson = main!,
            AccompanyingPersons = [.. accompanying]
        };

    [Fact]
    public async Task ValidateBookingRequestAsync_MitGueltigerEinzelbuchung_LaeuftDurch()
    {
        var request = Request(Person("Amina Benali", "amina@example.org", Slot(1, "09:00")));

        await _service.ValidateBookingRequestAsync(request);
    }

    [Fact]
    public async Task ValidateBookingRequestAsync_OhneHauptperson_WirdAbgewiesen()
    {
        await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => _service.ValidateBookingRequestAsync(Request(main: null)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ValidateBookingRequestAsync_OhneNamenDerHauptperson_WirdAbgewiesen(string name)
    {
        var request = Request(Person(name, "amina@example.org", Slot(1, "09:00")));

        await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => _service.ValidateBookingRequestAsync(request));
    }

    [Fact]
    public async Task ValidateBookingRequestAsync_OhneEmailDerHauptperson_WirdAbgewiesen()
    {
        // Die Bestaetigung samt Absage-Link geht ausschliesslich an die Hauptperson.
        var request = Request(Person("Amina Benali", null, Slot(1, "09:00")));

        await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => _service.ValidateBookingRequestAsync(request));
    }

    [Fact]
    public async Task ValidateBookingRequestAsync_WennEineBegleitpersonKeinenNamenHat_WirdAbgewiesen()
    {
        var request = Request(
            Person("Amina Benali", "amina@example.org", Slot(1, "09:00")),
            Person("  ", null, Slot(1, "09:30")));

        await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => _service.ValidateBookingRequestAsync(request));
    }

    [Fact]
    public async Task ValidateBookingRequestAsync_WennEinePersonKeinenTerminWaehlt_WirdAbgewiesen()
    {
        var request = Request(
            Person("Amina Benali", "amina@example.org", Slot(1, "09:00")),
            Person("Karim Haddad", null));

        await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => _service.ValidateBookingRequestAsync(request));
    }

    [Fact]
    public async Task ValidateBookingRequestAsync_MitZweiTerminenDerselbenPersonImSelbenService_WirdAbgewiesen()
    {
        // Regressionstest: die Pruefung zaehlte frueher Eintraege statt Personen und
        // lief dadurch immer leer.
        var request = Request(
            Person("Amina Benali", "amina@example.org", Slot(1, "09:00"), Slot(1, "11:00")));

        await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => _service.ValidateBookingRequestAsync(request));
    }

    [Fact]
    public async Task ValidateBookingRequestAsync_EinePersonMitZweiServicesUndAusreichendemAbstand_LaeuftDurch()
    {
        // Der Mindestabstand wird von Startzeit zu Startzeit gemessen: 30 Minuten genügen.
        var request = Request(
            Person("Amina Benali", "amina@example.org", Slot(1, "09:00"), Slot(2, "09:30")));

        await _service.ValidateBookingRequestAsync(request);
    }

    [Fact]
    public async Task ValidateBookingRequestAsync_EinePersonMitZweiZuDichtLiegendenServices_WirdAbgewiesen()
    {
        var request = Request(
            Person("Amina Benali", "amina@example.org", Slot(1, "09:00"), Slot(2, "09:20")));

        await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => _service.ValidateBookingRequestAsync(request));
    }

    [Fact]
    public async Task ValidateBookingRequestAsync_EinePersonMitZuGeringemAbstandZwischenServices_WirdAbgewiesen()
    {
        // Nur 25 Minuten Abstand: der Mindestabstand von 30 Minuten ist verletzt.
        var request = Request(
            Person("Amina Benali", "amina@example.org", Slot(1, "09:00"), Slot(2, "09:25")));

        await Assert.ThrowsAsync<BusinessRuleViolationException>(
            () => _service.ValidateBookingRequestAsync(request));
    }

    [Fact]
    public async Task ValidateBookingRequestAsync_MehrerePersonenMitEigenenTerminen_LaeuftDurch()
    {
        var request = Request(
            Person("Amina Benali", "amina@example.org", Slot(1, "09:00")),
            Person("Karim Haddad", null, Slot(1, "09:30")),
            Person("Nadia Cherif", null, Slot(1, "10:00")));

        await _service.ValidateBookingRequestAsync(request);
    }
}
