using System.Security.Cryptography;

namespace ConsulatTermine.Infrastructure.Security;

public static class TemporaryPasswordGenerator
{
    public static string Generate()
    {
        // 8‑stelliges sicheres Passwort
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(6))
            .Replace("=", "")
            .Replace("+", "A")
            .Replace("/", "B");
    }
}