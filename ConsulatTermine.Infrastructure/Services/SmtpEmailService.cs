using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text;
using ConsulatTermine.Application.Configuration;
using ConsulatTermine.Application.DTOs.Booking;
using ConsulatTermine.Application.Interfaces;
using ConsulatTermine.Application.Localization;
using ConsulatTermine.Infrastructure.Resources;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConsulatTermine.Infrastructure.Services;

/// <summary>
/// Versand ueber den konfigurierten SMTP-Postausgang.
/// </summary>
/// <remarks>
/// Jedes Schreiben wird in der Sprache des Empfaengers erzeugt. Neben den Texten
/// betrifft das die Schreibrichtung des HTML-Dokuments sowie die Formatierung von
/// Datum und Uhrzeit.
/// </remarks>
public class SmtpEmailService : IEmailService
{
    private readonly EmailOptions _emailOptions;
    private readonly ApplicationOptions _applicationOptions;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(
        IOptions<EmailOptions> emailOptions,
        IOptions<ApplicationOptions> applicationOptions,
        ILogger<SmtpEmailService> logger)
    {
        _emailOptions = emailOptions.Value;
        _applicationOptions = applicationOptions.Value;
        _logger = logger;
    }

    // =============================================================
    // TERMINBESTAETIGUNG
    // =============================================================
    public async Task SendBookingConfirmationAsync(
        string toEmail,
        string fullName,
        string bookingReference,
        string cancelToken,
        IReadOnlyList<BookingEmailAppointmentDto> appointments,
        string language)
    {
        var recipientLanguage = SupportedLanguages.Resolve(language);
        var culture = new CultureInfo(recipientLanguage.CultureCode);

        var cancelUrl =
            $"{_applicationOptions.BaseUrl.TrimEnd('/')}/appointment-cancel"
            + $"?ref={Uri.EscapeDataString(bookingReference)}"
            + $"&token={Uri.EscapeDataString(cancelToken)}";

        var content = new StringBuilder();

        content.Append(Paragraph(EmailTexts.Get("Salutation", culture)));
        content.Append(Paragraph(EmailTexts.Get("Confirmation.Intro", culture)));

        content.Append(DefinitionList(culture,
        [
            ("Label.Name", fullName),
            ("Label.BookingNumber", bookingReference)
        ]));

        content.Append(BuildAppointmentOverview(appointments, recipientLanguage, culture));

        content.Append(Paragraph(EmailTexts.Get("Confirmation.ManageIntro", culture)));
        content.Append(Button(cancelUrl, EmailTexts.Get("Confirmation.ManageButton", culture), "#1565c0"));
        content.Append(Paragraph(EmailTexts.Get("Confirmation.BringDocuments", culture)));

        await SendAsync(
            toEmail,
            EmailTexts.Get("Confirmation.Subject", culture),
            BuildDocument(
                recipientLanguage,
                culture,
                EmailTexts.Get("Confirmation.Heading", culture),
                "#2e7d32",
                content.ToString()));
    }

    // =============================================================
    // TEILABSAGE
    // =============================================================
    public async Task SendPartialCancellationAsync(
        string toEmail,
        string fullName,
        string serviceName,
        DateTime appointmentDate,
        string language)
    {
        var recipientLanguage = SupportedLanguages.Resolve(language);
        var culture = new CultureInfo(recipientLanguage.CultureCode);

        var content = new StringBuilder();

        content.Append(Paragraph(EmailTexts.Get("Salutation", culture)));

        content.Append(DefinitionList(culture,
        [
            ("Label.Name", fullName),
            ("Label.Service", serviceName),
            ("Label.Date", appointmentDate.ToString("d", culture)),
            ("Label.Time", appointmentDate.ToString("HH:mm", culture))
        ]));

        content.Append(Paragraph(EmailTexts.Get("PartialCancellation.Others", culture)));

        await SendAsync(
            toEmail,
            EmailTexts.Get("PartialCancellation.Subject", culture),
            BuildDocument(
                recipientLanguage,
                culture,
                EmailTexts.Get("PartialCancellation.Heading", culture),
                "#f9a825",
                content.ToString()));
    }

    // =============================================================
    // VOLLSTAENDIGE ABSAGE
    // =============================================================
    public async Task SendCancellationConfirmationAsync(
        string toEmail,
        string fullName,
        string bookingReference,
        string language)
    {
        var recipientLanguage = SupportedLanguages.Resolve(language);
        var culture = new CultureInfo(recipientLanguage.CultureCode);

        var content = new StringBuilder();

        content.Append(Paragraph(EmailTexts.Get("Salutation", culture)));

        content.Append(DefinitionList(culture,
        [
            ("Label.Name", fullName),
            ("Label.BookingNumber", bookingReference)
        ]));

        content.Append(Paragraph(EmailTexts.Get("Cancellation.Rebook", culture)));

        await SendAsync(
            toEmail,
            EmailTexts.Get("Cancellation.Subject", culture),
            BuildDocument(
                recipientLanguage,
                culture,
                EmailTexts.Get("Cancellation.Heading", culture),
                "#c62828",
                content.ToString()));
    }

    // =============================================================
    // MITARBEITER – WILLKOMMEN
    // =============================================================
    public async Task SendEmployeeWelcomeEmailAsync(
        string toEmail,
        string fullName,
        string employeeCode,
        string temporaryPassword,
        string changePasswordLink,
        string language)
    {
        var recipientLanguage = SupportedLanguages.Resolve(language);
        var culture = new CultureInfo(recipientLanguage.CultureCode);

        var content = new StringBuilder();

        content.Append(Paragraph(EmailTexts.Format("PersonalSalutation", culture, fullName)));
        content.Append(Paragraph(EmailTexts.Get("EmployeeWelcome.Intro", culture)));

        content.Append(DefinitionList(culture,
        [
            ("Label.EmployeeCode", employeeCode),
            ("Label.TemporaryPassword", temporaryPassword)
        ]));

        content.Append(Button(changePasswordLink, EmailTexts.Get("EmployeeWelcome.Button", culture), "#1565c0"));
        content.Append(Paragraph(EmailTexts.Get("EmployeeWelcome.Notice", culture)));

        await SendAsync(
            toEmail,
            EmailTexts.Get("EmployeeWelcome.Subject", culture),
            BuildDocument(
                recipientLanguage,
                culture,
                EmailTexts.Get("EmployeeWelcome.Heading", culture),
                "#1565c0",
                content.ToString()));
    }

    // =============================================================
    // MITARBEITER – PASSWORT ZURUECKSETZEN
    // =============================================================
    public async Task SendEmployeePasswordResetEmailAsync(
        string toEmail,
        string fullName,
        string resetLink,
        string language)
    {
        var recipientLanguage = SupportedLanguages.Resolve(language);
        var culture = new CultureInfo(recipientLanguage.CultureCode);

        var content = new StringBuilder();

        content.Append(Paragraph(EmailTexts.Format("PersonalSalutation", culture, fullName)));
        content.Append(Paragraph(EmailTexts.Get("PasswordReset.Intro", culture)));
        content.Append(Button(resetLink, EmailTexts.Get("PasswordReset.Button", culture), "#1565c0"));
        content.Append(SmallPrint(EmailTexts.Get("PasswordReset.Validity", culture)));

        await SendAsync(
            toEmail,
            EmailTexts.Get("PasswordReset.Subject", culture),
            BuildDocument(
                recipientLanguage,
                culture,
                EmailTexts.Get("PasswordReset.Heading", culture),
                "#1565c0",
                content.ToString()));
    }

    // =============================================================
    // MITARBEITER – PASSWORT GEAENDERT
    // =============================================================
    public async Task SendEmployeePasswordChangedConfirmationEmailAsync(
        string toEmail,
        string fullName,
        string loginLink,
        string language)
    {
        var recipientLanguage = SupportedLanguages.Resolve(language);
        var culture = new CultureInfo(recipientLanguage.CultureCode);

        var content = new StringBuilder();

        content.Append(Paragraph(EmailTexts.Format("PersonalSalutation", culture, fullName)));
        content.Append(Paragraph(EmailTexts.Get("PasswordChanged.Intro", culture)));
        content.Append(Button(loginLink, EmailTexts.Get("PasswordChanged.Button", culture), "#2e7d32"));

        await SendAsync(
            toEmail,
            EmailTexts.Get("PasswordChanged.Subject", culture),
            BuildDocument(
                recipientLanguage,
                culture,
                EmailTexts.Get("PasswordChanged.Heading", culture),
                "#2e7d32",
                content.ToString()));
    }

    // =============================================================
    // VERSAND
    // =============================================================
    private async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        if (!TryGetEmailConfig(out var email))
        {
            return;
        }

        using var smtpClient = CreateSmtpClient(email);

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(email.FromEmail, email.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true,

            // Arabische und deutsche Umlaute brauchen eine ausdrueckliche Kodierung,
            // sonst kommen Betreff und Text je nach Postausgang zerlegt an.
            SubjectEncoding = Encoding.UTF8,
            BodyEncoding = Encoding.UTF8
        };

        mailMessage.To.Add(toEmail);
        await smtpClient.SendMailAsync(mailMessage);
    }

    // =============================================================
    // HTML-BAUSTEINE
    //
    // Jeder eingesetzte Wert wird kodiert: Namen und Bezeichnungen stammen aus
    // Benutzereingaben beziehungsweise aus Stammdaten und duerfen das Schreiben
    // nicht veraendern koennen. Siehe harness/security.md Abschnitt 4.
    // =============================================================
    private static string BuildDocument(
        SupportedLanguage language,
        CultureInfo culture,
        string heading,
        string headingColor,
        string content)
    {
        var fontFamily = language.IsRightToLeft
            ? "'Segoe UI', 'Noto Naskh Arabic', 'Geeza Pro', Tahoma, Arial, sans-serif"
            : "Arial, Helvetica, sans-serif";

        return $"""
            <!DOCTYPE html>
            <html lang="{Encode(language.CultureCode)}" dir="{language.TextDirection}">
            <body style="font-family:{fontFamily}; background:#f5f5f5; padding:20px; margin:0">
            <div style="max-width:600px; margin:auto; background:#fff; padding:24px; border-radius:8px; text-align:{(language.IsRightToLeft ? "right" : "left")}">

            <h2 style="color:{headingColor}; margin-top:0">{Encode(heading)}</h2>

            {content}

            <p style="margin-top:30px">
            {Encode(EmailTexts.Get("Closing", culture))}<br/>
            <strong>{Encode(EmailTexts.Get("Signature", culture))}</strong>
            </p>

            <p style="font-size:12px; color:#888">
            {Encode(EmailTexts.Get("AutomatedNotice", culture))}
            </p>

            </div>
            </body>
            </html>
            """;
    }

    private static string BuildAppointmentOverview(
        IReadOnlyList<BookingEmailAppointmentDto> appointments,
        SupportedLanguage language,
        CultureInfo culture)
    {
        if (appointments is null || appointments.Count == 0)
        {
            return Paragraph(EmailTexts.Get("Confirmation.NoDetails", culture));
        }

        var html = new StringBuilder();

        html.Append(CultureInfo.InvariantCulture, $"<h3>{Encode(EmailTexts.Get("Confirmation.ListHeading", culture))}</h3>");

        var groupedByPerson = appointments
            .OrderBy(appointment => appointment.DateTime)
            .GroupBy(appointment => appointment.PersonFullName);

        foreach (var personGroup in groupedByPerson)
        {
            html.Append(CultureInfo.InvariantCulture, $"<p><strong>{Encode(personGroup.Key)}</strong></p><ul>");

            foreach (var appointment in personGroup)
            {
                var serviceName = LocalizedText.ForCulture(
                    appointment.ServiceName,
                    appointment.ServiceNameEnglish,
                    appointment.ServiceNameArabic,
                    language.CultureCode);

                var date = appointment.DateTime.ToString("d", culture);
                var time = appointment.DateTime.ToString("HH:mm", culture);

                html.Append(CultureInfo.InvariantCulture,
                    $"<li><strong>{Encode(serviceName)}</strong><br/>{Encode(date)} – {Encode(time)}</li>");
            }

            html.Append("</ul>");
        }

        return html.ToString();
    }

    private static string DefinitionList(
        CultureInfo culture,
        IReadOnlyList<(string LabelKey, string Value)> entries)
    {
        var html = new StringBuilder("<p>");

        for (var index = 0; index < entries.Count; index++)
        {
            var (labelKey, value) = entries[index];

            html.Append(CultureInfo.InvariantCulture,
                $"<strong>{Encode(EmailTexts.Get(labelKey, culture))}:</strong> {Encode(value)}");

            if (index < entries.Count - 1)
            {
                html.Append("<br/>");
            }
        }

        return html.Append("</p>").ToString();
    }

    private static string Paragraph(string text) =>
        $"<p>{Encode(text)}</p>";

    private static string SmallPrint(string text) =>
        $"""<p style="font-size:12px; color:#888">{Encode(text)}</p>""";

    private static string Button(string url, string label, string color) =>
        $"""
        <p style="text-align:center; margin:30px 0">
        <a href="{Encode(url)}" style="background:{color}; color:#fff; padding:12px 20px; text-decoration:none; border-radius:6px; font-weight:bold; display:inline-block">
        {Encode(label)}
        </a>
        </p>
        """;

    private static string Encode(string? value) =>
        WebUtility.HtmlEncode(value ?? string.Empty);

    // =============================================================
    // KONFIGURATION
    // =============================================================
    /// <summary>
    /// Prueft, ob ein Postausgang konfiguriert ist. Ohne SMTP-Server wird kein Versand
    /// versucht: lokal laeuft die Anwendung so ohne hinterlegte Zugangsdaten.
    /// </summary>
    private bool TryGetEmailConfig(out EmailOptions config)
    {
        config = _emailOptions;

        if (!string.IsNullOrWhiteSpace(_emailOptions.SmtpServer))
        {
            return true;
        }

        ServiceLog.SmtpNotConfigured(_logger);
        return false;
    }

    private static SmtpClient CreateSmtpClient(EmailOptions options)
    {
        return new SmtpClient
        {
            Host = options.SmtpServer,
            Port = options.Port,
            EnableSsl = options.UseSsl,
            Credentials = new NetworkCredential(options.Username, options.Password)
        };
    }
}
