using ConsulatTermine.Application.Services;

namespace ConsulatTermine.Application.Test;

/// <summary>
/// Tests der Auswahlregeln im ersten Buchungsschritt. Fachlich gilt:
/// jede Person braucht mindestens einen Termin, ein Service hoechstens einen je Person,
/// insgesamt hoechstens <see cref="ServiceSelectionCalculator.MaxServicesPerPerson"/> je Person.
/// </summary>
public sealed class ServiceSelectionCalculatorTests
{
    private const int Reisepass = 1;
    private const int Personalausweis = 2;
    private const int Beglaubigung = 3;

    [Fact]
    public void IsSelectionComplete_OhneJedeZuordnung_IstNichtVollstaendig()
    {
        var assignments = new Dictionary<int, int>();

        Assert.False(ServiceSelectionCalculator.IsSelectionComplete(assignments, 1));
    }

    [Fact]
    public void IsSelectionComplete_BeiEinerPersonUndEinemTermin_IstVollstaendig()
    {
        var assignments = new Dictionary<int, int> { [Reisepass] = 1 };

        Assert.True(ServiceSelectionCalculator.IsSelectionComplete(assignments, 1));
    }

    [Fact]
    public void IsSelectionComplete_BeiZweiPersonenUndNurEinemTermin_IstNichtVollstaendig()
    {
        // Der gemeldete Fehlerfall: zwei Personen, einmal auf "+" geklickt,
        // "Bestaetigen" bleibt gesperrt.
        var assignments = new Dictionary<int, int> { [Reisepass] = 1 };

        Assert.False(ServiceSelectionCalculator.IsSelectionComplete(assignments, 2));
    }

    [Fact]
    public void IsSelectionComplete_BeiZweiPersonenUndZweiTerminenDesselbenService_IstVollstaendig()
    {
        var assignments = new Dictionary<int, int> { [Reisepass] = 2 };

        Assert.True(ServiceSelectionCalculator.IsSelectionComplete(assignments, 2));
    }

    [Fact]
    public void IsSelectionComplete_BeiZweiPersonenUndZweiVerschiedenenServices_IstVollstaendig()
    {
        var assignments = new Dictionary<int, int> { [Reisepass] = 1, [Personalausweis] = 1 };

        Assert.True(ServiceSelectionCalculator.IsSelectionComplete(assignments, 2));
    }

    [Fact]
    public void MissingAssignments_NenntDieAnzahlDerNochFehlendenTermine()
    {
        var assignments = new Dictionary<int, int> { [Reisepass] = 1 };

        Assert.Equal(2, ServiceSelectionCalculator.MissingAssignments(assignments, 3));
    }

    [Fact]
    public void MissingAssignments_WennGenugTermineVergebenSind_IstNull()
    {
        var assignments = new Dictionary<int, int> { [Reisepass] = 2, [Personalausweis] = 2 };

        Assert.Equal(0, ServiceSelectionCalculator.MissingAssignments(assignments, 2));
    }

    [Fact]
    public void CanAddService_WennDerServiceBereitsJedePersonAbdeckt_IstNichtMehrMoeglich()
    {
        var assignments = new Dictionary<int, int> { [Reisepass] = 2 };

        Assert.False(ServiceSelectionCalculator.CanAddService(assignments, Reisepass, 2));
        Assert.True(ServiceSelectionCalculator.CanAddService(assignments, Personalausweis, 2));
    }

    [Fact]
    public void CanAddService_WennDasGesamtlimitErreichtIst_IstNichtMehrMoeglich()
    {
        // Eine Person darf hoechstens drei Termine erhalten.
        var assignments = new Dictionary<int, int>
        {
            [Reisepass] = 1,
            [Personalausweis] = 1,
            [Beglaubigung] = 1
        };

        Assert.Equal(3, ServiceSelectionCalculator.MaxTotalAssignments(1));
        Assert.False(ServiceSelectionCalculator.CanAddService(assignments, 4, 1));
    }

    [Fact]
    public void CanRemoveService_NurWennDemServiceEinTerminZugeordnetIst()
    {
        var assignments = new Dictionary<int, int> { [Reisepass] = 1 };

        Assert.True(ServiceSelectionCalculator.CanRemoveService(assignments, Reisepass));
        Assert.False(ServiceSelectionCalculator.CanRemoveService(assignments, Personalausweis));
    }

    [Fact]
    public void Normalize_BeiVerringerterPersonenzahl_KapptJedenServiceAufDieNeuePersonenzahl()
    {
        // Regressionsfall: zwei Personen mit zwei Reisepass-Terminen, danach auf eine Person
        // reduziert. Ohne Normalisierung blieb ein Zustand bestehen, der ueber "+" gar nicht
        // erzeugt werden kann, und liess sich trotzdem bestaetigen.
        var assignments = new Dictionary<int, int> { [Reisepass] = 2 };

        var normalized = ServiceSelectionCalculator.Normalize(assignments, 1);

        Assert.Equal(1, normalized[Reisepass]);
        Assert.True(ServiceSelectionCalculator.IsSelectionComplete(normalized, 1));
    }

    [Fact]
    public void Normalize_KapptAuchDasGesamtlimit_UndBeginntBeimGroesstenService()
    {
        var assignments = new Dictionary<int, int>
        {
            [Reisepass] = 3,
            [Personalausweis] = 2,
            [Beglaubigung] = 1
        };

        // Zwei Personen erlauben hoechstens sechs Termine; sechs sind bereits vergeben.
        var normalized = ServiceSelectionCalculator.Normalize(assignments, 2);

        Assert.Equal(2, normalized[Reisepass]);
        Assert.Equal(2, normalized[Personalausweis]);
        Assert.Equal(1, normalized[Beglaubigung]);
        Assert.Equal(5, ServiceSelectionCalculator.TotalAssigned(normalized));
    }

    [Fact]
    public void Normalize_EntferntServicesOhneZuordnung()
    {
        var assignments = new Dictionary<int, int> { [Reisepass] = 1, [Personalausweis] = 0 };

        var normalized = ServiceSelectionCalculator.Normalize(assignments, 1);

        Assert.False(normalized.ContainsKey(Personalausweis));
    }

    [Fact]
    public void Normalize_BeiErhoehterPersonenzahl_LaesstDieAuswahlUnveraendert()
    {
        var assignments = new Dictionary<int, int> { [Reisepass] = 1 };

        var normalized = ServiceSelectionCalculator.Normalize(assignments, 3);

        Assert.Equal(1, normalized[Reisepass]);
        Assert.False(ServiceSelectionCalculator.IsSelectionComplete(normalized, 3));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(ServiceSelectionCalculator.MaxPersons + 1)]
    public void IsSelectionComplete_BeiUnzulaessigerPersonenzahl_WirdAbgewiesen(int personCount)
    {
        var assignments = new Dictionary<int, int> { [Reisepass] = 1 };

        Assert.Throws<ArgumentOutOfRangeException>(
            () => ServiceSelectionCalculator.IsSelectionComplete(assignments, personCount));
    }
}
