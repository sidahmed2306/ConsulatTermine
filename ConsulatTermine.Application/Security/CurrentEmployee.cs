using ConsulatTermine.Domain.Enums;

namespace ConsulatTermine.Application.Security;

/// <summary>
/// Der aktuell angemeldete Mitarbeiter, wie er aus der serverseitigen Identitaet hervorgeht.
/// Wird nicht aus Benutzereingaben oder Browser-Speicher aufgebaut.
/// </summary>
/// <param name="EmployeeId">Datenbank-Id des Mitarbeiters.</param>
/// <param name="EmployeeCode">Fachliche Kennung, zum Beispiel CDZ-001.</param>
/// <param name="Role">Rolle laut Datenbank zum Zeitpunkt der Anmeldung.</param>
public sealed record CurrentEmployee(int EmployeeId, string EmployeeCode, EmployeeRole Role);
