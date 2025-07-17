// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

using msih.p4g.Server.Features.Base.ProfileService.Model;
using msih.p4g.Server.Features.Base.SettingsService.Interfaces;

namespace msih.p4g.Server.Common.Utilities
{
    public class ReferralLinkHelper
    {
        private readonly ISettingsService _settingsService;

        public ReferralLinkHelper(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }
        /// <summary>
        /// Computes the username for referral links using only the first name.
        /// Format: "{FirstName}" (e.g., "John")
        /// </summary>
        /// <param name="profile">The user profile containing the first name</param>
        /// <returns>Formatted username with first letter capitalized and rest lowercase</returns>
        public string ComputeUsername(Profile? profile)
        {
            if (profile?.FirstName == null)
                return string.Empty;

            var firstName = profile.FirstName.Trim();
            if (string.IsNullOrEmpty(firstName))
                return string.Empty;

            return char.ToUpper(firstName[0]) + firstName[1..].ToLower();
        }

        /// <summary>
        /// Generates a complete referral link with optional username appended.
        /// </summary>
        /// <param name="profile">The user profile containing referral code and name</param>
        /// <param name="donationUrl">The base donation URL (if null, will get from settings)</param>
        /// <param name="referralCode">Override referral code (if null, gets from profile)</param>
        /// <param name="appendName">Whether to append the username to the link</param>
        /// <returns>Complete referral link</returns>
        public async Task<string> GenerateReferralLinkAsync(Profile? profile, string? donationUrl = null, string? referralCode = null, bool appendName = true)
        {
            // Get referral code from profile if not provided
            referralCode ??= profile?.ReferralCode;

            if (string.IsNullOrEmpty(referralCode))
                return string.Empty;

            // Get donation URL from settings if not provided
            if (string.IsNullOrEmpty(donationUrl))
            {
                var baseUrl = await _settingsService.GetValueAsync("BaseUrl") ?? "https://msih.org";
                donationUrl = await _settingsService.GetValueAsync("donationUrl") ?? $"{baseUrl.TrimEnd('/')}/donate";
            }

            if (string.IsNullOrEmpty(donationUrl))
                return string.Empty;

            var finalUrl = donationUrl.TrimEnd('/');

            if (appendName && profile != null)
            {
                var username = ComputeUsername(profile);
                if (!string.IsNullOrEmpty(username))
                {
                    return $"{finalUrl}/{referralCode}-{username.Replace(" ", "")}";
                }
            }

            return $"{finalUrl}/{referralCode}";
        }

        /// <summary>
        /// Generates a complete referral link with optional username appended (synchronous version).
        /// </summary>
        /// <param name="donationUrl">The base donation URL (required for synchronous version)</param>
        /// <param name="referralCode">The user's referral code</param>
        /// <param name="profile">The user profile (optional)</param>
        /// <param name="appendName">Whether to append the username to the link</param>
        /// <returns>Complete referral link</returns>
        public string GenerateReferralLink(string donationUrl, string referralCode, Profile? profile = null, bool appendName = false)
        {
            if (string.IsNullOrEmpty(donationUrl) || string.IsNullOrEmpty(referralCode))
                return string.Empty;

            var baseUrl = donationUrl.TrimEnd('/');

            if (appendName && profile != null)
            {
                var username = ComputeUsername(profile);
                if (!string.IsNullOrEmpty(username))
                {
                    return $"{baseUrl}/{referralCode}-{username.Replace(" ", "")}";
                }
            }

            return $"{baseUrl}/{referralCode}";
        }
    }
}
