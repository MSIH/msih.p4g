// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

using Microsoft.EntityFrameworkCore;
using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Features.Base.AffiliateMonitoringService.Interfaces;
using msih.p4g.Server.Features.Base.MessageService.Interfaces;
using MSIH.Core.Services.Profiles.Interfaces;
using MSIH.Core.Services.Profiles.Model;
using msih.p4g.Server.Features.Base.SettingsService.Interfaces;
using MSIH.Core.Services.Users.Interfaces;
using msih.p4g.Server.Features.FundraiserService.Interfaces;
using msih.p4g.Server.Features.FundraiserService.Model;

namespace msih.p4g.Server.Features.Base.AffiliateMonitoringService.Services
{
    /// <summary>
    /// Service for monitoring affiliate accounts and handling suspension logic
    /// </summary>
    public class AffiliateMonitoringService : IAffiliateMonitoringService
    {
        private readonly ApplicationDbContext _context;
        private readonly IProfileService _profileService;
        private readonly IFundraiserService _fundraiserService;
        private readonly IUserService _userService;
        private readonly IMessageService _messageService;
        private readonly ILogger<AffiliateMonitoringService> _logger;
        private readonly ISettingsService _settingService; // Add this field

        public AffiliateMonitoringService(
            ApplicationDbContext context,
            IProfileService profileService,
            IFundraiserService fundraiserService,
            IUserService userService,
            IMessageService messageService,
            ILogger<AffiliateMonitoringService> logger,
            ISettingsService settingService)
        {
            _context = context;
            _profileService = profileService;
            _fundraiserService = fundraiserService;
            _userService = userService;
            _messageService = messageService;
            _logger = logger;
        }

        /// <summary>
        /// Checks if an affiliate should be suspended based on the creation of a new donor
        /// </summary>
        public async Task<bool> CheckAffiliateAfterDonorCreationAsync(string referralCode)
        {
            if (string.IsNullOrEmpty(referralCode))
                return false;

            try
            {
                // get user with profile and fundraiser information
                var user = await _userService.GetByReferralCodeAsync(referralCode, includeProfile: true, includeFundraiser: true);

                if (user == null)
                    return false;

                if (user.Fundraiser == null || user.Fundraiser.IsSuspended)
                    return false;

                string? suspensionReason = null;

                // Count unqualified accounts linked to this affiliate
                var unqualifiedCountFirst = await CountUnqualifiedDonorsBeforeFirstDonationAsync(referralCode);

                if (unqualifiedCountFirst == 2)
                {
                    // Suspend if first 2 accounts are unqualified
                    suspensionReason = "First two accounts associated with affiliate are unqualified.";
                }

                // Count unqualified accounts linked to this affiliate
                var unqualifiedCount = await CountUnqualifiedAccountsAsync(referralCode);

                // Check suspension criteria
                if (unqualifiedCount > 9)
                {
                    // Suspend if more than 9 unqualified accounts
                    suspensionReason = $"More than nine unqualified accounts ({unqualifiedCount}) are associated with affiliate.";
                }

                if (!string.IsNullOrEmpty(suspensionReason))
                {
                    _logger.LogInformation("Suspending affiliate {ReferralCode} - Reason: {Reason}",
                        referralCode, suspensionReason);

                    // Suspend the affiliate
                    await SuspendAffiliateAsync(user.Fundraiser, suspensionReason);

                    // Send notification email
                    await SendSuspensionNotificationAsync(user.Profile, user.Fundraiser, suspensionReason);

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking affiliate suspension for referral code {ReferralCode}", referralCode);
                return false;
            }
        }

        /// <summary>
        /// Counts unqualified donor accounts linked to an affiliate
        /// </summary>
        public async Task<int> CountUnqualifiedAccountsAsync(string referralCode)
        {
            if (string.IsNullOrEmpty(referralCode))
                return 0;

            // Count donors with this referral code who haven't made any donations
            var unqualifiedCount = await _context.Donors
                .Where(d => d.ReferralCode == referralCode && d.IsActive)
                .Where(d => !d.Donations.Any()) // No donations means unqualified
                .CountAsync();

            return unqualifiedCount;
        }


        /// <summary>
        /// Counts the number of donor accounts (with no donations) created before the first donor with a donation for a given referral code.
        /// This helps identify if a referral code is being used to create accounts that do not donate.
        /// </summary>
        public async Task<int> CountUnqualifiedDonorsBeforeFirstDonationAsync(string referralCode)
        {
            if (string.IsNullOrEmpty(referralCode))
                return 0;

            // Get all donors with this referral code, ordered by creation date (assuming Id is incremental)
            var donors = await _context.Donors
                .Where(d => d.ReferralCode == referralCode && d.IsActive)
                .OrderBy(d => d.Id)
                .ToListAsync();

            int count = 0;
            foreach (var donor in donors)
            {
                if (donor.Donations.Any())
                    break; // Stop at the first donor who made a donation
                count++;
            }

            return count;
        }

        /// <summary>
        /// Suspends an affiliate account
        /// </summary>
        public async Task<bool> SuspendAffiliateAsync(Fundraiser fundraiser, string reason)
        {
            try
            {
                fundraiser.IsSuspended = true;
                fundraiser.SuspensionReason = reason;
                fundraiser.SuspendedDate = DateTime.UtcNow;

                await _fundraiserService.UpdateAsync(fundraiser);

                _logger.LogInformation("Affiliate fundraiser {FundraiserId} suspended successfully", fundraiser.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error suspending affiliate fundraiser {FundraiserId}", fundraiser.Id);
                return false;
            }
        }

        /// <summary>
        /// Sends suspension notification email to the affiliate
        /// </summary>
        public async Task<bool> SendSuspensionNotificationAsync(Profile profile, Fundraiser fundraiser, string reason)
        {
            try
            {
                if (profile.User?.Email == null)
                {
                    _logger.LogWarning("Cannot send suspension notification - no email found for affiliate {ReferralCode}",
                        profile.ReferralCode);
                    return false;
                }

                var subject = "Affiliate Account Suspended - Make Sure It Happens";
                var htmlContent = $@"
                    <html>
                    <body>
                        <h2>Affiliate Account Suspension Notice</h2>
                        <p>Dear {profile.FullName},</p>

                        <p>We are writing to inform you that your affiliate account has been suspended due to the following reason:</p>

                        <p><strong>Reason:</strong> {reason}</p>

                        <p>Your affiliate referral code: <strong>{profile.ReferralCode}</strong></p>
                        <p>Suspension date: <strong>{fundraiser.SuspendedDate:yyyy-MM-dd HH:mm:ss} UTC</strong></p>

                        <p>If you believe this suspension was made in error or if you have any questions,
                        please contact our support team for assistance.</p>

                        <p>Thank you for your understanding.</p>

                        <p>Best regards,<br>
                        Make Sure It Happens Team</p>
                    </body>
                    </html>";



                await _messageService.SendEmailAsync(
                    to: profile.User.Email,
                    subject: subject,
                    htmlContent: htmlContent
                );

                _logger.LogInformation("Suspension notification sent to affiliate {ReferralCode} at {Email}",
                    profile.ReferralCode, profile.User.Email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending suspension notification to affiliate {ReferralCode}",
                    profile.ReferralCode);
                return false;
            }
        }
    }
}
