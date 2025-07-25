/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using MSIH.Core.Services.Sms.Models;
using System.Threading.Tasks;

namespace MSIH.Core.Services.Sms.Interfaces
{
    public interface ISmsService
    {
        /// <summary>
        /// Sends an SMS message to the specified phone number.
        /// </summary>
        /// <param name="to">The recipient's phone number in E.164 format (e.g., +12125551234).</param>
        /// <param name="message">The message content to send.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendSmsAsync(string to, string message);

        /// <summary>
        /// Validates if the provided phone number is in a valid format.
        /// </summary>
        /// <param name="phoneNumber">The phone number to validate.</param>
        /// <returns>True if the phone number is valid, false otherwise.</returns>
        bool IsValidPhoneNumber(string phoneNumber);

        /// <summary>
        /// Validates a phone number using Twilio's Lookup service to determine if it is a valid mobile number.
        /// Results are stored in the database for future reference.
        /// </summary>
        /// <param name="phoneNumber">The phone number to validate in E.164 format.</param>
        /// <param name="useCache">Whether to use cached validation results if available.</param>
        /// <param name="usePaidService">Whether to use Twilio's paid carrier lookup service for advanced validation.</param>
        /// <returns>A validated phone number object with validation results.</returns>
        Task<ValidatedPhoneNumber> ValidatePhoneNumberAsync(string phoneNumber, bool useCache = true, bool usePaidService = false);
    }
}
