using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using msih.p4g.Server.Common.Utilities;
using msih.p4g.Server.Features.Base.EmailService.Interfaces;
using System.Threading.Tasks;

namespace msih.p4g.Server.Features.Base.EmailService.Services
{
    public class AWSSESEmailService : IEmailService
    {
        private readonly string _accessKey;
        private readonly string _secretKey;
        private readonly string _region;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly ILogger<AWSSESEmailService> _logger;

        public AWSSESEmailService(IConfiguration configuration, ILogger<AWSSESEmailService> logger)
        {
            _accessKey = configuration["AWS:SES:AccessKey"] ?? throw new Exception("AWS SES AccessKey not configured");
            _secretKey = configuration["AWS:SES:SecretKey"] ?? throw new Exception("AWS SES SecretKey not configured");
            _region = configuration["AWS:SES:Region"] ?? "us-east-1";
            _fromEmail = configuration["AWS:SES:FromEmail"] ?? throw new Exception("AWS SES FromEmail not configured");
            _fromName = configuration["AWS:SES:FromName"] ?? "NoReply";
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string from, string subject, string htmlContent)
        {
            try
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

                // Use the from parameter if provided, otherwise fall back to configuration
                var senderEmail = !string.IsNullOrWhiteSpace(from) ? from : _fromEmail;

                // Create the Amazon SES client
                using var client = new AmazonSimpleEmailServiceClient(
                    _accessKey,
                    _secretKey,
                    RegionEndpoint.GetBySystemName(_region)
                );

                // Create the email message
                var sendRequest = new SendEmailRequest
                {
                    Source = $"{_fromName} <{senderEmail}>",
                    Destination = new Destination { ToAddresses = new List<string> { to } },
                    Message = new Message
                    {
                        Subject = new Content(subject),
                        Body = new Body
                        {
                            Html = new Content
                            {
                                Charset = "UTF-8",
                                Data = htmlContent
                            }
                        }
                    }
                };

                // Send the email
                var response = await client.SendEmailAsync(sendRequest);
                _logger.LogInformation($"Email sent with message ID: {response.MessageId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email using AWS SES");
                throw;
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