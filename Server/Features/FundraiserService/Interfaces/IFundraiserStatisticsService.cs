// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

using msih.p4g.Server.Features.FundraiserService.Model;

namespace msih.p4g.Server.Features.FundraiserService.Interfaces
{
    /// <summary>
    /// Interface for the fundraiser statistics service
    /// </summary>
    public interface IFundraiserStatisticsService
    {
        /// <summary>
        /// Gets statistics for a fundraiser
        /// </summary>
        /// <param name="fundraiserId">ID of the fundraiser</param>
        /// <returns>Fundraiser statistics</returns>
        Task<FundraiserStatistics> GetStatisticsAsync(int fundraiserId);

        /// <summary>
        /// Gets first-time donors who made their very first donation with this fundraiser
        /// </summary>
        /// <param name="fundraiserId">ID of the fundraiser</param>
        /// <returns>List of first-time donor information</returns>
        Task<List<FirstTimeDonorInfo>> GetFirstTimeDonorsAsync(int fundraiserId);

        Task<List<FirstTimeDonorInfo>> GetDeferralDonorsAsync(int fundraiserId);


    }
}
