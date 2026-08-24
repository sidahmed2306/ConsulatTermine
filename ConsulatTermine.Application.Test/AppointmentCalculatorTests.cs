using System.Globalization;
using ConsulatTermine.Application.DTOs;
using ConsulatTermine.Application.Services;
using ConsulatTermine.Domain.Entities;
using ConsulatTermine.Domain.Enums;

namespace ConsulatTermine.Application.Test;

/// <summary>
/// Tests der Slot-Berechnung. Die fachliche Rangfolge lautet:
/// Override auf ein Datum &gt; Override auf einen Wochentag &gt; regulaere Oeffnungszeit.
/// </summary>
public sealed class AppointmentCalculatorTests
{
    /// <summary>Montag, damit der Wochentag in den Tests eindeutig ist.</summary>
    private static readonly DateTime Monday = new(2026, 8, 24, 0, 0, 0, DateTimeKind.Unspecified);

    private static Service ServiceWithSlotDuration(int minutes, int employeeCount = 1)
    {
        var service = new Service { Id = 1, SlotDurationMinutes = minutes };

        for (var i = 0; i < employeeCount; i++)
        {
            service.AssignedEmployees.Add(new EmployeeServiceAssignment { EmployeeId = i + 1, ServiceId = 1 });
        }

        return service;
    }

    private static List<WorkingHours> OpeningHours(DayOfWeek day, string from, string to) =>
    [
        new WorkingHours
        {
            ServiceId = 1,
            Day = day,
            StartTime = TimeSpan.Parse(from, CultureInfo.InvariantCulture),
            EndTime = TimeSpan.Parse(to, CultureInfo.InvariantCulture)
        }
    ];

    [Fact]
    public void GetDailySlots_MitRegulaererOeffnungszeit_TeiltDenZeitraumVollstaendigAuf()
    {
        var slots = AppointmentCalculator.GetDailySlots(
            ServiceWithSlotDuration(30),
            Monday,
            OpeningHours(DayOfWeek.Monday, "09:00", "11:00"),
            []);

        Assert.Equal(4, slots.Count);
        Assert.Equal(TimeSpan.Parse("09:00", CultureInfo.InvariantCulture), slots[0].Start);
        Assert.Equal(TimeSpan.Parse("10:30", CultureInfo.InvariantCulture), slots[3].Start);
        Assert.Equal(TimeSpan.Parse("11:00", CultureInfo.InvariantCulture), slots[3].End);
    }

    [Fact]
    public void GetDailySlots_WennDerLetzteSlotNichtMehrVollstaendigPasst_WirdErNichtAngeboten()
    {
        // 09:00 bis 10:10 bei 30 Minuten: der Rest von 10 Minuten faellt weg.
        var slots = AppointmentCalculator.GetDailySlots(
            ServiceWithSlotDuration(30),
            Monday,
            OpeningHours(DayOfWeek.Monday, "09:00", "10:10"),
            []);

        Assert.Equal(2, slots.Count);
        Assert.Equal(TimeSpan.Parse("10:00", CultureInfo.InvariantCulture), slots[1].End);
    }

    [Fact]
    public void GetDailySlots_OhneOeffnungszeitAmWochentag_LiefertKeineSlots()
    {
        var slots = AppointmentCalculator.GetDailySlots(
            ServiceWithSlotDuration(30),
            Monday,
            OpeningHours(DayOfWeek.Tuesday, "09:00", "17:00"),
            []);

        Assert.Empty(slots);
    }

    [Fact]
    public void GetDailySlots_WennStartNachEndeLiegt_LiefertKeineSlots()
    {
        var slots = AppointmentCalculator.GetDailySlots(
            ServiceWithSlotDuration(30),
            Monday,
            OpeningHours(DayOfWeek.Monday, "17:00", "09:00"),
            []);

        Assert.Empty(slots);
    }

    [Fact]
    public void GetDailySlots_DatumsOverrideSchlaegtWochentagOverrideUndOeffnungszeit()
    {
        List<ServiceDayOverride> overrides =
        [
            new ServiceDayOverride
            {
                ServiceId = 1,
                IsWeeklyOverride = true,
                WeeklyDay = DayOfWeek.Monday,
                StartTime = TimeSpan.Parse("13:00", CultureInfo.InvariantCulture),
                EndTime = TimeSpan.Parse("14:00", CultureInfo.InvariantCulture)
            },
            new ServiceDayOverride
            {
                ServiceId = 1,
                IsWeeklyOverride = false,
                Date = Monday,
                StartTime = TimeSpan.Parse("08:00", CultureInfo.InvariantCulture),
                EndTime = TimeSpan.Parse("09:00", CultureInfo.InvariantCulture)
            }
        ];

        var slots = AppointmentCalculator.GetDailySlots(
            ServiceWithSlotDuration(60),
            Monday,
            OpeningHours(DayOfWeek.Monday, "09:00", "17:00"),
            overrides);

        Assert.Single(slots);
        Assert.Equal(TimeSpan.Parse("08:00", CultureInfo.InvariantCulture), slots[0].Start);
    }

    [Fact]
    public void GetDailySlots_WochentagOverrideSchlaegtRegulaereOeffnungszeit()
    {
        List<ServiceDayOverride> overrides =
        [
            new ServiceDayOverride
            {
                ServiceId = 1,
                IsWeeklyOverride = true,
                WeeklyDay = DayOfWeek.Monday,
                StartTime = TimeSpan.Parse("13:00", CultureInfo.InvariantCulture),
                EndTime = TimeSpan.Parse("14:00", CultureInfo.InvariantCulture)
            }
        ];

        var slots = AppointmentCalculator.GetDailySlots(
            ServiceWithSlotDuration(60),
            Monday,
            OpeningHours(DayOfWeek.Monday, "09:00", "17:00"),
            overrides);

        Assert.Single(slots);
        Assert.Equal(TimeSpan.Parse("13:00", CultureInfo.InvariantCulture), slots[0].Start);
    }

    [Fact]
    public void GetDailySlots_AnGeschlossenemTag_LiefertKeineSlots()
    {
        List<ServiceDayOverride> overrides =
        [
            new ServiceDayOverride { ServiceId = 1, IsWeeklyOverride = false, Date = Monday, IsClosed = true }
        ];

        var slots = AppointmentCalculator.GetDailySlots(
            ServiceWithSlotDuration(30),
            Monday,
            OpeningHours(DayOfWeek.Monday, "09:00", "17:00"),
            overrides);

        Assert.Empty(slots);
    }

    [Fact]
    public void GetAvailableSlots_ZaehltNurGebuchteTermineDesSelbenSlots()
    {
        var service = ServiceWithSlotDuration(30, employeeCount: 2);

        List<Appointment> appointments =
        [
            // Zaehlt: liegt im ersten Slot und ist gebucht.
            new Appointment { ServiceId = 1, Date = Monday.AddHours(9), Status = AppointmentStatus.Booked },
            // Zaehlt nicht: abgesagt.
            new Appointment { ServiceId = 1, Date = Monday.AddHours(9), Status = AppointmentStatus.Cancelled },
            // Zaehlt nicht: anderer Slot.
            new Appointment { ServiceId = 1, Date = Monday.AddHours(10), Status = AppointmentStatus.Booked }
        ];

        var available = AppointmentCalculator.GetAvailableSlots(
            service,
            Monday,
            OpeningHours(DayOfWeek.Monday, "09:00", "10:00"),
            [],
            appointments);

        Assert.Equal(1, available[(TimeSpan.Parse("09:00", CultureInfo.InvariantCulture), TimeSpan.Parse("09:30", CultureInfo.InvariantCulture))]);
        Assert.Equal(2, available[(TimeSpan.Parse("09:30", CultureInfo.InvariantCulture), TimeSpan.Parse("10:00", CultureInfo.InvariantCulture))]);
    }

    [Fact]
    public void GetAvailableSlots_OhneZugewieseneMitarbeiter_IstDieKapazitaetNull()
    {
        var available = AppointmentCalculator.GetAvailableSlots(
            ServiceWithSlotDuration(30, employeeCount: 0),
            Monday,
            OpeningHours(DayOfWeek.Monday, "09:00", "10:00"),
            [],
            []);

        Assert.All(available.Values, free => Assert.Equal(0, free));
    }

    [Theory]
    // Kandidat endet 10:00, danach 30 Minuten Puffer, anderer Termin ab 10:30: passt genau.
    [InlineData("09:30", 30, "10:30", 30, true)]
    // Anderer Termin beginnt 10:29: der Puffer wird verletzt.
    [InlineData("09:30", 30, "10:29", 30, false)]
    // Direkte Ueberschneidung.
    [InlineData("09:30", 60, "10:00", 30, false)]
    // Kandidat liegt vollstaendig nach dem anderen Termin samt Puffer.
    [InlineData("11:00", 30, "09:00", 60, true)]
    public void IsSlotTimeCompatible_PruefstDenMindestabstandZwischenZweiTerminen(
        string candidateStart,
        int candidateDuration,
        string otherStart,
        int otherDuration,
        bool expected)
    {
        var buffer = TimeSpan.FromMinutes(30);

        var actual = AppointmentCalculator.IsSlotTimeCompatible(
            Monday.Add(TimeSpan.Parse(candidateStart, CultureInfo.InvariantCulture)),
            candidateDuration,
            [(Monday.Add(TimeSpan.Parse(otherStart, CultureInfo.InvariantCulture)), otherDuration)],
            buffer);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsSlotTimeCompatible_OhneWeitereTermine_IstImmerVertraeglich()
    {
        Assert.True(AppointmentCalculator.IsSlotTimeCompatible(
            Monday.AddHours(9),
            30,
            [],
            TimeSpan.FromMinutes(30)));
    }

    // ============================================================
    // AUSWAEHLBARE SLOTS
    // ============================================================

    /// <summary>Montag, 24.08.2026, 07:00 Uhr: liegt vor allen Slots der Tests.</summary>
    private static readonly DateTime MondayMorning = Monday.AddHours(7);

    private static readonly DateOnly MondayDay = DateOnly.FromDateTime(Monday);

    private static AvailableSlotDto Slot(string start, int freeCapacity) => new()
    {
        SlotStart = Monday.Add(TimeSpan.Parse(start, CultureInfo.InvariantCulture)),
        FreeCapacity = freeCapacity
    };

    private static List<AvailableSlotDto> SelectableSlots(
        List<AvailableSlotDto> availableSlots,
        DateTime now,
        int slotDurationMinutes = 10,
        List<(DateTime Start, int DurationMinutes)>? otherSelectedSlots = null) =>
        AppointmentCalculator.GetSelectableSlots(
            availableSlots,
            MondayDay,
            slotDurationMinutes,
            otherSelectedSlots ?? [],
            TimeSpan.FromMinutes(60),
            30,
            now);

    [Fact]
    public void GetSelectableSlots_FasstDieSlotsEinesRastersZusammen()
    {
        var selectable = SelectableSlots(
            [Slot("09:00", 3), Slot("09:10", 3), Slot("09:20", 3), Slot("09:30", 2)],
            MondayMorning);

        Assert.Equal(2, selectable.Count);
        Assert.Equal(Monday.AddHours(9), selectable[0].SlotStart);
        Assert.Equal(Monday.AddHours(9).AddMinutes(30), selectable[1].SlotStart);
    }

    [Fact]
    public void GetSelectableSlots_NimmtJeRasterDieKleinsteFreieKapazitaet()
    {
        var selectable = SelectableSlots(
            [Slot("09:00", 3), Slot("09:10", 1), Slot("09:20", 3)],
            MondayMorning);

        Assert.Equal(1, Assert.Single(selectable).FreeCapacity);
    }

    [Fact]
    public void GetSelectableSlots_OhneFreieKapazitaet_BietetDasRasterNichtAn()
    {
        var selectable = SelectableSlots(
            [Slot("09:00", 0), Slot("09:10", 0)],
            MondayMorning);

        Assert.Empty(selectable);
    }

    [Fact]
    public void GetSelectableSlots_AmLaufendenTag_LaesstNurSlotsMitAusreichendemVorlauf()
    {
        // 09:05 Uhr am selben Tag: 09:00 ist vorbei, 09:30 liegt innerhalb der
        // Vorlaufzeit von 30 Minuten, erst 10:00 bleibt uebrig.
        var selectable = SelectableSlots(
            [Slot("09:00", 2), Slot("09:30", 2), Slot("10:00", 2)],
            Monday.AddHours(9).AddMinutes(5));

        Assert.Equal(Monday.AddHours(10), Assert.Single(selectable).SlotStart);
    }

    [Fact]
    public void GetSelectableSlots_AnEinemAnderenTag_GiltDieVorlaufzeitNicht()
    {
        // Dieselbe Uhrzeit, aber der Tag liegt in der Zukunft.
        var selectable = SelectableSlots(
            [Slot("09:00", 2), Slot("09:30", 2)],
            Monday.AddDays(-1).AddHours(9).AddMinutes(5));

        Assert.Equal(2, selectable.Count);
    }

    [Fact]
    public void GetSelectableSlots_VerwirftWasZuNahAnEinemTerminEinesAnderenServiceLiegt()
    {
        // Anderer Termin 10:00 bis 10:10, Puffer 60 Minuten: 09:30 kollidiert,
        // 11:30 haelt den Abstand ein.
        var selectable = SelectableSlots(
            [Slot("09:30", 2), Slot("11:30", 2)],
            MondayMorning,
            otherSelectedSlots: [(Monday.AddHours(10), 10)]);

        Assert.Equal(Monday.AddHours(11).AddMinutes(30), Assert.Single(selectable).SlotStart);
    }

    [Fact]
    public void GetSelectableSlots_OhneSlots_LiefertEineLeereListe()
    {
        Assert.Empty(SelectableSlots([], MondayMorning));
    }

    [Fact]
    public void GetSelectableSlots_LiefertDieRasterAufsteigendSortiert()
    {
        var selectable = SelectableSlots(
            [Slot("11:00", 1), Slot("09:00", 1), Slot("10:00", 1)],
            MondayMorning);

        Assert.Equal(
            [Monday.AddHours(9), Monday.AddHours(10), Monday.AddHours(11)],
            selectable.Select(slot => slot.SlotStart));
    }

    [Fact]
    public void GetSelectableSlots_MitUngueltigemRaster_WirdAbgewiesen()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => AppointmentCalculator.GetSelectableSlots(
            [Slot("09:00", 1)],
            MondayDay,
            10,
            [],
            TimeSpan.FromMinutes(60),
            0,
            MondayMorning));
    }
}
