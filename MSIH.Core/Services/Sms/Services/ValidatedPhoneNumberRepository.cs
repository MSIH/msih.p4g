/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.EntityFrameworkCore;
using MSIH.Core.Common.Data;
using MSIH.Core.Common.Data.Repositories;
using MSIH.Core.Services.Sms.Interfaces;
using MSIH.Core.Services.Sms.Models;

namespace MSIH.Core.Services.Sms.Services
{
    /// <summary>
    /// Repository implementation for managing validated phone numbers in the database
    /// </summary>
    public class ValidatedPhoneNumberRepository : GenericRepository<ValidatedPhoneNumber>, IValidatedPhoneNumberRepository
    {
        public ValidatedPhoneNumberRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : base(contextFactory)
        {
        }

        /// <inheritdoc />
        public async Task<ValidatedPhoneNumber> GetByPhoneNumberAsync(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ArgumentException("Phone number cannot be null or empty", nameof(phoneNumber));
            }

            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<ValidatedPhoneNumber>()
                .FirstOrDefaultAsync(p => p.PhoneNumber == phoneNumber && p.IsActive) ?? null!;
        }

        /// <inheritdoc />
        public override async Task<ValidatedPhoneNumber> AddAsync(ValidatedPhoneNumber entity, string createdBy = "SmsService")
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            // Set validation time
            entity.ValidatedOn = DateTime.UtcNow;

            return await base.AddAsync(entity, createdBy);
        }

        /// <inheritdoc />
        public async Task<ValidatedPhoneNumber> AddOrUpdateAsync(ValidatedPhoneNumber validatedPhoneNumber)
        {
            if (validatedPhoneNumber == null)
            {
                throw new ArgumentNullException(nameof(validatedPhoneNumber));
            }

            var existingRecord = await GetByPhoneNumberAsync(validatedPhoneNumber.PhoneNumber);

            if (existingRecord == null)
            {
                // Add new record
                return await AddAsync(validatedPhoneNumber);
            }
            else
            {
                // Update existing record
                existingRecord.IsMobile = validatedPhoneNumber.IsMobile;
                existingRecord.Carrier = validatedPhoneNumber.Carrier;
                existingRecord.CountryCode = validatedPhoneNumber.CountryCode;
                existingRecord.IsValid = validatedPhoneNumber.IsValid;
                existingRecord.ValidatedOn = DateTime.UtcNow;

                return await UpdateAsync(existingRecord);
            }
        }
    }
}
