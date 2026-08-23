using System.Globalization;
using ConsulatTermine.Infrastructure.Security;
using ConsulatTermine.Infrastructure.Services.Booking;

namespace ConsulatTermine.Infrastructure.Test;

public sealed class PasswordResetTokenTests
{
    [Fact]
    public void Create_LiefertBeiJedemAufrufEinAnderesToken()
    {
        var first = PasswordResetToken.Create();
        var second = PasswordResetToken.Create();

        Assert.NotEqual(first.Token, second.Token);
        Assert.NotEqual(first.Hash, second.Hash);
    }

    [Fact]
    public void Create_LiefertEinenHashDerNichtDasTokenSelbstIst()
    {
        var (token, hash) = PasswordResetToken.Create();

        // Ein Angreifer mit Lesezugriff auf die Datenbank darf aus dem Hash keinen
        // gueltigen Link bauen koennen.
        Assert.NotEqual(token, hash);
        Assert.DoesNotContain(token, hash, StringComparison.Ordinal);
    }

    [Fact]
    public void Create_LiefertEinUrlSicheresToken()
    {
        var (token, _) = PasswordResetToken.Create();

        Assert.Equal(token, Uri.EscapeDataString(token));
    }

    [Fact]
    public void Hash_IstFuerDasselbeTokenReproduzierbar()
    {
        var (token, hash) = PasswordResetToken.Create();

        Assert.Equal(hash, PasswordResetToken.Hash(token));
    }

    [Fact]
    public void Hash_PasstInDieDatenbankspalte()
    {
        var (_, hash) = PasswordResetToken.Create();

        // Die Spalte ist auf 64 Zeichen begrenzt, siehe EmployeeMapping.
        Assert.Equal(64, hash.Length);
    }

    [Fact]
    public void Hash_OhneToken_WirdAbgelehnt()
    {
        Assert.Throws<ArgumentException>(() => PasswordResetToken.Hash("  "));
    }
}

public sealed class InitialPasswordGeneratorTests
{
    [Fact]
    public void Generate_LiefertBeiJedemAufrufEinAnderesPasswort()
    {
        var passwords = Enumerable.Range(0, 50)
            .Select(_ => InitialPasswordGenerator.Generate())
            .ToHashSet(StringComparer.Ordinal);

        Assert.Equal(50, passwords.Count);
    }

    [Fact]
    public void Generate_VermeidetLeichtVerwechselbareZeichen()
    {
        // Das Passwort wird aus einer E-Mail abgetippt.
        for (var i = 0; i < 100; i++)
        {
            var password = InitialPasswordGenerator.Generate();

            Assert.DoesNotContain('0', password);
            Assert.DoesNotContain('O', password);
            Assert.DoesNotContain('1', password);
            Assert.DoesNotContain('l', password);
            Assert.DoesNotContain('I', password);
        }
    }

    [Fact]
    public void Generate_HatEineAusreichendeLaenge()
    {
        Assert.Equal(14, InitialPasswordGenerator.Generate().Length);
    }
}

public sealed class BookingReferenceGeneratorTests
{
    [Fact]
    public void GenerateReference_IstUnabhaengigVonDerAktuellenKultur()
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            // Die Anwendung unterstuetzt ar-DZ. Kulturabhaengige Formatierung wuerde
            // andere Ziffernzeichen erzeugen; die Referenz muss in E-Mail, Datenbank
            // und Absage-Link exakt uebereinstimmen.
            CultureInfo.CurrentCulture = new CultureInfo("ar-DZ");
            var arabic = new BookingReferenceGenerator().GenerateReference();

            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            var german = new BookingReferenceGenerator().GenerateReference();

            Assert.Matches("^CONSUL-[0-9]{4}-[0-9A-F]{6}$", arabic);
            Assert.Matches("^CONSUL-[0-9]{4}-[0-9A-F]{6}$", german);
            Assert.Equal(arabic[..12], german[..12]);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void GenerateReference_PasstInDieDatenbankspalte()
    {
        // Die Spalte ist auf 40 Zeichen begrenzt, siehe AppointmentMapping.
        Assert.True(new BookingReferenceGenerator().GenerateReference().Length <= 40);
    }
}
