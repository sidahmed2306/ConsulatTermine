using ConsulatTermine.Application.Resources;

namespace ConsulatTermine.Application.Exceptions;

/// <summary>
/// Der aufrufende Benutzer besitzt nicht die fuer diesen Anwendungsfall noetige Berechtigung.
/// Wird serverseitig ausgeloest und ist unabhaengig davon, was die UI anzeigt oder ausblendet.
/// </summary>
public sealed class NotAuthorizedException : Exception
{
    public NotAuthorizedException()
        : base(BusinessMessages.Get("NotAuthorized"))
    {
    }

    public NotAuthorizedException(string message)
        : base(message)
    {
    }

    public NotAuthorizedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
