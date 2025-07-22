/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using System.Text.RegularExpressions;

namespace MSIH.Core.Common.Utilities
{
    /// <summary>
    /// Provides validation utilities for common data formats
    /// </summary>
    public static class ValidationUtilities
    {
        private static readonly Regex _emailRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex _phoneRegex = new Regex(
            @"^\+?[1-9]\d{1,14}$", // E.164 international phone number format
            RegexOptions.Compiled);

        /// <summary>
        /// Validates whether a string is in a valid email format
        /// </summary>
        /// <param name="email">The email string to validate</param>
        /// <returns>True if the email format is valid, false otherwise</returns>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return _emailRegex.IsMatch(email);
        }

        /// <summary>
        /// Validates whether a string is in a valid phone number format
        /// Uses E.164 international format validation (e.g., +12125551234)
        /// </summary>
        /// <param name="phoneNumber">The phone number to validate</param>
        /// <returns>True if the phone number format is valid, false otherwise</returns>
        public static bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            return _phoneRegex.IsMatch(phoneNumber);
        }
    }
}