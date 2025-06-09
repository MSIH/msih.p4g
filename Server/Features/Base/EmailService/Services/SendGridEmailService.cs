using msih.p4g.Server.Common.Utilities;
using msih.p4g.Server.Features.Base.EmailService.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;

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

        public async Task SendEmailAsync(string to, string from, string subject, string htmlContent)
        {
            if (!IsValidEmail(to))
            {
                _logger.LogError($"Invalid recipient email address: {to}");
                throw new ArgumentException("Invalid recipient email address", nameof(to));
            }

            if (!string.IsNullOrWhiteSpace(from) && !IsValidEmail(from))
            {
                _logger.LogError($"Invalid sender email address: {from}");
                throw new ArgumentException("Invalid sender email address", nameof(from));
            }

            var client = new SendGridClient(_apiKey);

            // Use the from parameter if provided, otherwise fall back to configuration
            var senderEmail = !string.IsNullOrWhiteSpace(from) ? from : _fromEmail;
            var fromAddress = new EmailAddress(senderEmail, _fromName);

            var toEmail = new EmailAddress(to);
            var msg = MailHelper.CreateSingleEmail(fromAddress, toEmail, subject, null, htmlContent);
            var response = await client.SendEmailAsync(msg);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Body.ReadAsStringAsync();
                _logger.LogError($"Failed to send email: {response.StatusCode} - {body}");
            }
        }

        /// <summary>
        /// Validates if the provided email address is in a valid format.
        /// </summary>
        /// <param name="email">The email address to validate.</param>
        /// <returns>True if the email is valid, false otherwise.</returns>
        public bool IsValidEmail(string email)
        {
            return ValidationUtilities.IsValidEmail(email);
        }
    }
}