/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.Base.SmsService.Model;
using System.Threading.Tasks;

namespace msih.p4g.Server.Features.Base.SmsService.Interfaces
{
    /// <summary>
    /// Repository interface for managing validated phone numbers
    /// </summary>
    public interface IValidatedPhoneNumberRepository : IGenericRepository<ValidatedPhoneNumber>
    {
        /// <summary>
        /// Gets a validated phone number from the database by phone number
        /// </summary>
        /// <param name="phoneNumber">The phone number to retrieve in E.164 format</param>
        /// <returns>The validated phone number if found, null otherwise</returns>
        Task<ValidatedPhoneNumber> GetByPhoneNumberAsync(string phoneNumber);
        
        /// <summary>
        /// Adds or updates a validated phone number in the database
        /// </summary>
        /// <param name="validatedPhoneNumber">The validated phone number to add or update</param>
        /// <returns>The added or updated validated phone number</returns>
        Task<ValidatedPhoneNumber> AddOrUpdateAsync(ValidatedPhoneNumber validatedPhoneNumber);
    }
}
