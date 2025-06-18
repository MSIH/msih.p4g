// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

using msih.p4g.Server.Features.Base.ProfileService.Interfaces;
using msih.p4g.Server.Features.Base.ProfileService.Model;
using msih.p4g.Server.Features.Base.UserProfileService.Interfaces;
using msih.p4g.Server.Features.Base.UserService.Interfaces;
using msih.p4g.Server.Features.Base.UserService.Models;
using msih.p4g.Server.Features.FundraiserService.Interfaces;
using msih.p4g.Server.Features.FundraiserService.Model;

namespace msih.p4g.Server.Features.Base.UserProfileService.Services
{
    /// <summary>
    /// Service that coordinates operations between User and Profile entities
    /// </summary>
    public class UserProfileService : IUserProfileService
    {
        private readonly IUserService _userService;
        private readonly IProfileService _profileService;
        private readonly IFundraiserService _fundraiserService;

        public UserProfileService(
            IUserService userService,
            IProfileService profileService,
            IFundraiserService fundraiserService
            )
        {
            _userService = userService;
            _profileService = profileService;
            _fundraiserService = fundraiserService;
        }

        /// <summary>
        /// Creates a new user and associated profile in a single coordinated operation
        /// </summary>
        /// <param name="user">The user to create</param>
        /// <param name="profile">The profile to create and associate with the user</param>
        /// <param name="createdBy">Who created these records</param>
        /// <returns>The created profile with generated referral code</returns>
        public async Task<Profile> CreateUserWithProfileAsync(User user, Profile profile, string createdBy = "System")
        {
            // Step 1: Create the user first to get the UserId
            var createdUser = await _userService.AddAsync(user, createdBy);

            // Step 2: Set the UserId on the profile
            profile.UserId = createdUser.Id;

            // Step 3: Add the profile (ProfileService.AddAsync will call GenerateReferralCode)
            var createdProfile = await _profileService.AddAsync(profile, createdBy);

            // Step 4: If the user role is Fundraiser, create a Fundraiser entity
            if (createdUser.Role == UserRole.Fundraiser)
            {
                var fundraiser = new Fundraiser
                {
                    UserId = createdUser.Id,
                    // Set any default properties for a new Fundraiser
                    IsActive = true
                };

                await _fundraiserService.AddAsync(fundraiser);
            }

            return createdProfile;
        }

        /// <summary>
        /// Gets a user's profile by the user's email address
        /// </summary>
        /// <param name="email">The email address of the user</param>
        /// <returns>The user's profile or null if not found</returns>
        public async Task<Profile?> GetProfileByUserEmailAsync(string email)
        {
            // Find the user by email
            var user = await _userService.GetByEmailAsync(email);
            if (user == null)
                return null;

            // Get the user's profile
            return await _profileService.GetByIdAsync(user.Id);
        }

        /// <summary>
        /// Updates an existing profile
        /// </summary>
        /// <param name="profile">The profile with updated information</param>
        /// <param name="modifiedBy">Who modified the profile</param>
        /// <returns>The updated profile</returns>
        public async Task<Profile> UpdateAsync(Profile profile, string modifiedBy = "System")
        {
            // Update the profile using the profile service
            return await _profileService.UpdateAsync(profile, modifiedBy);
        }
    }
}
