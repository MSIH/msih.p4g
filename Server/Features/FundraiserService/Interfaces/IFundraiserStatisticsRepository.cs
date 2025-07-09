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
using msih.p4g.Server.Features.FundraiserService.Model;

namespace msih.p4g.Server.Features.FundraiserService.Interfaces
{
    /// <summary>
    /// Repository interface for fundraiser statistics
    /// </summary>
    public interface IFundraiserStatisticsRepository
    {
        /// <summary>
        /// Gets statistics for a fundraiser
        /// </summary>
        /// <param name="fundraiserId">ID of the fundraiser</param>
        /// <returns>Fundraiser statistics</returns>
        Task<FundraiserStatistics> GetStatisticsAsync(int fundraiserId);

        /// <summary>
        /// Gets the referral code for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The referral code, or null if not found</returns>
        Task<string?> GetReferralCodeAsync(int userId);

        /// <summary>
        /// Gets first-time donors who made their very first donation with this fundraiser
        /// </summary>
        /// <param name="fundraiserId">ID of the fundraiser</param>
        /// <returns>List of first-time donor information</returns>
        Task<List<FirstTimeDonorInfo>> GetFirstTimeDonorsAsync(int fundraiserId);

        Task<List<FirstTimeDonorInfo>> GetReferralDonorsAsync(int fundraiserId);
    }
}
