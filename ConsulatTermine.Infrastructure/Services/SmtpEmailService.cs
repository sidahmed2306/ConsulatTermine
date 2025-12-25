using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using ConsulatTermine.Application.Interfaces;

namespace ConsulatTermine.Infrastructure.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public SmtpEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // -------------------------------------------------------------
        // TERMINBESTÄTIGUNG
        // -------------------------------------------------------------
        public async Task SendBookingConfirmationAsync(
            string toEmail,
            string fullName,
            string bookingReference,
            string cancelToken)
        {
            var email = GetEmailConfig();

            var cancelUrl =
                $"http://localhost:5262/appointment-cancel?ref={bookingReference}&token={cancelToken}";

            using var smtpClient = CreateSmtpClient(email);

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(email.FromEmail, email.FromName),
                Subject = "Terminbestätigung – Konsulat",
                Body = BuildHtmlMailBody(
                    fullName,
                    bookingReference,
                    cancelUrl,
                    "<p>Die Details Ihrer gebuchten Services entnehmen Sie bitte dem Terminportal.</p>"),
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);
            await smtpClient.SendMailAsync(mailMessage);
        }

        // -------------------------------------------------------------
        // TEIL-ABSAGE
        // -------------------------------------------------------------
        public async Task SendPartialCancellationAsync(
            string toEmail,
            string fullName,
            string serviceName,
            DateTime date)
        {
            var email = GetEmailConfig();
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

        // -------------------------------------------------------------
        // VOLLSTÄNDIGE ABSAGE
        // -------------------------------------------------------------
        public async Task SendCancellationConfirmationAsync(
            string toEmail,
            string fullName,
            string bookingReference)
        {
            var email = GetEmailConfig();
            using var smtpClient = CreateSmtpClient(email);

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(email.FromEmail, email.FromName),
                Subject = "Alle Termine abgesagt – Konsulat",
                Body = BuildCancellationHtmlMailBody(fullName, bookingReference),
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);
            await smtpClient.SendMailAsync(mailMessage);
        }

        // -------------------------------------------------------------
// MITARBEITER – PASSWORT GEÄNDERT (E-Mail 2)
// -------------------------------------------------------------
public async Task SendEmployeePasswordChangedConfirmationEmailAsync(
    string toEmail,
    string fullName,
    string loginLink)
{
    var email = GetEmailConfig();
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


// -------------------------------------------------------------
// MITARBEITER – WILLKOMMEN (E-Mail 1)
// -------------------------------------------------------------
public async Task SendEmployeeWelcomeEmailAsync(
    string toEmail,
    string fullName,
    string employeeCode,
    string temporaryPassword,
    string changePasswordLink)
{
    var email = GetEmailConfig();
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
        // HTML BUILDER
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

<p>Der folgende Termin wurde abgesagt:</p>

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

<p>Sehr geehrte Damen und Herren,</p>

<p>
Alle Termine zu Ihrer Buchung wurden erfolgreich abgesagt.
</p>

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

<p>Sehr geehrte Damen und Herren,</p>

<p>
Ihr Mitarbeiterzugang wurde erfolgreich erstellt.
</p>

<p>
<strong>Name:</strong> {fullName}<br/>
<strong>Mitarbeiter-Kennung:</strong> {employeeCode}<br/>
<strong>Temporäres Passwort:</strong> {temporaryPassword}
</p>

<p style=""margin-top:20px"">
Bitte ändern Sie Ihr Passwort über den folgenden Button:
</p>

<p style=""text-align:center; margin:30px 0"">
<a href=""{changePasswordLink}""
style=""background:#1565c0;color:#fff;padding:12px 20px;
text-decoration:none;border-radius:6px;font-weight:bold"">
Passwort ändern
</a>
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

<p>Sehr geehrte Damen und Herren,</p>

<p>
Ihr Passwort wurde erfolgreich geändert.
</p>

<p style=""margin-top:20px"">
Sie können sich nun über den folgenden Button anmelden:
</p>

<p style=""text-align:center; margin:30px 0"">
<a href=""{loginLink}""
style=""background:#2e7d32;color:#fff;padding:12px 20px;
text-decoration:none;border-radius:6px;font-weight:bold"">
Zum Login
</a>
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
        // HELPER
        // =============================================================

        private (string SmtpServer, int Port, bool UseSsl, string Username,
                 string Password, string FromEmail, string FromName)
            GetEmailConfig()
        {
            var c = _configuration.GetSection("Email");

            return (
                c["SmtpServer"]!,
                int.Parse(c["Port"]!),
                bool.Parse(c["UseSsl"]!),
                c["Username"]!,
                c["Password"]!,
                c["FromEmail"]!,
                c["FromName"] ?? "Konsulat – Terminservice"
            );
        }

        private static SmtpClient CreateSmtpClient(
            (string SmtpServer, int Port, bool UseSsl, string Username,
             string Password, string FromEmail, string FromName) e)
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
}
