using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using msih.p4g.Server.Common.Utilities;
using msih.p4g.Server.Features.Base.SmsService.Interfaces;
using msih.p4g.Shared.Models;
using System;
using System.Threading.Tasks;
using Twilio;
using Twilio.Exceptions;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Rest.Lookups.V1;
using Twilio.Types;

namespace msih.p4g.Server.Features.Base.SmsService.Services
{
    public class TwilioSmsService : ISmsService
    {
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _fromNumber;
        private readonly ILogger<TwilioSmsService> _logger;
        private readonly IValidatedPhoneNumberRepository _phoneNumberRepository;

        public TwilioSmsService(
            IConfiguration configuration, 
            ILogger<TwilioSmsService> logger,
            IValidatedPhoneNumberRepository phoneNumberRepository)
        {
            _accountSid = configuration["Twilio:AccountSid"] ?? throw new Exception("Twilio AccountSid not configured");
            _authToken = configuration["Twilio:AuthToken"] ?? throw new Exception("Twilio AuthToken not configured");
            _fromNumber = configuration["Twilio:FromNumber"] ?? throw new Exception("Twilio FromNumber not configured");
            _logger = logger;
            _phoneNumberRepository = phoneNumberRepository ?? throw new ArgumentNullException(nameof(phoneNumberRepository));

            // Initialize Twilio client
            TwilioClient.Init(_accountSid, _authToken);
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
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ArgumentException("Phone number cannot be null or empty", nameof(phoneNumber));
            }            if (!IsValidPhoneNumber(phoneNumber))
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
                }                // Setup the Lookup parameters
                var options = new Twilio.Rest.Lookups.V1.PhoneNumberOptions(phoneNumber);
                
                // Add carrier type to the lookup if using the paid service
                if (usePaidService)
                {
                    options.Type = new[] { "carrier" };
                }

                // Perform the lookup
                var lookupResult = await PhoneNumberResource.FetchAsync(options);
                  // Create the validated phone number record
                var validatedNumber = new ValidatedPhoneNumber
                {
                    PhoneNumber = phoneNumber,
                    IsValid = true,
                    ValidatedOn = DateTime.UtcNow,
                    CountryCode = lookupResult.CountryCode,
                    CreatedBy = "System"
                };

                // If carrier information is available (from paid service)
                if (lookupResult.Carrier != null)
                {
                    validatedNumber.Carrier = lookupResult.Carrier["name"]?.ToString();
                    validatedNumber.IsMobile = lookupResult.Carrier["type"]?.ToString()?.Equals("mobile", StringComparison.OrdinalIgnoreCase) ?? false;
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