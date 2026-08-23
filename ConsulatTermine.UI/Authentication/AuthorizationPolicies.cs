namespace ConsulatTermine.UI.Authentication;

/// <summary>
/// Die im Projekt gueltigen Policies. Sie bilden die drei bereits vorhandenen Rollen
/// Employee &lt; ServiceChef &lt; Admin ab und werden in PROJECT_CONTEXT.md Abschnitt 7
/// gefuehrt. Neue Policies werden nicht erfunden, sondern fachlich entschieden.
/// </summary>
public static class AuthorizationPolicies
{
    /// <summary>Jeder angemeldete, aktive Mitarbeiter.</summary>
    public const string MitarbeiterZugriff = "Mitarbeiter.Zugriff";

    /// <summary>
    /// Mitarbeiterverwaltung, Service-Zuweisungen und Arbeitszeiten: ServiceChef und Admin.
    /// </summary>
    public const string DienstplanVerwalten = "Dienstplan.Verwalten";

    /// <summary>Serviceverwaltung und Rollenvergabe: ausschliesslich Admin.</summary>
    public const string AdministrationVerwalten = "Administration.Verwalten";
}
