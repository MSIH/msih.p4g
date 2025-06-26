// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using msih.p4g.Server.Common.Utilities;
using msih.p4g.Server.Features.Base.EmailService.Interfaces;
using msih.p4g.Server.Features.Base.SettingsService.Interfaces;
using Smtp2Go.Api;
using Smtp2Go.Api.Models.Emails;
using System.Text.RegularExpressions;

namespace msih.p4g.Server.Features.Base.EmailService.Services
{
    /// <summary>
    /// Implementation of IEmailService that uses SMTP2GO for sending emails
    /// </summary>
    public class Smtp2GoEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ISettingsService _settingsService;
        private readonly ILogger<Smtp2GoEmailService> _logger;

        // Cache for settings to avoid DB lookups on every email
        private string _apiKey;
        private string _fromEmail;
        private string _fromName;
        private bool _settingsInitialized = false;

        /// <summary>
        /// Initializes a new instance of the Smtp2GoEmailService class
        /// </summary>
        public Smtp2GoEmailService(
            IConfiguration configuration,
            ISettingsService settingsService,
            ILogger<Smtp2GoEmailService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Initializes the settings for the SMTP2GO service
        /// </summary>
        private async Task InitializeSettingsAsync()
        {
            if (_settingsInitialized)
                return;

            // Try to get settings from the settings service (DB first, then appsettings, then environment)
            _apiKey = await _settingsService.GetValueAsync("SMTP2GO:ApiKey")
                ?? throw new Exception("SMTP2GO API key not configured");

            _fromEmail = await _settingsService.GetValueAsync("SMTP2GO:FromEmail")
                ?? throw new Exception("SMTP2GO FromEmail not configured");

            _fromName = await _settingsService.GetValueAsync("SMTP2GO:FromName")
                ?? throw new Exception("SMTP2GO FromName not configured");

            _settingsInitialized = true;
        }

        /// <summary>
        /// Sends an email using the SMTP2GO service
        /// </summary>
        /// <param name="to">Recipient email address</param>
        /// <param name="from">Sender email address (optional)</param>
        /// <param name="subject">Email subject</param>
        /// <param name="htmlContent">HTML content of the email</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task SendEmailAsync(string to, string from, string subject, string htmlContent)
        {
            try
            {
                await InitializeSettingsAsync();

                if (!IsValidEmail(to))
                {
                    _logger.LogError("Invalid recipient email address: {To}", to);
                    throw new ArgumentException("Invalid recipient email address", nameof(to));
                }

                if (!string.IsNullOrWhiteSpace(from) && !IsValidEmail(from))
                {
                    _logger.LogError("Invalid sender email address: {From}", from);
                    throw new ArgumentException("Invalid sender email address", nameof(from));
                }

                // Use the from parameter if provided, otherwise fall back to configuration
                var senderEmail = !string.IsNullOrWhiteSpace(from) ? from : _fromEmail;

                var service = new Smtp2GoApiService(_apiKey);

                var message = new EmailMessage(senderEmail, to)
                {
                    BodyHtml = htmlContent,
                    BodyText = HtmlToPlainText(htmlContent),
                    Subject = subject
                };

                _logger.LogDebug("Sending email via SMTP2GO to {To} with subject: {Subject}", to, subject);

                var response = await service.SendEmail(message);

                await ProcessEmailResponseAsync(response, to, subject);
            }
            catch (Exception ex) when (ex is not ArgumentException)
            {
                _logger.LogError(ex, "Error sending email to {To} with subject: {Subject}", to, subject);
                throw new Exception($"Error sending email: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Processes the email response from SMTP2GO and handles errors appropriately
        /// </summary>
        /// <param name="response">The email response from SMTP2GO</param>
        /// <param name="to">Recipient email address for logging</param>
        /// <param name="subject">Email subject for logging</param>
        private async Task ProcessEmailResponseAsync(EmailResponse response, string to, string subject)
        {
            if (response == null)
            {
                _logger.LogError("Received null response from SMTP2GO for email to {To}", to);
                throw new Exception("Received null response from SMTP2GO");
            }

            _logger.LogDebug("SMTP2GO Response Status: {ResponseStatus}, Request ID: {RequestId}",
                response.ResponseStatus, response.RequestId);

            // Handle HTTP response status
            if (!string.IsNullOrEmpty(response.ResponseStatus))
            {
                await HandleHttpStatusAsync(response.ResponseStatus, response.RequestId, to, subject);
            }

            // Handle API response data
            if (response.Data != null)
            {
                await HandleApiResponseDataAsync(response.Data, response.RequestId, to, subject);
            }
            else
            {
                _logger.LogWarning("SMTP2GO response data is null for email to {To}, Request ID: {RequestId}",
                    to, response.RequestId);
            }
        }

        /// <summary>
        /// Handles HTTP status codes from SMTP2GO response
        /// </summary>
        private async Task HandleHttpStatusAsync(string responseStatus, string requestId, string to, string subject)
        {
            // Try to parse as HTTP status code
            if (int.TryParse(responseStatus, out int statusCode))
            {
                switch (statusCode)
                {
                    case 200:
                        _logger.LogInformation("Email sent successfully to {To}, Request ID: {RequestId}", to, requestId);
                        break;

                    case 400:
                        _logger.LogError("Bad Request (400) - Invalid parameters for email to {To}, Request ID: {RequestId}",
                            to, requestId);
                        throw new Exception("Bad Request: The request was unacceptable, likely due to missing required parameters");

                    case 401:
                        _logger.LogError("Unauthorized (401) - Invalid API key for email to {To}, Request ID: {RequestId}",
                            to, requestId);
                        throw new Exception("Unauthorized: No valid API key was provided");

                    case 402:
                        _logger.LogError("Request Failed (402) - Parameters valid but request failed for email to {To}, Request ID: {RequestId}",
                            to, requestId);
                        throw new Exception("Request Failed: The parameters were valid but the request failed");

                    case 403:
                        _logger.LogError("Forbidden (403) - API key lacks permissions for email to {To}, Request ID: {RequestId}",
                            to, requestId);
                        throw new Exception("Forbidden: The API key doesn't have permission to perform the request");

                    case 404:
                        _logger.LogError("Not Found (404) - Resource doesn't exist for email to {To}, Request ID: {RequestId}",
                            to, requestId);
                        throw new Exception("Not Found: The requested resource doesn't exist");

                    case 429:
                        _logger.LogWarning("Too Many Requests (429) - Rate limited for email to {To}, Request ID: {RequestId}",
                            to, requestId);
                        throw new Exception("Too Many Requests: Rate limit exceeded. Please implement exponential backoff");

                    case 500:
                    case 502:
                    case 503:
                    case 504:
                        _logger.LogError("Server Error ({StatusCode}) - SMTP2GO server error for email to {To}, Request ID: {RequestId}",
                            statusCode, to, requestId);
                        throw new Exception($"Server Error ({statusCode}): Something went wrong on SMTP2GO's end");

                    default:
                        _logger.LogWarning("Unexpected HTTP status code {StatusCode} for email to {To}, Request ID: {RequestId}",
                            statusCode, to, requestId);
                        if (statusCode >= 400)
                        {
                            throw new Exception($"HTTP Error {statusCode}: Unexpected error occurred");
                        }
                        break;
                }
            }
            else
            {
                _logger.LogDebug("Response status is not a numeric HTTP code: {ResponseStatus}, Request ID: {RequestId}",
                    responseStatus, requestId);
            }
        }

        /// <summary>
        /// Handles API response data from SMTP2GO
        /// </summary>
        private async Task HandleApiResponseDataAsync(EmailResponseData data, string requestId, string to, string subject)
        {
            // Check for API errors
            if (!string.IsNullOrEmpty(data.Error))
            {
                _logger.LogError("SMTP2GO API Error: {Error}, Error Code: {ErrorCode}, Request ID: {RequestId}, Email to: {To}",
                    data.Error, data.ErrorCode, requestId, to);

                // Handle specific error codes
                switch (data.ErrorCode)
                {
                    case "E_ApiResponseCodes.API_EXCEPTION":
                        throw new Exception($"API Exception: {data.Error}");
                    case "E_ApiResponseCodes.NON_VALIDATING_IN_PAYLOAD":
                        throw new Exception($"Validation Error: {data.Error}");
                    default:
                        throw new Exception($"SMTP2GO Error ({data.ErrorCode}): {data.Error}");
                }
            }

            // Log success metrics
            if (data.Succeeded > 0)
            {
                _logger.LogInformation("Successfully sent {Succeeded} email(s) to {To}, Request ID: {RequestId}",
                    data.Succeeded, to, requestId);
            }

            // Log failure metrics
            if (data.Failed > 0)
            {
                _logger.LogWarning("Failed to send {Failed} email(s) to {To}, Request ID: {RequestId}",
                    data.Failed, to, requestId);

                // If all emails failed, throw an exception
                if (data.Succeeded == 0)
                {
                    throw new Exception($"All email sends failed. Failed count: {data.Failed}");
                }
            }

            // Handle field validation errors if present
            await HandleFieldValidationErrorsAsync(data, requestId, to);
        }

        /// <summary>
        /// Handles field validation errors from SMTP2GO response
        /// </summary>
        private async Task HandleFieldValidationErrorsAsync(EmailResponseData data, string requestId, string to)
        {
            // Note: The SMTP2GO .NET client library may not expose field validation errors directly
            // This is a placeholder for potential future enhancement
            // In the raw API response, field_validation_errors would contain detailed validation information

            _logger.LogDebug("Field validation error handling completed for Request ID: {RequestId}", requestId);
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

        /// <summary>
        /// Converts HTML content to plain text
        /// </summary>
        /// <param name="html">The HTML content</param>
        /// <returns>Plain text version of the HTML content</returns>
        private string HtmlToPlainText(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            // Remove HTML tags
            var text = Regex.Replace(html, @"<[^>]+>", string.Empty);

            // Decode HTML entities
            text = System.Net.WebUtility.HtmlDecode(text);

            // Replace multiple spaces with a single space
            text = Regex.Replace(text, @"\s+", " ");

            // Replace multiple newlines with a single newline
            text = Regex.Replace(text, @"(\r\n)+", "\r\n");

            return text.Trim();
        }
    }
}
