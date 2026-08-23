namespace ConsulatTermine.UI.Authentication;

/// <summary>
/// Zusaetzliche Claims der Mitarbeiteranmeldung. Die Standard-Claims
/// (<c>NameIdentifier</c>, <c>Name</c>, <c>Role</c>) stammen aus <c>ClaimTypes</c>.
/// </summary>
public static class EmployeeClaimTypes
{
    /// <summary>
    /// Kennzeichnet, dass der Mitarbeiter vor der weiteren Nutzung ein eigenes Passwort
    /// setzen muss. Wert ist <c>"true"</c> oder der Claim fehlt.
    /// </summary>
    public const string MustChangePassword = "consulat:must-change-password";
}
