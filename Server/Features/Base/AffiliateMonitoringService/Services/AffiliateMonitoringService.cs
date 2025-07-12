// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

using Microsoft.EntityFrameworkCore;
using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Features.Base.AffiliateMonitoringService.Interfaces;
using msih.p4g.Server.Features.Base.EmailService.Interfaces;
using msih.p4g.Server.Features.Base.ProfileService.Interfaces;
using msih.p4g.Server.Features.Base.ProfileService.Model;
using msih.p4g.Server.Features.Base.UserService.Interfaces;

namespace msih.p4g.Server.Features.Base.AffiliateMonitoringService.Services
{
    /// <summary>
    /// Service for monitoring affiliate accounts and handling suspension logic
    /// </summary>
    public class AffiliateMonitoringService : IAffiliateMonitoringService
    {
        private readonly ApplicationDbContext _context;
        private readonly IProfileService _profileService;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AffiliateMonitoringService> _logger;

        public AffiliateMonitoringService(
            ApplicationDbContext context,
            IProfileService profileService,
            IUserService userService,
            IEmailService emailService,
            ILogger<AffiliateMonitoringService> logger)
        {
            _context = context;
            _profileService = profileService;
            _userService = userService;
            _emailService = emailService;
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
                // Get the affiliate profile
                var affiliate = await GetAffiliateByReferralCodeAsync(referralCode);
                if (affiliate == null || affiliate.IsSuspended)
                    return false;

                // Count unqualified accounts linked to this affiliate
                var unqualifiedCount = await CountUnqualifiedAccountsAsync(referralCode);

                string? suspensionReason = null;

                // Check suspension criteria
                if (unqualifiedCount == 2)
                {
                    // Suspend if first 2 accounts are unqualified
                    suspensionReason = "First two accounts associated with affiliate are unqualified (have not donated).";
                }
                else if (unqualifiedCount > 9)
                {
                    // Suspend if more than 9 unqualified accounts
                    suspensionReason = $"More than nine unqualified accounts ({unqualifiedCount}) are associated with affiliate.";
                }

                if (!string.IsNullOrEmpty(suspensionReason))
                {
                    _logger.LogInformation("Suspending affiliate {ReferralCode} - Reason: {Reason}", 
                        referralCode, suspensionReason);

                    // Suspend the affiliate
                    await SuspendAffiliateAsync(affiliate, suspensionReason);

                    // Send notification email
                    await SendSuspensionNotificationAsync(affiliate, suspensionReason);

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
        /// Gets the affiliate profile by referral code
        /// </summary>
        public async Task<Profile?> GetAffiliateByReferralCodeAsync(string referralCode)
        {
            if (string.IsNullOrEmpty(referralCode))
                return null;

            return await _context.Profiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.ReferralCode == referralCode && p.IsActive);
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
        /// Suspends an affiliate account
        /// </summary>
        public async Task<bool> SuspendAffiliateAsync(Profile profile, string reason)
        {
            try
            {
                profile.IsSuspended = true;
                profile.SuspensionReason = reason;
                profile.SuspendedDate = DateTime.UtcNow;

                await _profileService.UpdateAsync(profile, "AffiliateMonitoringService");

                _logger.LogInformation("Affiliate {ReferralCode} suspended successfully", profile.ReferralCode);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error suspending affiliate {ReferralCode}", profile.ReferralCode);
                return false;
            }
        }

        /// <summary>
        /// Sends suspension notification email to the affiliate
        /// </summary>
        public async Task<bool> SendSuspensionNotificationAsync(Profile profile, string reason)
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
                        <p>Suspension date: <strong>{profile.SuspendedDate:yyyy-MM-dd HH:mm:ss} UTC</strong></p>
                        
                        <p>If you believe this suspension was made in error or if you have any questions, 
                        please contact our support team for assistance.</p>
                        
                        <p>Thank you for your understanding.</p>
                        
                        <p>Best regards,<br>
                        Make Sure It Happens Team</p>
                    </body>
                    </html>";

                await _emailService.SendEmailAsync(
                    to: profile.User.Email,
                    from: "noreply@makesureithappens.org", // This should be configurable
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