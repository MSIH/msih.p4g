using msih.p4g.Server.Features.Base.ProfileService.Interfaces;
using msih.p4g.Server.Features.Base.ProfileService.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace msih.p4g.Server.Features.Base.ProfileService.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IProfileRepository _profileRepository;
        public ProfileService(IProfileRepository profileRepository)
        {
            _profileRepository = profileRepository;
        }

        public async Task<IEnumerable<Profile>> GetAllAsync(bool includeInactive = false)
        {
            return await _profileRepository.GetAllAsync(includeInactive: includeInactive);
        }

        public async Task<Profile> GetByIdAsync(int id)
        {
            return await _profileRepository.GetByIdAsync(id);
        }

        public async Task<Profile> AddAsync(Profile profile, string createdBy = "System")
        {
            // Generate a unique referral code before adding to the repository
            profile.GenerateReferralCode();
            return await _profileRepository.AddAsync(profile, createdBy);
        }

        public async Task<Profile> UpdateAsync(Profile profile, string modifiedBy = "System")
        {
            return await _profileRepository.UpdateAsync(profile, modifiedBy);
        }

        public async Task<bool> SetActiveAsync(int id, bool isActive, string modifiedBy = "System")
        {
            return await _profileRepository.SetActiveStatusAsync(id, isActive, modifiedBy);
        }

        /// <summary>
        /// Gets a profile by its referral code
        /// </summary>
        /// <param name="referralCode">The referral code to search for</param>
        /// <returns>The profile with the specified referral code, or null if not found</returns>
        public async Task<Profile> GetByReferralCodeAsync(string referralCode)
        {
            if (string.IsNullOrEmpty(referralCode))
                return null;

            // Use the FindAsync method from the repository to find profiles with the given referral code
            var profiles = await _profileRepository.FindAsync(p => p.ReferralCode == referralCode);

            // Since referral codes are unique, we should only have one result (or none)
            return profiles.FirstOrDefault();
        }
    }
}
