using msih.p4g.Server.Features.Base.ProfileService.Interfaces;
using msih.p4g.Server.Features.Base.ProfileService.Model;
using System.Collections.Generic;
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
            return await _profileRepository.GetAllAsync(includeInactive: includeInactive, includeDeleted: false);
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

        public async Task<bool> DeleteAsync(int id, string modifiedBy = "System")
        {
            return await _profileRepository.DeleteAsync(id, modifiedBy);
        }

        public async Task<bool> SetActiveAsync(int id, bool isActive, string modifiedBy = "System")
        {
            return await _profileRepository.SetActiveStatusAsync(id, isActive, modifiedBy);
        }
    }
}
