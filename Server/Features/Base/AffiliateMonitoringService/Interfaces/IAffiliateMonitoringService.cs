// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

using msih.p4g.Server.Features.Base.ProfileService.Model;

namespace msih.p4g.Server.Features.Base.AffiliateMonitoringService.Interfaces
{
    /// <summary>
    /// Service for monitoring affiliate accounts and handling suspension logic
    /// </summary>
    public interface IAffiliateMonitoringService
    {
        /// <summary>
        /// Checks if an affiliate should be suspended based on the creation of a new donor
        /// </summary>
        /// <param name="referralCode">The affiliate's referral code</param>
        /// <returns>True if the affiliate was suspended, false if no action was taken</returns>
        Task<bool> CheckAffiliateAfterDonorCreationAsync(string referralCode);

        /// <summary>
        /// Counts unqualified donor accounts linked to an affiliate
        /// </summary>
        /// <param name="referralCode">The affiliate's referral code</param>
        /// <returns>Number of unqualified accounts</returns>
        Task<int> CountUnqualifiedAccountsAsync(string referralCode);

        /// <summary>
        /// Suspends an affiliate account
        /// </summary>
        /// <param name="profile">The affiliate profile to suspend</param>
        /// <param name="reason">Reason for suspension</param>
        /// <returns>True if successfully suspended</returns>
        Task<bool> SuspendAffiliateAsync(Profile profile, string reason);

        /// <summary>
        /// Sends suspension notification email to the affiliate
        /// </summary>
        /// <param name="profile">The suspended affiliate profile</param>
        /// <param name="reason">Reason for suspension</param>
        /// <returns>True if email was sent successfully</returns>
        Task<bool> SendSuspensionNotificationAsync(Profile profile, string reason);
    }
}