// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

using Microsoft.Extensions.Configuration;
using msih.p4g.Server.Features.Base.SettingsService.Interfaces;
using msih.p4g.Server.Features.Base.UserProfileService.Interfaces;

namespace msih.p4g.Server.Common.Utilities
{
    /// <summary>
    /// Utility class for generating referral URLs for donations
    /// </summary>
    /// <example>
    /// Usage example:
    /// <code>
    /// // Inject the ReferalURLGenerator in your service or controller
    /// private readonly ReferalURLGenerator _referralGenerator;
    /// 
    /// // Generate a donation URL with referral code
    /// var donationUrl = await _referralGenerator.GenerateDonationUrlAsync("user@example.com");
    /// // Result: "https://localhost:63581/donate/ABC123" (if user has profile with referral code)
    /// // Result: "https://localhost:63581/donate" (if user has no profile or referral code)
    /// </code>
    /// </example>
    public class ReferalURLGenerator
    {
        private readonly ISettingsService _settingsService;
        private readonly IConfiguration _configuration;
        private readonly IUserProfileService _userProfileService;

        public ReferalURLGenerator(
            ISettingsService settingsService,
            IConfiguration configuration,
            IUserProfileService userProfileService)
        {
            _settingsService = settingsService;
            _configuration = configuration;
            _userProfileService = userProfileService;
        }

        /// <summary>
        /// Generates a donation URL with referral code for the given email address
        /// </summary>
        /// <param name="email">The email address of the user to generate referral URL for</param>
        /// <returns>Full donation URL with referral code, or base donation URL if user not found</returns>
        public async Task<string> GenerateDonationUrlAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return await GetBaseDonationUrlAsync();
            }

            try
            {
                // Get the user's profile by email
                var profile = await _userProfileService.GetProfileByUserEmailAsync(email);
                
                // Get the base donation URL
                var baseDonationUrl = await GetBaseDonationUrlAsync();
                
                // If profile found and has referral code, append it to the URL
                if (profile != null && !string.IsNullOrEmpty(profile.ReferralCode))
                {
                    return $"{baseDonationUrl}/{profile.ReferralCode}";
                }
                
                // Return base URL if no profile or referral code found
                return baseDonationUrl;
            }
            catch
            {
                // Return base URL on any error
                return await GetBaseDonationUrlAsync();
            }
        }

        /// <summary>
        /// Gets the base donation URL from settings or configuration
        /// Priority order: 1) DonationURL from database settings, 2) DonationURL from configuration, 
        /// 3) BaseURL from settings + "/donate", 4) BaseUrl from configuration + "/donate", 5) Default fallback
        /// </summary>
        /// <returns>The base donation URL</returns>
        public async Task<string> GetBaseDonationUrlAsync()
        {
            // Try to get donation URL from settings first
            var donationUrl = await _settingsService.GetValueAsync("DonationURL");
            
            // If not found in settings, check configuration
            if (string.IsNullOrEmpty(donationUrl))
            {
                donationUrl = _configuration["DonationURL"];
            }
            
            // If still not found, try to construct from BaseURL setting
            if (string.IsNullOrEmpty(donationUrl))
            {
                var baseUrl = await _settingsService.GetValueAsync("BaseURL") 
                              ?? _configuration["BaseUrl"];
                              
                if (!string.IsNullOrEmpty(baseUrl))
                {
                    // Remove trailing slash if present
                    baseUrl = baseUrl.TrimEnd('/');
                    donationUrl = $"{baseUrl}/donate";
                }
            }
            
            // Final fallback to default URL
            return donationUrl ?? "https://msih.org/donate";
        }
    }
}
