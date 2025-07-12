// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

namespace msih.p4g.Server.Features.AffiliateService.Interfaces
{
    /// <summary>
    /// Interface for affiliate monitoring operations
    /// </summary>
    public interface IAffiliateMonitoringService
    {
        /// <summary>
        /// Checks affiliate status after a donor is created
        /// </summary>
        /// <param name="donorId">The ID of the newly created donor</param>
        /// <param name="affiliateId">The ID of the affiliate</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task CheckAffiliateAfterDonorCreationAsync(int donorId, int affiliateId);
    }
}