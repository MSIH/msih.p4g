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
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.DonationService.Models;
using msih.p4g.Shared.Models;

namespace msih.p4g.Server.Features.DonationService.Interfaces
{
    /// <summary>
    /// Repository interface for Donation entity
    /// </summary>
    public interface IDonationRepository : IGenericRepository<Donation>
    {
        /// <summary>
        /// Gets paginated donations for a specific user by email.
        /// </summary>
        Task<PagedResult<Donation>> GetPagedByUserEmailAsync(string email, PaginationParameters parameters);

        /// <summary>
        /// Searches for paginated donations for a specific user by email.
        /// </summary>
        Task<PagedResult<Donation>> SearchPagedByUserEmailAsync(string email, PaginationParameters parameters);

        /// <summary>
        /// Gets paginated donations for a specific referral code.
        /// </summary>
        Task<PagedResult<Donation>> GetPagedByReferralCodeAsync(string referralCode, PaginationParameters parameters);

        /// <summary>
        /// Gets all donations with related entities included.
        /// </summary>
        Task<IEnumerable<Donation>> GetAllWithIncludesAsync();
    }
}
