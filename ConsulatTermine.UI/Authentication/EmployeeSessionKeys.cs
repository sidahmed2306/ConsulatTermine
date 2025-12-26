namespace ConsulatTermine.UI.Authentication;

public static class EmployeeSessionKeys
{
    public const string EmployeeId = "EmployeeId";
    public const string EmployeeCode = "EmployeeCode";
    public const string ActiveServiceId = "EmployeeActiveServiceId";


    // Für Timeout / Inaktivität
    public const string LastActivityUtcTicks = "EmployeeLastActivityUtcTicks";
}
