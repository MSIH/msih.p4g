/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using msih.p4g.Server.Common.Utilities;
using MSIH.Core.Services.Email.Interfaces;
using msih.p4g.Server.Features.Base.SettingsService.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace MSIH.Core.Services.Email.Services
{
    public class SendGridEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ISettingsService _settingsService;
        private readonly ILogger<SendGridEmailService> _logger;

        // Cache for settings to avoid DB lookups on every email
        private string _apiKey;
        private string _fromEmail;
        private string _fromName;
        private bool _settingsInitialized = false;

        public SendGridEmailService(
            IConfiguration configuration, 
            ISettingsService settingsService,
            ILogger<SendGridEmailService> logger)
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
            _apiKey = await _settingsService.GetValueAsync("SendGrid:ApiKey") 
                ?? throw new Exception("SendGrid API key not configured");
            
            _fromEmail = await _settingsService.GetValueAsync("SendGrid:FromEmail") 
                ?? throw new Exception("SendGrid FromEmail not configured");
            
            _fromName = await _settingsService.GetValueAsync("SendGrid:FromName") 
                ?? "NoReply";

            _settingsInitialized = true;
        }

        public async Task SendEmailAsync(string to, string from, string subject, string htmlContent)
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