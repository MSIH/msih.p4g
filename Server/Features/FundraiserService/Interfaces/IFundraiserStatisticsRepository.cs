/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using System.Threading.Tasks;
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
    }
}
