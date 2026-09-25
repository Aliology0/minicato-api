using AlMostashar.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace AlMostashar.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
        {
            var emailSettings = _configuration.GetSection("Email");

            var smtpHost    = emailSettings["SmtpHost"] ?? throw new InvalidOperationException("SMTP Host is not configured.");
            var smtpPort    = int.Parse(emailSettings["SmtpPort"] ?? "587");
            var senderEmail = emailSettings["SenderEmail"] ?? throw new InvalidOperationException("Sender Email is not configured.");
            var senderName  = emailSettings["SenderName"] ?? "AlMostashar";
            var appPassword = emailSettings["AppPassword"] ?? throw new InvalidOperationException("App Password is not configured.");

            using var message = new MailMessage
            {
                From       = new MailAddress(senderEmail, senderName),
                Subject    = subject,
                Body       = htmlBody,
                IsBodyHtml = true,
            };

            message.To.Add(new MailAddress(toEmail));

            using var smtp = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(senderEmail, appPassword),
                EnableSsl   = true,
            };

            try
            {
                await smtp.SendMailAsync(message, cancellationToken);
                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                throw;
            }
        }
    }
}
