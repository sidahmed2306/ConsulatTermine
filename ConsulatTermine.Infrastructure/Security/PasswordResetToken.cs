using System.Security.Cryptography;
using System.Text;

namespace ConsulatTermine.Infrastructure.Security;

/// <summary>
/// Token zum Zuruecksetzen des Passworts.
/// In der Datenbank liegt ausschliesslich der SHA-256-Hash. Ein Angreifer mit Lesezugriff
/// auf die Datenbank kann daraus keinen gueltigen Link ableiten.
/// </summary>
public static class PasswordResetToken
{
    private const int TokenByteLength = 32;

    /// <summary>
    /// Erzeugt ein neues Token als URL-sicheren Klartext samt zugehoerigem Hash.
    /// </summary>
    /// <returns>
    /// <c>Token</c> gehoert in den E-Mail-Link, <c>Hash</c> in die Datenbank.
    /// </returns>
    public static (string Token, string Hash) Create()
    {
        var token = Base64UrlEncode(RandomNumberGenerator.GetBytes(TokenByteLength));
        return (token, Hash(token));
    }

    /// <summary>
    /// Bildet den Hash eines Tokens aus einem eingehenden Link, um ihn mit der
    /// Datenbank vergleichen zu koennen.
    /// </summary>
    public static string Hash(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hash);
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
