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
using msih.p4g.Server.Features.Base.SettingsService.Interfaces;
using msih.p4g.Server.Features.Base.SmsService.Interfaces;
using msih.p4g.Server.Features.Base.SmsService.Model;
using Newtonsoft.Json;
using Twilio;
using Twilio.Exceptions;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Rest.Lookups.V2;
using Twilio.Types;

namespace msih.p4g.Server.Features.Base.SmsService.Services
{
    public class TwilioSmsService : ISmsService
    {
        private readonly IConfiguration _configuration;
        private readonly ISettingsService _settingsService;
        private readonly ILogger<TwilioSmsService> _logger;
        private readonly IValidatedPhoneNumberRepository _phoneNumberRepository;

        // Cache for settings to avoid DB lookups on every SMS
        private string _accountSid;
        private string _authToken;
        private string _fromNumber;
        private bool _settingsInitialized = false;

        public TwilioSmsService(
            IConfiguration configuration,
            ISettingsService settingsService,
            ILogger<TwilioSmsService> logger,
            IValidatedPhoneNumberRepository phoneNumberRepository)
        {
            _configuration = configuration;
            _settingsService = settingsService;
            _logger = logger;
            _phoneNumberRepository = phoneNumberRepository ?? throw new ArgumentNullException(nameof(phoneNumberRepository));
        }

        private async Task InitializeSettingsAsync()
        {
            if (_settingsInitialized)
                return;

            // Try to get settings from the settings service (DB first, then appsettings, then environment)
            _accountSid = await _settingsService.GetValueAsync("Twilio:AccountSid")
                ?? throw new Exception("Twilio AccountSid not configured");

            _authToken = await _settingsService.GetValueAsync("Twilio:AuthToken")
                ?? throw new Exception("Twilio AuthToken not configured");

            _fromNumber = await _settingsService.GetValueAsync("Twilio:FromNumber")
                ?? throw new Exception("Twilio FromNumber not configured");

            // Initialize Twilio client
            TwilioClient.Init(_accountSid, _authToken);

            _settingsInitialized = true;
        }

        /// <summary>
        /// Sends an SMS message to the specified phone number.
        /// </summary>
        /// <param name="to">The recipient's phone number in E.164 format (e.g., +12125551234).</param>
        /// <param name="message">The message content to send.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SendSmsAsync(string to, string message)
        {
            try
            {
                await InitializeSettingsAsync();

                if (!IsValidPhoneNumber(to))
                {
                    _logger.LogError($"Invalid phone number format: {to}");
                    throw new ArgumentException("Phone number must be in E.164 format (e.g., +12125551234)", nameof(to));
                }

                // Create the message
                var messageOptions = new CreateMessageOptions(new PhoneNumber(to))
                {
                    From = new PhoneNumber(_fromNumber),
                    Body = message
                };

                // Send the message
                var response = await MessageResource.CreateAsync(messageOptions);
                _logger.LogInformation($"SMS sent with SID: {response.Sid}, Status: {response.Status}");

                // Check for potential issues
                if (response.ErrorMessage != null)
                {
                    _logger.LogWarning($"SMS sent with warning: {response.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SMS using Twilio");
                throw;
            }
        }

        /// <summary>
        /// Validates if the provided phone number is in a valid format.
        /// </summary>
        /// <param name="phoneNumber">The phone number to validate.</param>
        /// <returns>True if the phone number is valid, false otherwise.</returns>
        public bool IsValidPhoneNumber(string phoneNumber)
        {
            return ValidationUtilities.IsValidPhoneNumber(phoneNumber);
        }

        /// <summary>
        /// Validates a phone number using Twilio's Lookup service to determine if it is a valid mobile number.
        /// Results are stored in the database for future reference.
        /// </summary>
        /// <param name="phoneNumber">The phone number to validate in E.164 format.</param>
        /// <param name="useCache">Whether to use cached validation results if available.</param>
        /// <param name="usePaidService">Whether to use Twilio's paid carrier lookup service for advanced validation.</param>
        /// <returns>A validated phone number object with validation results.</returns>
        public async Task<ValidatedPhoneNumber> ValidatePhoneNumberAsync(string phoneNumber, bool useCache = true, bool usePaidService = false)
        {
            await InitializeSettingsAsync();

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ArgumentException("Phone number cannot be null or empty", nameof(phoneNumber));
            }
            if (!IsValidPhoneNumber(phoneNumber))
            {
                _logger.LogWarning($"Invalid phone number format: {phoneNumber}");
                return new ValidatedPhoneNumber
                {
                    PhoneNumber = phoneNumber,
                    IsValid = false,
                    ValidatedOn = DateTime.UtcNow,
                    CreatedBy = "System"
                };
            }

            try
            {
                // Check if we already have this phone number validated in the database
                if (useCache)
                {
                    var cachedResult = await _phoneNumberRepository.GetByPhoneNumberAsync(phoneNumber);
                    if (cachedResult != null)
                    {
                        // Check if the cached result is recent enough (e.g., validated within the last 30 days)
                        /* if ((DateTime.UtcNow - cachedResult.ValidatedOn).TotalDays < 30)
                        {
                            _logger.LogInformation($"Using cached validation result for {phoneNumber}");
                            return cachedResult;
                        } */
                    }
                }

                // Use the actual phoneNumber parameter and request line_type_intelligence
                var phoneNumberLookup = await PhoneNumberResource.FetchAsync(
                    pathPhoneNumber: phoneNumber,
                    fields: "line_type_intelligence"
                );

                var validatedNumber = new ValidatedPhoneNumber
                {
                    PhoneNumber = phoneNumber,
                    IsValid = phoneNumberLookup?.Valid ?? false,
                    ValidatedOn = DateTime.UtcNow,
                    CountryCode = phoneNumberLookup?.CountryCode,
                    CreatedBy = "System"
                };

                // Check for line_type_intelligence and set IsMobile/Carrier accordingly
                if (phoneNumberLookup?.LineTypeIntelligence != null)
                {
                    var LineTypeIntelligence = JsonConvert.DeserializeObject<Dictionary<string, string>>(phoneNumberLookup.LineTypeIntelligence.ToString());
                    var type = LineTypeIntelligence.TryGetValue("type", out var phoneNumbertype) ? phoneNumbertype : null;
                    var carrierName = LineTypeIntelligence.TryGetValue("carrier_name", out var phoneNumberCarrier) ? phoneNumberCarrier : null;
                    validatedNumber.IsMobile = type == "mobile";
                    validatedNumber.Carrier = carrierName;
                }
                else
                {
                    validatedNumber.IsMobile = false;
                    validatedNumber.Carrier = null;
                }

                // Save the validated number to the database
                await _phoneNumberRepository.AddOrUpdateAsync(validatedNumber);

                _logger.LogInformation($"Phone number {phoneNumber} validated. IsMobile: {validatedNumber.IsMobile}");
                return validatedNumber;
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, $"Error validating phone number {phoneNumber} with Twilio Lookup service");
                // Create an invalid record for this phone number
                var invalidNumber = new ValidatedPhoneNumber
                {
                    PhoneNumber = phoneNumber,
                    IsValid = false,
                    ValidatedOn = DateTime.UtcNow,
                    CreatedBy = "System"
                };

                // Save the invalid number to the database
                await _phoneNumberRepository.AddOrUpdateAsync(invalidNumber);

                return invalidNumber;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error validating phone number {phoneNumber}");
                throw;
            }
        }
    }
}
