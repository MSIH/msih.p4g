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
using System.Security.Cryptography;
using System.Text;

namespace msih.p4g.Server.Features.Base.UserService.Utilities
{
    public static class UserExtensions
    {
        /// <summary>
        /// Generates a verification token for email confirmation
        /// </summary>
        /// <param name="user">The user to generate a token for</param>
        /// <param name="secret">Secret key for token generation</param>
        /// <param name="expiryHours">Hours until token expiry</param>
        /// <returns>A verification token</returns>
        public static string GenerateEmailVerificationToken(this User user, string secret, int expiryHours = 24)
        {
            // Create a unique value using user data and expiry time
            var expiry = DateTime.UtcNow.AddHours(expiryHours);
            var dataToHash = $"{user.Id}:{user.Email}:{expiry.Ticks}:{secret}";

            // Create a hash of the data
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(dataToHash));
                var hashString = Convert.ToBase64String(hashBytes);

                // Combine token data (userId, expiry, hash) in a URL-safe format
                return Convert.ToBase64String(
                    Encoding.UTF8.GetBytes($"{user.Id}:{expiry.Ticks}:{hashString}")
                ).Replace('/', '_').Replace('+', '-').Replace('=', '~');
            }
        }

        /// <summary>
        /// Validates an email verification token
        /// </summary>
        /// <param name="token">The token to validate</param>
        /// <param name="secret">Secret key for token validation</param>
        /// <returns>User ID if valid, null if invalid</returns>
        public static (bool isValid, int? userId) ValidateEmailVerificationToken(string token, string secret)
        {
            try
            {
                // Decode the token
                var decodedToken = Encoding.UTF8.GetString(
                    Convert.FromBase64String(token.Replace('_', '/').Replace('-', '+').Replace('~', '='))
                );

                // Split the token parts
                var parts = decodedToken.Split(':');
                if (parts.Length != 3)
                    return (false, null);

                // Extract token data
                var userId = int.Parse(parts[0]);
                var expiryTicks = long.Parse(parts[1]);
                var hash = parts[2];

                // Check if token has expired
                var expiry = new DateTime(expiryTicks);
                if (DateTime.UtcNow > expiry)
                    return (false, null);

                return (true, userId);
            }
            catch
            {
                return (false, null);
            }
        }
    }
}
