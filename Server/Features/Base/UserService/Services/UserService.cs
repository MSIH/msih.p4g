// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

using msih.p4g.Server.Features.Base.UserService.Interfaces;
using msih.p4g.Server.Features.Base.UserService.Models;

namespace msih.p4g.Server.Features.Base.UserService.Services
{
    /// <summary>
    /// Service for managing user operations
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <inheritdoc />
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _userRepository.GetByEmailAsync(email);
        }

        /// <summary>
        /// Gets a user by their email with optional related entities
        /// </summary>
        /// <param name="email">The email of the user to retrieve</param>
        /// <param name="includeProfile">Whether to include the user's profile</param>
        /// <param name="includeDonor">Whether to include the user's donor record</param>
        /// <param name="includeFundraiser">Whether to include the user's fundraiser record</param>
        /// <param name="includeAddress">Whether to include the user's profile address information</param>
        /// <returns>The user if found, otherwise null</returns>
        public async Task<User?> GetByEmailAsync(string email, bool includeProfile = false, bool includeAddress = false, bool includeDonor = false, bool includeFundraiser = false)
        {
            return await _userRepository.GetByEmailAsync(email, includeProfile, includeAddress, includeDonor, includeFundraiser);
        }

        /// <summary>
        /// Gets a user by their profile referral code with optional related entities
        /// </summary>
        /// <param name="referralCode">The referral code of the user to retrieve</param>
        /// <param name="includeProfile">Whether to include the user's profile</param>
        /// <param name="includeDonor">Whether to include the user's donor record</param>
        /// <param name="includeFundraiser">Whether to include the user's fundraiser record</param>
        /// <param name="includeAddress">Whether to include the user's profile address information</param>
        /// <returns>The user if found, otherwise null</returns>
        public async Task<User?> GetByReferralCodeAsync(string referralCode, bool includeProfile = false, bool includeAddress = false, bool includeDonor = false, bool includeFundraiser = false)
        {
            return await _userRepository.GetByReferralCodeAsync(referralCode, includeProfile, includeAddress, includeDonor, includeFundraiser);
        }

        /// <summary>
        /// Gets a user by their ID
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve</param>
        /// <returns>The user if found, otherwise null</returns>
        public async Task<User?> GetByIdAsync(int userId)
        {
            return await _userRepository.GetByIdAsync(userId);
        }

        /// <inheritdoc />
        public async Task<User> AddAsync(User user, string createdBy = "UserService")
        {
            return await _userRepository.AddAsync(user, createdBy);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<User>> GetAllAsync(bool includeInactive = false)
        {
            return await _userRepository.GetAllAsync(includeInactive: includeInactive);
        }

        /// <inheritdoc />
        public async Task UpdateAsync(User user, string modifiedBy = "UserService")
        {
            await _userRepository.UpdateAsync(user, modifiedBy);
        }

        /// <inheritdoc />
        public async Task SetActiveAsync(int userId, bool isActive, string modifiedBy = "UserService")
        {
            await _userRepository.SetActiveStatusAsync(userId, isActive, modifiedBy);
        }

        // look up user by emailtoken
        public async Task<User?> GetUserByTokenAsync(string emailToken)
        {
            if (string.IsNullOrWhiteSpace(emailToken))
                return null;
            return await _userRepository.GetUserByTokenAsync(emailToken);
        }

        public async Task<bool> LogOutUserByIdAsync(int userId)
        {
            if (userId <= 0)
                return false;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return false;

            user.EmailConfirmed = false;
            await _userRepository.UpdateAsync(user, "UserService");
            return true;
        }
    }
}
