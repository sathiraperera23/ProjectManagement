using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using TaskManagementApi.Application.Interfaces;

namespace TaskManagementApi.Infrastructure.Services
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

        public async Task SendEmailAsync(string to, string subject, string htmlBody, string? plainTextBody = null)
        {
            var message = new EmailMessage
            {
                To = to,
                Subject = subject,
                HtmlBody = htmlBody,
                PlainTextBody = plainTextBody
            };
            await SendEmailAsync(message);
        }

        public async Task SendEmailAsync(EmailMessage message)
        {
            try
            {
                var email = new MimeMessage();
                var fromAddress = message.FromAddress ?? _configuration["Email:FromAddress"];
                var fromDisplayName = message.FromDisplayName ?? _configuration["Email:FromDisplayName"];
                email.From.Add(new MailboxAddress(fromDisplayName, fromAddress));

                foreach (var to in message.To.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    email.To.Add(MailboxAddress.Parse(to.Trim()));
                }

                if (!string.IsNullOrEmpty(message.Cc))
                {
                    foreach (var cc in message.Cc.Split(',', StringSplitOptions.RemoveEmptyEntries))
                        email.Cc.Add(MailboxAddress.Parse(cc.Trim()));
                }

                if (!string.IsNullOrEmpty(message.Bcc))
                {
                    foreach (var bcc in message.Bcc.Split(',', StringSplitOptions.RemoveEmptyEntries))
                        email.Bcc.Add(MailboxAddress.Parse(bcc.Trim()));
                }

                email.Subject = message.Subject;

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = message.HtmlBody;
                if (!string.IsNullOrEmpty(message.PlainTextBody))
                {
                    bodyBuilder.TextBody = message.PlainTextBody;
                }
                email.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                client.Timeout = int.Parse(_configuration["Email:ConnectionTimeout"] ?? "5000");

                var host = _configuration["Email:Host"];
                var port = int.Parse(_configuration["Email:Port"] ?? "587");
                var useSsl = bool.Parse(_configuration["Email:EnableSsl"] ?? "false");

                await client.ConnectAsync(host, port, useSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_configuration["Email:Username"], _configuration["Email:Password"]);
                await client.SendAsync(email);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {To}", message.To);
            }
        }
    }
}
