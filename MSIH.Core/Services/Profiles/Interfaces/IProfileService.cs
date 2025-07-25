// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

using MSIH.Core.Common.Models;
using MSIH.Core.Services.Profiles.Model;

namespace MSIH.Core.Services.Profiles.Interfaces
{
    public interface IProfileService
    {
        Task<IEnumerable<MSIH.Core.Services.Profiles.Model.Profile>> GetAllAsync(bool includeInactive = false);
        Task<MSIH.Core.Services.Profiles.Model.Profile> GetByIdAsync(int id);
        Task<MSIH.Core.Services.Profiles.Model.Profile> AddAsync(MSIH.Core.Services.Profiles.Model.Profile profile, string createdBy = "ProfileService", bool consentReceiveEmail = true);
        Task<MSIH.Core.Services.Profiles.Model.Profile> UpdateAsync(MSIH.Core.Services.Profiles.Model.Profile profile, string modifiedBy = "ProfileService");
        Task<bool> SetActiveAsync(int id, bool isActive, string modifiedBy = "ProfileService");

        /// <summary>
        /// Gets a profile by its referral code
        /// </summary>
        /// <param name="referralCode">The referral code to search for</param>
        /// <returns>The profile with the specified referral code, or null if not found</returns>
        Task<MSIH.Core.Services.Profiles.Model.Profile> GetByReferralCodeAsync(string referralCode);

        /// <summary>
        /// Gets all profiles with User navigation properties included
        /// </summary>
        /// <returns>A list of profiles with related data</returns>
        Task<List<MSIH.Core.Services.Profiles.Model.Profile>> GetAllWithUserDataAsync();

        /// <summary>
        /// Gets paginated profiles with User navigation properties included
        /// </summary>
        /// <param name="paginationParameters">Pagination and search parameters</param>
        /// <returns>A paginated result of profiles with related data</returns>
        Task<PagedResult<MSIH.Core.Services.Profiles.Model.Profile>> GetPaginatedWithUserDataAsync(PaginationParameters paginationParameters);
    }
}
