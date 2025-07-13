// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

using msih.p4g.Server.Features.Base.UserService.Models;

namespace msih.p4g.Server.Features.Base.UserService.Interfaces
{
    /// <summary>
    /// Interface for managing user operations
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Retrieves a user by their email address
        /// </summary>
        /// <param name="email">The email address to search for</param>
        /// <returns>The found user or null if not found</returns>
        Task<User?> GetByEmailAsync(string email);

        Task<User?> GetByEmailAsync(
            string email,
            bool includeProfile = false,
            bool includeAddress = false,
            bool includeDonor = false,
            bool includeFundraiser = false);

        /// <summary>
        /// Retrieves a user by their ID
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve</param>
        /// <returns>The found user or null if not found</returns>
        Task<User?> GetByIdAsync(int userId);

        /// <summary>
        /// Creates a new user in the system
        /// </summary>
        /// <param name="user">The user to create</param>
        /// <param name="createdBy">Who created the user</param>
        /// <returns>The created user with Id assigned</returns>

        /// <summary>
        /// Gets a user by their profile referral code with optional related entities
        /// </summary>
        /// <param name="referralCode">The referral code of the user to retrieve</param>
        /// <param name="includeProfile">Whether to include the user's profile</param>
        /// <param name="includeAddress">Whether to include the user's profile address information</param>
        /// <param name="includeDonor">Whether to include the user's donor record</param>
        /// <param name="includeFundraiser">Whether to include the user's fundraiser record</param>
        /// <returns>The user if found, otherwise null</returns>
        Task<User?> GetByReferralCodeAsync(string referralCode, bool includeProfile = false, bool includeAddress = false, bool includeDonor = false, bool includeFundraiser = false);

        Task<User> AddAsync(User user, string createdBy = "UserService");


        /// <summary>
        /// Gets all active users in the system
        /// </summary>
        /// <param name="includeInactive">Whether to include inactive users</param>
        /// <returns>A collection of users</returns>
        Task<IEnumerable<User>> GetAllAsync(bool includeInactive = false);

        /// <summary>
        /// Updates an existing user
        /// </summary>
        /// <param name="user">The user with updated information</param>
        /// <param name="modifiedBy">Who modified the user</param>
        Task UpdateAsync(User user, string modifiedBy = "UserService");

        /// <summary>
        /// Sets the active status of a user
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <param name="isActive">The new active status</param>
        /// <param name="modifiedBy">Who changed the status</param>
        Task SetActiveAsync(int userId, bool isActive, string modifiedBy = "UserService");
        Task<User?> GetUserByTokenAsync(string token);
        Task<bool> LogOutUserByIdAsync(int userId);
    }
}
