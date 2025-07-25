/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using MSIH.Core.Common.Data.Repositories;
using MSIH.Core.Services.Sms.Interfaces;
using MSIH.Core.Services.Sms.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MSIH.Core.Services.Sms.Services
{
    /// <summary>
    /// Service for phone number validation and retrieval, for use in Blazor Server DI
    /// </summary>
    public class PhoneValidationService
    {
        private readonly ISmsService _smsService;
        private readonly IValidatedPhoneNumberRepository _phoneNumberRepository;

        public PhoneValidationService(
            ISmsService smsService,
            IValidatedPhoneNumberRepository phoneNumberRepository)
        {
            _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
            _phoneNumberRepository = phoneNumberRepository ?? throw new ArgumentNullException(nameof(phoneNumberRepository));
        }

        /// <summary>
        /// Validates a phone number
        /// </summary>
        public async Task<ValidatedPhoneNumber> ValidatePhoneNumberAsync(string phoneNumber, bool useCache = true, bool usePaidService = false)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("Phone number is required", nameof(phoneNumber));
            return await _smsService.ValidatePhoneNumberAsync(phoneNumber, useCache, usePaidService);
        }

        /// <summary>
        /// Gets all validated phone numbers
        /// </summary>
        public async Task<IEnumerable<ValidatedPhoneNumber>> GetAllAsync(bool includeInactive = false)
        {
            var repository = _phoneNumberRepository as IGenericRepository<ValidatedPhoneNumber>;
            if (repository == null)
                throw new InvalidOperationException("Repository does not implement IGenericRepository");
            return await repository.GetAllAsync(includeInactive);
        }
    }
}
