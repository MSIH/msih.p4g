// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

using msih.p4g.Server.Features.Base.ProfileService.Model;
using msih.p4g.Shared.Models;

namespace msih.p4g.Server.Features.Base.ProfileService.Interfaces
{
    public interface IProfileService
    {
        Task<IEnumerable<Profile>> GetAllAsync(bool includeInactive = false);
        Task<Profile> GetByIdAsync(int id);
        Task<Profile> AddAsync(Profile profile, string createdBy = "ProfileService", bool consentReceiveEmail = true);
        Task<Profile> UpdateAsync(Profile profile, string modifiedBy = "ProfileService");
        Task<bool> SetActiveAsync(int id, bool isActive, string modifiedBy = "ProfileService");

        /// <summary>
        /// Gets a profile by its referral code
        /// </summary>
        /// <param name="referralCode">The referral code to search for</param>
        /// <returns>The profile with the specified referral code, or null if not found</returns>
        Task<Profile> GetByReferralCodeAsync(string referralCode);

        /// <summary>
        /// Gets all profiles with User navigation properties included
        /// </summary>
        /// <returns>A list of profiles with related data</returns>
        Task<List<Profile>> GetAllWithUserDataAsync();

        /// <summary>
        /// Gets paginated profiles with User navigation properties included
        /// </summary>
        /// <param name="paginationParameters">Pagination and search parameters</param>
        /// <returns>A paginated result of profiles with related data</returns>
        Task<PagedResult<Profile>> GetPaginatedWithUserDataAsync(PaginationParameters paginationParameters);
    }
}
