// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

using MSIH.Core.Common.Models;
using MSIH.Core.Services.Profiles.Interfaces;
using ProfileEntity = MSIH.Core.Services.Profiles.Model.Profile;

namespace MSIH.Core.Services.Profiles.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IProfileRepository _profileRepository;
        public ProfileService(IProfileRepository profileRepository)
        {
            _profileRepository = profileRepository;
        }

        public async Task<IEnumerable<ProfileEntity>> GetAllAsync(bool includeInactive = false)
        {
            return await _profileRepository.GetAllAsync(includeInactive: includeInactive);
        }

        public async Task<ProfileEntity> GetByIdAsync(int id)
        {
            return await _profileRepository.GetByIdAsync(id);
        }

        public async Task<ProfileEntity> AddAsync(ProfileEntity profile, string createdBy = "ProfileService", bool consentReceiveEmail = true)
        {
            // Generate a unique referral code before adding to the repository
            profile.GenerateReferralCode();
            profile.ConsentReceiveEmail = consentReceiveEmail; // Default to true for email consent
            return await _profileRepository.AddAsync(profile, createdBy);
        }

        public async Task<ProfileEntity> UpdateAsync(ProfileEntity profile, string modifiedBy = "ProfileService")
        {
            return await _profileRepository.UpdateAsync(profile, modifiedBy);
        }

        public async Task<bool> SetActiveAsync(int id, bool isActive, string modifiedBy = "ProfileService")
        {
            return await _profileRepository.SetActiveStatusAsync(id, isActive, modifiedBy);
        }

        /// <summary>
        /// Gets a profile by its referral code
        /// </summary>
        /// <param name="referralCode">The referral code to search for</param>
        /// <returns>The profile with the specified referral code, or null if not found</returns>
        public async Task<ProfileEntity> GetByReferralCodeAsync(string referralCode)
        {
            if (string.IsNullOrEmpty(referralCode))
                return null;

            // Use the FindAsync method from the repository to find profiles with the given referral code
            var profiles = await _profileRepository.FindAsync(p => p.ReferralCode == referralCode);

            // Since referral codes are unique, we should only have one result (or none)
            return profiles.FirstOrDefault();
        }

        /// <inheritdoc />
        public async Task<List<ProfileEntity>> GetAllWithUserDataAsync()
        {
            return await _profileRepository.GetAllWithUserDataAsync();
        }

        /// <inheritdoc />
        public async Task<PagedResult<ProfileEntity>> GetPaginatedWithUserDataAsync(PaginationParameters paginationParameters)
        {
            return await _profileRepository.GetPaginatedWithUserDataAsync(paginationParameters);
        }
    }
}
