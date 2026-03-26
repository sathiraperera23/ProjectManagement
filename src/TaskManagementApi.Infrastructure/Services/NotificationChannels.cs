using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            // Placeholder for SendGrid / SMTP
            var apiKey = _configuration["SendGrid:ApiKey"];
            _logger.LogInformation("Sending email to {To}: {Subject}", to, subject);
            await Task.CompletedTask;
        }
    }

    public class SmsService : ISmsService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmsService> _logger;

        public SmsService(IConfiguration configuration, ILogger<SmsService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendSmsAsync(string to, string message)
        {
            // Placeholder for Twilio
            var accountSid = _configuration["Twilio:AccountSid"];
            _logger.LogInformation("Sending SMS to {To}: {Message}", to, message);
            await Task.CompletedTask;
        }
    }
}
