using msih.p4g.Server.Features.Base.ProfileService.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace msih.p4g.Server.Features.Base.ProfileService.Interfaces
{
    public interface IProfileService
    {
        Task<IEnumerable<Profile>> GetAllAsync(bool includeInactive = false);
        Task<Profile> GetByIdAsync(int id);
        Task<Profile> AddAsync(Profile profile, string createdBy = "System");
        Task<Profile> UpdateAsync(Profile profile, string modifiedBy = "System");
        Task<bool> SetActiveAsync(int id, bool isActive, string modifiedBy = "System");

        /// <summary>
        /// Gets a profile by its referral code
        /// </summary>
        /// <param name="referralCode">The referral code to search for</param>
        /// <returns>The profile with the specified referral code, or null if not found</returns>
        Task<Profile> GetByReferralCodeAsync(string referralCode);
    }
}
