/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using System.Text.RegularExpressions;

namespace msih.p4g.Server.Features.Base.W9FormService.Utilities
{
    /// <summary>
    /// Utility methods for handling SSN and EIN information
    /// </summary>
    public static class SsnUtility
    {
        /// <summary>
        /// Validates an SSN format
        /// </summary>
        public static bool IsValidSsn(string ssn)
        {
            if (string.IsNullOrWhiteSpace(ssn))
                return false;

            // Remove any non-digit characters 
            string digitsOnly = Regex.Replace(ssn, @"\D", "");

            // SSN must be 9 digits
            if (digitsOnly.Length != 9)
                return false;

            // Additional validation could be added here (e.g., not all zeros)
            return true;
        }

        /// <summary>
        /// Validates an EIN format
        /// </summary>
        public static bool IsValidEin(string ein)
        {
            if (string.IsNullOrWhiteSpace(ein))
                return false;

            // Remove any non-digit characters
            string digitsOnly = Regex.Replace(ein, @"\D", "");

            // EIN must be 9 digits
            if (digitsOnly.Length != 9)
                return false;

            return true;
        }

        /// <summary>
        /// Formats an SSN to standard format (XXX-XX-XXXX)
        /// </summary>
        public static string FormatSsn(string ssn)
        {
            if (string.IsNullOrWhiteSpace(ssn))
                return string.Empty;

            // Remove any non-digit characters
            string digitsOnly = Regex.Replace(ssn, @"\D", "");

            // If we have 9 digits, format as XXX-XX-XXXX
            if (digitsOnly.Length == 9)
            {
                return $"{digitsOnly.Substring(0, 3)}-{digitsOnly.Substring(3, 2)}-{digitsOnly.Substring(5, 4)}";
            }

            // Return original if not valid
            return ssn;
        }

        /// <summary>
        /// Masks an SSN, showing only the last 4 digits
        /// </summary>
        public static string MaskSsn(string ssn)
        {
            if (string.IsNullOrWhiteSpace(ssn))
                return string.Empty;

            // Remove any non-digit characters
            string digitsOnly = Regex.Replace(ssn, @"\D", "");

            // If we have 9 digits, mask the first 5
            if (digitsOnly.Length == 9)
            {
                return $"XXX-XX-{digitsOnly.Substring(5, 4)}";
            }

            // Return masked version if not exactly 9 digits
            return "XXX-XX-XXXX";
        }

        /// <summary>
        /// Masks an EIN, showing only the last 4 digits
        /// </summary>
        public static string MaskEin(string ein)
        {
            if (string.IsNullOrWhiteSpace(ein))
                return string.Empty;

            // Remove any non-digit characters
            string digitsOnly = Regex.Replace(ein, @"\D", "");

            // If we have 9 digits, mask the first 5
            if (digitsOnly.Length == 9)
            {
                return $"XX-XXXXX{digitsOnly.Substring(5, 4)}";
            }

            // Return masked version if not exactly 9 digits
            return "XX-XXXXXXX";
        }
    }
}
