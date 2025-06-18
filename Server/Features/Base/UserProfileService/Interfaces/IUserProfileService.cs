using System.Threading.Tasks;
using msih.p4g.Server.Features.Base.UserService.Models;
using msih.p4g.Server.Features.Base.ProfileService.Model;

namespace msih.p4g.Server.Features.Base.UserProfileService.Interfaces
{
    /// <summary>
    /// Service interface that coordinates operations between User and Profile entities
    /// </summary>
    public interface IUserProfileService
    {
        /// <summary>
        /// Creates a new user and associated profile in a single coordinated operation
        /// </summary>
        /// <param name="user">The user to create</param>
        /// <param name="profile">The profile to create and associate with the user</param>
        /// <param name="createdBy">Who created these records</param>
        /// <returns>The created profile with generated referral code</returns>
        Task<Profile> CreateUserWithProfileAsync(User user, Profile profile, string createdBy = "System");
        
        /// <summary>
        /// Gets a user's profile by the user's email address
        /// </summary>
        /// <param name="email">The email address of the user</param>
        /// <returns>The user's profile or null if not found</returns>
        Task<Profile?> GetProfileByUserEmailAsync(string email);

        /// <summary>
        /// Updates an existing profile
        /// </summary>
        /// <param name="profile">The profile with updated information</param>
        /// <param name="modifiedBy">Who modified the profile</param>
        /// <returns>The updated profile</returns>
        Task<Profile> UpdateAsync(Profile profile, string modifiedBy = "System");
    }
}
