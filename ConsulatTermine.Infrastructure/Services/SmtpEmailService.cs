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

        public async Task SendBookingConfirmationAsync(
            string toEmail,
            string fullName,
            string bookingReference)
        {
            var emailConfig = _configuration.GetSection("Email");

            var smtpServer = emailConfig["SmtpServer"]
                ?? throw new InvalidOperationException("Email:SmtpServer fehlt in der Konfiguration.");

            var port = int.Parse(emailConfig["Port"]
                ?? throw new InvalidOperationException("Email:Port fehlt in der Konfiguration."));

            var useSsl = bool.Parse(emailConfig["UseSsl"]
                ?? throw new InvalidOperationException("Email:UseSsl fehlt in der Konfiguration."));

            var username = emailConfig["Username"]
                ?? throw new InvalidOperationException("Email:Username fehlt in der Konfiguration.");

            var password = emailConfig["Password"]
                ?? throw new InvalidOperationException("Email:Password fehlt in der Konfiguration.");

            var fromEmail = emailConfig["FromEmail"]
                ?? throw new InvalidOperationException("Email:FromEmail fehlt in der Konfiguration.");

            var fromName = emailConfig["FromName"] ?? "Konsulat – Terminservice";

            var cancelUrl =
                $"http://localhost:5262/appointment-cancel?ref={bookingReference}";

            using var smtpClient = new SmtpClient
            {
                Host = smtpServer,
                Port = port,
                EnableSsl = useSsl,
                Credentials = new NetworkCredential(username, password)
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = "Terminbestätigung – Konsulat",
                Body = BuildHtmlMailBody(fullName, bookingReference, cancelUrl),
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }

        private static string BuildHtmlMailBody(
            string fullName,
            string bookingReference,
            string cancelUrl)
        {
            return $@"
<!DOCTYPE html>
<html lang=""de"">
<head>
    <meta charset=""utf-8"" />
    <title>Terminbestätigung</title>
</head>
<body style=""font-family: Arial, Helvetica, sans-serif; background-color: #f5f5f5; padding: 20px;"">
    <div style=""max-width: 600px; margin: auto; background-color: #ffffff; padding: 24px; border-radius: 8px;"">

        <h2 style=""color: #2e7d32;"">Terminbestätigung</h2>

        <p>Sehr geehrte Damen und Herren,</p>

        <p>
            Ihr Termin beim <strong>Konsulat</strong> wurde erfolgreich registriert.
        </p>

        <p>
            <strong>Name:</strong> {fullName}<br />
            <strong>Buchungsnummer:</strong> {bookingReference}
        </p>

        <p>
            Bitte bewahren Sie diese Buchungsnummer gut auf.
            Sie benötigen sie für Rückfragen oder zur Terminabsage.
        </p>

        <hr style=""margin: 24px 0;"" />

        <p>
            Falls Sie Ihren Termin nicht wahrnehmen können, können Sie ihn über den folgenden Button absagen:
        </p>

        <p style=""text-align: center; margin: 30px 0;"">
            <a href=""{cancelUrl}""
               style=""background-color: #c62828;
                      color: #ffffff;
                      padding: 12px 20px;
                      text-decoration: none;
                      border-radius: 6px;
                      font-weight: bold;
                      display: inline-block;"">
                Termin absagen
            </a>
        </p>

        <p style=""font-size: 14px; color: #555;"">
            Bitte sagen Sie Ihren Termin rechtzeitig ab, damit andere Personen diesen Termin nutzen können.
        </p>

        <p style=""margin-top: 30px;"">
            Mit freundlichen Grüßen<br />
            <strong>Konsulat – Terminservice</strong>
        </p>

        <p style=""font-size: 12px; color: #888; margin-top: 20px;"">
            Dies ist eine automatisch generierte E-Mail. Bitte antworten Sie nicht auf diese Nachricht.
        </p>
    </div>
</body>
</html>";
        }
    }
}
