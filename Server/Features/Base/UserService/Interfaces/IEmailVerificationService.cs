// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using msih.p4g.Server.Features.Base.UserService.Models;

namespace msih.p4g.Server.Features.Base.UserService.Interfaces
{
    public interface IEmailVerificationService
    {
        /// <summary>
        /// Sends a verification email to a user
        /// </summary>
        /// <param name="user">The user to send verification to</param>
        /// <returns>True if email was sent successfully</returns>
        Task<bool> SendVerificationEmailAsync(User user);

        /// <summary>
        /// Verifies a user's email using a token
        /// </summary>
        /// <param name="token">The verification token</param>
        /// <returns>True if verification was successful</returns>
        Task<bool> VerifyEmailAsync(string token);

        /// <summary>
        /// Checks if a user's email is verified
        /// </summary>
        /// <param name="userId">The user ID to check</param>
        /// <returns>True if email is verified</returns>
        Task<bool> IsEmailVerifiedAsync(int userId);
    }
}
