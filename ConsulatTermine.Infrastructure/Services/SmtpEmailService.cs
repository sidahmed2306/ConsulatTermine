using System.Net;
using System.Net.Mail;
using ConsulatTermine.Application.DTOs.Booking;
using ConsulatTermine.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ConsulatTermine.Infrastructure.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public SmtpEmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // =============================================================
    // TERMINBESTÄTIGUNG (ERWEITERT – MIT SERVICE / DATUM / UHRZEIT)
    // =============================================================
    public async Task SendBookingConfirmationAsync(
        string toEmail,
        string fullName,
        string bookingReference,
        string cancelToken,
        IReadOnlyList<BookingEmailAppointmentDto> appointments)
    {
        if (!TryGetEmailConfig(out var email))
        {
            return;
        }

        var cancelUrl =
            $"http://localhost:5262/appointment-cancel?ref={bookingReference}&token={cancelToken}";

        var servicesHtml = BuildServicesOverviewHtml(appointments);

        using var smtpClient = CreateSmtpClient(email);

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(email.FromEmail, email.FromName),
            Subject = "Terminbestätigung – Konsulat",
            Body = BuildHtmlMailBody(
                fullName,
                bookingReference,
                cancelUrl,
                servicesHtml),
            IsBodyHtml = true
        };

        mailMessage.To.Add(toEmail);
        await smtpClient.SendMailAsync(mailMessage);
    }

    // =============================================================
    // TEIL-ABSAGE
    // =============================================================
    public async Task SendPartialCancellationAsync(
        string toEmail,
        string fullName,
        string serviceName,
        DateTime date)
    {
        if (!TryGetEmailConfig(out var email))
        {
            return;
        }

        using var smtpClient = CreateSmtpClient(email);

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(email.FromEmail, email.FromName),
            Subject = "Termin teilweise abgesagt – Konsulat",
            Body = BuildPartialCancellationHtmlMailBody(
                fullName,
                serviceName,
                date),
            IsBodyHtml = true
        };

        mailMessage.To.Add(toEmail);
        await smtpClient.SendMailAsync(mailMessage);
    }

    // =============================================================
    // VOLLSTÄNDIGE ABSAGE
    // =============================================================
    public async Task SendCancellationConfirmationAsync(
        string toEmail,
        string fullName,
        string bookingReference)
    {
        if (!TryGetEmailConfig(out var email))
        {
            return;
        }

        using var smtpClient = CreateSmtpClient(email);

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(email.FromEmail, email.FromName),
            Subject = "Alle Termine abgesagt – Konsulat",
            Body = BuildCancellationHtmlMailBody(
                fullName,
                bookingReference),
            IsBodyHtml = true
        };

        mailMessage.To.Add(toEmail);
        await smtpClient.SendMailAsync(mailMessage);
    }

    // =============================================================
    // MITARBEITER – WILLKOMMEN
    // =============================================================
    public async Task SendEmployeeWelcomeEmailAsync(
        string toEmail,
        string fullName,
        string employeeCode,
        string temporaryPassword,
        string changePasswordLink)
    {
        if (!TryGetEmailConfig(out var email))
        {
            return;
        }

        using var smtpClient = CreateSmtpClient(email);

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(email.FromEmail, email.FromName),
            Subject = "Willkommen – Mitarbeiterzugang Konsulat",
            Body = BuildEmployeeWelcomeHtmlMailBody(
                fullName,
                employeeCode,
                temporaryPassword,
                changePasswordLink),
            IsBodyHtml = true
        };

        mailMessage.To.Add(toEmail);
        await smtpClient.SendMailAsync(mailMessage);
    }

    // =============================================================
    // MITARBEITER – PASSWORT GEÄNDERT
    // =============================================================
    public async Task SendEmployeePasswordChangedConfirmationEmailAsync(
        string toEmail,
        string fullName,
        string loginLink)
    {
        if (!TryGetEmailConfig(out var email))
        {
            return;
        }

        using var smtpClient = CreateSmtpClient(email);

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(email.FromEmail, email.FromName),
            Subject = "Passwort erfolgreich geändert – Konsulat",
            Body = BuildEmployeePasswordChangedHtmlMailBody(
                fullName,
                loginLink),
            IsBodyHtml = true
        };

        mailMessage.To.Add(toEmail);
        await smtpClient.SendMailAsync(mailMessage);
    }

    // =============================================================
    // HTML – TERMINBESTÄTIGUNG (HAUPT-TEMPLATE)
    // =============================================================
    private static string BuildHtmlMailBody(
        string fullName,
        string bookingReference,
        string manageUrl,
        string servicesOverviewHtml)
    {
        return $@"
<!DOCTYPE html>
<html lang=""de"">
<body style=""font-family: Arial; background:#f5f5f5; padding:20px"">
<div style=""max-width:600px; margin:auto; background:#fff; padding:24px; border-radius:8px"">

<h2 style=""color:#2e7d32"">Terminbestätigung</h2>

<p>Sehr geehrte Damen und Herren,</p>

<p>
Ihr Termin beim <strong>Konsulat</strong> wurde erfolgreich registriert.
</p>

<p>
<strong>Name:</strong> {fullName}<br/>
<strong>Buchungsnummer:</strong> {bookingReference}
</p>

{servicesOverviewHtml}

<p style=""margin-top:20px"">
Über den folgenden Button können Sie Ihre Termine einsehen, verwalten oder absagen:
</p>

<p style=""text-align:center; margin:30px 0"">
<a href=""{manageUrl}""
style=""background:#1565c0;color:#fff;padding:12px 20px;
text-decoration:none;border-radius:6px;font-weight:bold"">
Termin verwalten
</a>
</p>

<p>
Bitte erscheinen Sie pünktlich und bringen Sie alle erforderlichen Unterlagen mit.
</p>

<p style=""margin-top:30px"">
Mit freundlichen Grüßen<br/>
<strong>Konsulat – Terminservice</strong>
</p>

<p style=""font-size:12px;color:#888"">
Dies ist eine automatisch generierte E-Mail.
</p>

</div>
</body>
</html>";
    }

    // =============================================================
    // HTML – SERVICES-ÜBERSICHT (NEU)
    // =============================================================
    private static string BuildServicesOverviewHtml(
        IReadOnlyList<BookingEmailAppointmentDto> appointments)
    {
        if (appointments == null || appointments.Count == 0)
        {
            return "<p><em>Keine Termindetails verfügbar.</em></p>";
        }

        var grouped = appointments
            .OrderBy(a => a.DateTime)
            .GroupBy(a => a.PersonFullName);

        var html = "<h3>Gebuchte Termine</h3>";

        foreach (var personGroup in grouped)
        {
            html += $"<p><strong>{personGroup.Key}</strong></p><ul>";

            foreach (var a in personGroup)
            {
                html += $@"
<li>
<strong>{a.ServiceName}</strong><br/>
{a.DateTime:dd.MM.yyyy} – {a.DateTime:HH:mm} Uhr
</li>";
            }

            html += "</ul>";
        }

        return html;
    }

    // =============================================================
    // HTML – TEILABSAGE
    // =============================================================
    private static string BuildPartialCancellationHtmlMailBody(
        string fullName,
        string serviceName,
        DateTime date)
    {
        return $@"
<!DOCTYPE html>
<html lang=""de"">
<body style=""font-family: Arial; background:#f5f5f5; padding:20px"">
<div style=""max-width:600px; margin:auto; background:#fff; padding:24px; border-radius:8px"">

<h2 style=""color:#f9a825"">Termin teilweise abgesagt</h2>

<p>Sehr geehrte Damen und Herren,</p>

<ul>
<li><strong>Name:</strong> {fullName}</li>
<li><strong>Service:</strong> {serviceName}</li>
<li><strong>Datum:</strong> {date:dd.MM.yyyy}</li>
<li><strong>Uhrzeit:</strong> {date:HH:mm} Uhr</li>
</ul>

<p>Andere gebuchte Termine bleiben bestehen.</p>

<p style=""margin-top:30px"">
Mit freundlichen Grüßen<br/>
<strong>Konsulat – Terminservice</strong>
</p>

</div>
</body>
</html>";
    }

    // =============================================================
    // HTML – VOLLSTÄNDIGE ABSAGE
    // =============================================================
    private static string BuildCancellationHtmlMailBody(
        string fullName,
        string bookingReference)
    {
        return $@"
<!DOCTYPE html>
<html lang=""de"">
<body style=""font-family: Arial; background:#f5f5f5; padding:20px"">
<div style=""max-width:600px; margin:auto; background:#fff; padding:24px; border-radius:8px"">

<h2 style=""color:#c62828"">Alle Termine abgesagt</h2>

<p>
<strong>Name:</strong> {fullName}<br/>
<strong>Buchungsnummer:</strong> {bookingReference}
</p>

<p>
Sie können jederzeit einen neuen Termin über unser Online-Terminportal buchen.
</p>

<p style=""margin-top:30px"">
Mit freundlichen Grüßen<br/>
<strong>Konsulat – Terminservice</strong>
</p>

</div>
</body>
</html>";
    }

    // =============================================================
    // HTML – MITARBEITER
    // =============================================================
    private static string BuildEmployeeWelcomeHtmlMailBody(
        string fullName,
        string employeeCode,
        string temporaryPassword,
        string changePasswordLink)
    {
        return $@"
<!DOCTYPE html>
<html lang=""de"">
<body style=""font-family: Arial; background:#f5f5f5; padding:20px"">
<div style=""max-width:600px; margin:auto; background:#fff; padding:24px; border-radius:8px"">

<h2 style=""color:#1565c0"">Willkommen im Konsulat</h2>

<p>
<strong>Name:</strong> {fullName}<br/>
<strong>Mitarbeiter-Kennung:</strong> {employeeCode}<br/>
<strong>Temporäres Passwort:</strong> {temporaryPassword}
</p>

<p style=""text-align:center; margin:30px 0"">
<a href=""{changePasswordLink}""
style=""background:#1565c0;color:#fff;padding:12px 20px;
text-decoration:none;border-radius:6px;font-weight:bold"">
Passwort ändern
</a>
</p>

</div>
</body>
</html>";
    }

    public async Task SendEmployeePasswordResetEmailAsync(
        string toEmail,
        string fullName,
        string resetLink)
    {
        if (!TryGetEmailConfig(out var email))
        {
            return;
        }

        using var smtpClient = CreateSmtpClient(email);

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(email.FromEmail, email.FromName),
            Subject = "Passwort zurücksetzen – Konsulat",
            Body = BuildEmployeePasswordResetHtmlMailBody(fullName, resetLink),
            IsBodyHtml = true
        };

        mailMessage.To.Add(toEmail);
        await smtpClient.SendMailAsync(mailMessage);
    }

    private static string BuildEmployeePasswordResetHtmlMailBody(
        string fullName,
        string resetLink)
    {
        return $@"
<!DOCTYPE html>
<html lang=""de"">
<body style=""font-family: Arial; background:#f5f5f5; padding:20px"">
<div style=""max-width:600px; margin:auto; background:#fff; padding:24px; border-radius:8px"">

<h2 style=""color:#1565c0"">Passwort zurücksetzen</h2>

<p>Guten Tag {fullName},</p>

<p>Sie haben eine Zurücksetzung Ihres Passworts angefordert.</p>

<p style=""text-align:center; margin:30px 0"">
<a href=""{resetLink}""
style=""background:#1565c0;color:#fff;padding:12px 20px;
text-decoration:none;border-radius:6px;font-weight:bold"">
Passwort zurücksetzen
</a>
</p>

<p style=""font-size:12px;color:#888"">
Dieser Link ist 1 Stunde gültig. Wenn Sie die Zurücksetzung nicht angefordert haben, ignorieren Sie diese E-Mail.
</p>

<p style=""margin-top:30px"">
Mit freundlichen Grüßen<br/>
<strong>Konsulat – Terminservice</strong>
</p>

</div>
</body>
</html>";
    }

    private static string BuildEmployeePasswordChangedHtmlMailBody(
        string fullName,
        string loginLink)
    {
        return $@"
<!DOCTYPE html>
<html lang=""de"">
<body style=""font-family: Arial; background:#f5f5f5; padding:20px"">
<div style=""max-width:600px; margin:auto; background:#fff; padding:24px; border-radius:8px"">

<h2 style=""color:#2e7d32"">Passwort geändert</h2>

<p>Ihr Passwort wurde erfolgreich geändert.</p>

<p style=""text-align:center; margin:30px 0"">
<a href=""{loginLink}""
style=""background:#2e7d32;color:#fff;padding:12px 20px;
text-decoration:none;border-radius:6px;font-weight:bold"">
Zum Login
</a>
</p>

</div>
</body>
</html>";
    }

    // =============================================================
    // HELPER
    // =============================================================
    /// <summary>Liest E-Mail-Konfiguration. Gibt false zurück, wenn SMTP nicht konfiguriert (z. B. leere User Secrets).</summary>
    private bool TryGetEmailConfig(
        out (string SmtpServer, int Port, bool UseSsl,
             string Username, string Password,
             string FromEmail, string FromName) config)
    {
        var c = _configuration.GetSection("Email");
        var smtpServer = c["SmtpServer"] ?? string.Empty;

        if (string.IsNullOrWhiteSpace(smtpServer))
        {
            config = default;
            return false;
        }

        config = (
            smtpServer,
            int.TryParse(c["Port"], out var port) ? port : 587,
            bool.TryParse(c["UseSsl"], out var useSsl) && useSsl,
            c["Username"] ?? string.Empty,
            c["Password"] ?? string.Empty,
            c["FromEmail"] ?? string.Empty,
            c["FromName"] ?? "Konsulat – Terminservice"
        );
        return true;
    }

    private static SmtpClient CreateSmtpClient(
        (string SmtpServer, int Port, bool UseSsl,
         string Username, string Password,
         string FromEmail, string FromName) e)
    {
        return new SmtpClient
        {
            Host = e.SmtpServer,
            Port = e.Port,
            EnableSsl = e.UseSsl,
            Credentials = new NetworkCredential(e.Username, e.Password)
        };
    }
}
