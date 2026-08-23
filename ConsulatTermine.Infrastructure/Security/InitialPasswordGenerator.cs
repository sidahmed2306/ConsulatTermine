using System.Security.Cryptography;

namespace ConsulatTermine.Infrastructure.Security;

/// <summary>
/// Erzeugt das Initialpasswort eines neu angelegten Mitarbeiters.
/// Der Klartext existiert nur bis zum Versand der Willkommens-E-Mail; gespeichert wird
/// ausschliesslich der Hash.
/// </summary>
public static class InitialPasswordGenerator
{
    /// <summary>
    /// Zeichenvorrat ohne leicht verwechselbare Zeichen (0/O, 1/l/I), damit das Passwort
    /// aus einer E-Mail zuverlaessig abgetippt werden kann.
    /// </summary>
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";

    private const int PasswordLength = 14;

    /// <summary>
    /// Erzeugt ein kryptografisch zufaelliges Passwort mit rund 80 Bit Entropie.
    /// </summary>
    public static string Generate()
    {
        return RandomNumberGenerator.GetString(Alphabet, PasswordLength);
    }
}
