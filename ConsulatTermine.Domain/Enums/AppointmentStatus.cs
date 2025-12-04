namespace ConsulatTermine.Domain.Enums;

public enum AppointmentStatus
{
    Booked = 0,
    CheckedIn = 1,
    InProgress = 2,   // neu: Termin wird gerade bearbeitet / aufgerufen
    Completed = 3,
    Cancelled = 4
}
