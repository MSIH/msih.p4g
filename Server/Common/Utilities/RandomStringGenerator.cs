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
using System.Text;

namespace msih.p4g.Server.Common.Utilities
{
    /// <summary>
    /// Provides methods to generate pseudo-random strings with configurable character sets and length.
    /// </summary>
    public static class RandomStringGenerator
    {
        // Character sets without confusing characters
        private const string _uppercase = "ABCDEFGHJKMNPQRSTUVWXYZ"; // Removed I, L, O
        private const string _lowercase = "abcdefghjkmnpqrstuvwxyz"; // Removed i, l, o
        private const string _numbers = "23456789"; // Removed 0, 1

        [Flags]
        public enum CharSet
        {
            Uppercase = 1,
            Lowercase = 2,
            Numbers = 4,
            LowercaseAndUppercase = Uppercase | Lowercase,
            UppercaseAndNumbers = Uppercase | Numbers,
            LowercaseAndNumbers = Lowercase | Numbers,
            All = Uppercase | Lowercase | Numbers
        }

        /// <summary>
        /// Generates a pseudo-random string.
        /// </summary>
        /// <param name="length">The length of the string to generate.</param>
        /// <param name="charSet">The character set to use.</param>
        /// <returns>A pseudo-random string.</returns>
        public static string Generate(int id, int length = 5, CharSet charSet = CharSet.All)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be positive.");

            var chars = new StringBuilder();
            if (charSet.HasFlag(CharSet.Uppercase))
                chars.Append(_uppercase);
            if (charSet.HasFlag(CharSet.Lowercase))
                chars.Append(_lowercase);
            if (charSet.HasFlag(CharSet.Numbers))
                chars.Append(_numbers);

            if (chars.Length == 0)
                throw new ArgumentException("At least one character set must be specified.", nameof(charSet));

            var result = new StringBuilder(length);
            var random = new Random(id);
            for (int i = 0; i < length; i++)
            {
                int idx = random.Next(chars.Length);
                result.Append(chars[idx]);
            }
            return result.ToString();
        }
    }
}
