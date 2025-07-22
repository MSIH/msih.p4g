/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using System.Threading.Tasks;

namespace MSIH.Core.Services.Email.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string from, string subject, string htmlContent);
        
        /// <summary>
        /// Validates if the provided email address is in a valid format.
        /// </summary>
        /// <param name="email">The email address to validate.</param>
        /// <returns>True if the email is valid, false otherwise.</returns>
        bool IsValidEmail(string email);
    }
}