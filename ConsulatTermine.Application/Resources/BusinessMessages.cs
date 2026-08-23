using System.Globalization;
using System.Resources;

namespace ConsulatTermine.Application.Resources;

/// <summary>
/// Fachliche Meldungen, die einem Benutzer angezeigt werden.
/// </summary>
/// <remarks>
/// Die Meldungen liegen in der Anwendungsschicht, weil sie zum fachlichen Vertrag
/// der Anwendungsfaelle gehoeren und dort ausgeloest werden, nicht erst in der
/// Oberflaeche. Sie enthalten keine internen Einzelheiten; technische Ursachen
/// gehoeren ins Log.
///
/// Gelesen wird in <see cref="CultureInfo.CurrentUICulture"/>: Empfaenger der
/// Meldung ist immer die Sitzung, die den Anwendungsfall ausgeloest hat. Der
/// Schriftverkehr ist davon zu unterscheiden, dort gilt die Sprache des Empfaengers.
///
/// <c>BusinessMessages.resx</c> ist Deutsch und zugleich die Rueckfallebene,
/// <c>BusinessMessages.en.resx</c> und <c>BusinessMessages.ar.resx</c> ergaenzen
/// Englisch und Arabisch.
/// </remarks>
public static class BusinessMessages
{
    private static readonly ResourceManager _resources =
        new("ConsulatTermine.Application.Resources.BusinessMessages", typeof(BusinessMessages).Assembly);

    /// <summary>
    /// Liest eine Meldung in der Sprache der laufenden Sitzung. Fehlt der Eintrag,
    /// bleibt der Schluessel stehen: eine leere Fehlermeldung waere schlimmer als
    /// eine technische, weil der Vorgang dann ohne erkennbaren Grund abbricht.
    /// </summary>
    public static string Get(string key) =>
        _resources.GetString(key, CultureInfo.CurrentUICulture) ?? key;

    /// <summary>
    /// Liest eine Meldung mit Platzhaltern und setzt die Werte ein.
    /// </summary>
    /// <remarks>
    /// Der Text stammt aus <see cref="CultureInfo.CurrentUICulture"/>, die Werte
    /// werden mit <see cref="CultureInfo.CurrentCulture"/> formatiert. Beides ist
    /// bewusst getrennt: die erste Kultur bestimmt die Sprache, die zweite die
    /// Schreibweise von Datum, Uhrzeit und Zahlen.
    /// </remarks>
    public static string Format(string key, params object?[] arguments) =>
        string.Format(CultureInfo.CurrentCulture, Get(key), arguments);
}
