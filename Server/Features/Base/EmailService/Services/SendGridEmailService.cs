using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using msih.p4g.Server.Features.Base.EmailService.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace msih.p4g.Server.Features.Base.EmailService.Services
{
    public class SendGridEmailService : IEmailService
    {
        private readonly string _apiKey;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly ILogger<SendGridEmailService> _logger;

        public SendGridEmailService(IConfiguration configuration, ILogger<SendGridEmailService> logger)
        {
            _apiKey = configuration["SendGrid:ApiKey"] ?? throw new Exception("SendGrid API key not configured");
            _fromEmail = configuration["SendGrid:FromEmail"] ?? throw new Exception("SendGrid FromEmail not configured");
            _fromName = configuration["SendGrid:FromName"] ?? "NoReply";
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlContent)
        {
            var client = new SendGridClient(_apiKey);
            var from = new EmailAddress(_fromEmail, _fromName);
            var toEmail = new EmailAddress(to);
            var msg = MailHelper.CreateSingleEmail(from, toEmail, subject, null, htmlContent);
            var response = await client.SendEmailAsync(msg);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Body.ReadAsStringAsync();
                _logger.LogError($"Failed to send email: {response.StatusCode} - {body}");
            }
        }
    }
}