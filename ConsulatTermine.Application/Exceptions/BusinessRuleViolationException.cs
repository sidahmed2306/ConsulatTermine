namespace ConsulatTermine.Application.Exceptions;

/// <summary>
/// Erwarteter fachlicher Fehler: eine Geschaeftsregel wurde verletzt.
/// Die Meldung ist fuer die Anzeige an Benutzer bestimmt und enthaelt keine internen Details.
/// Abzugrenzen von technischen Fehlern, die als solche durchgereicht werden.
/// </summary>
public sealed class BusinessRuleViolationException : Exception
{
    public BusinessRuleViolationException()
    {
    }

    public BusinessRuleViolationException(string message)
        : base(message)
    {
    }

    public BusinessRuleViolationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
