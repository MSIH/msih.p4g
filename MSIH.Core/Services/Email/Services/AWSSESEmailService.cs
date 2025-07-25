/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MSIH.Core.Common.Utilities;
using MSIH.Core.Services.Email.Interfaces;
using MSIH.Core.Services.Settings.Interfaces;
using System.Threading.Tasks;

namespace MSIH.Core.Services.Email.Services
{
    public class AWSSESEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ISettingsService _settingsService;
        private readonly ILogger<AWSSESEmailService> _logger;

        // Cache for settings to avoid DB lookups on every email
        private string _accessKey;
        private string _secretKey;
        private string _region;
        private string _fromEmail;
        private string _fromName;
        private bool _settingsInitialized = false;

        public AWSSESEmailService(
            IConfiguration configuration,
            ISettingsService settingsService,
            ILogger<AWSSESEmailService> logger)
        {
            _configuration = configuration;
            _settingsService = settingsService;
            _logger = logger;
        }

        private async Task InitializeSettingsAsync()
        {
            if (_settingsInitialized)
                return;

            // Try to get settings from the settings service (DB first, then appsettings, then environment)
            _accessKey = await _settingsService.GetValueAsync("AWS:SES:AccessKey")
                ?? throw new Exception("AWS SES AccessKey not configured");

            _secretKey = await _settingsService.GetValueAsync("AWS:SES:SecretKey")
                ?? throw new Exception("AWS SES SecretKey not configured");

            _region = await _settingsService.GetValueAsync("AWS:SES:Region")
                ?? "us-east-1";

            _fromEmail = await _settingsService.GetValueAsync("AWS:SES:FromEmail")
                ?? throw new Exception("AWS SES FromEmail not configured");

            _fromName = await _settingsService.GetValueAsync("AWS:SES:FromName")
                ?? "NoReply";

            _settingsInitialized = true;
        }

        public async Task SendEmailAsync(string to, string from, string subject, string htmlContent)
        {
            try
            {
                await InitializeSettingsAsync();

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
                    Message = new Amazon.SimpleEmail.Model.Message
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
