// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

using MSIH.Core.Common.Data.Repositories;
using MSIH.Core.Services.User.Models;

namespace MSIH.Core.Services.User.Interfaces
{
    public interface IUserRepository : IGenericRepository<MSIH.Core.Services.User.Models.User>
    {
        Task<MSIH.Core.Services.User.Models.User?> GetByEmailAsync(string email);
        // Add any additional custom methods for User here

        Task<MSIH.Core.Services.User.Models.User?> GetByEmailAsync(string email, bool includeProfile = false, bool includeAddress = false, bool includeDonor = false, bool includeFundraiser = false);
        Task<MSIH.Core.Services.User.Models.User?> GetUserByTokenAsync(string emailToken);

        Task<MSIH.Core.Services.User.Models.User?> GetByReferralCodeAsync(string referralCode, bool includeProfile = false, bool includeAddress = false, bool includeDonor = false, bool includeFundraiser = false);
    }
}
