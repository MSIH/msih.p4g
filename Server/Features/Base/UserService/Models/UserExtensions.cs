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
using Server.Common.Utilities;
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
        public static string
            GenerateEmailVerificationToken(this User user)
        {

            // based ont he current time get number in format of yyyyMMddHHmmss
            var currentTime = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            // covert to int
            var currentTimeInt = int.Parse(currentTime);

            // Generate a verification token
            var token = RandomStringGenerator.Generate(user.Id + currentTimeInt, 8, RandomStringGenerator.CharSet.All);

            return token;
        }

        /// <summary>
        /// Validates an email verification token
        /// </summary>
        /// <param name="token">The token to validate</param>
        /// <param name="secret">Secret key for token validation</param>
        /// <returns>User ID if valid, null if invalid</returns>
        public static (bool isValid, int? userId) ValidateEmailVerificationToken(string token)
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
