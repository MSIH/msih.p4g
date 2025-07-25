// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

using Microsoft.EntityFrameworkCore;
using MSIH.Core.Common.Data;
using MSIH.Core.Common.Data.Repositories;
using MSIH.Core.Services.Users.Interfaces;
using UserEntity = MSIH.Core.Services.Users.Models.User;

namespace MSIH.Core.Services.Users.Repositories
{
    public class UserRepository : GenericRepository<UserEntity>, IUserRepository
    {
        public UserRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : base(contextFactory)
        {
        }

        public async Task<UserEntity?> GetByEmailAsync(string email)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<UserEntity>().FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
        }
        // All other CRUD methods are inherited from GenericRepository

        public async Task<UserEntity?> GetByEmailAsync(string email, bool includeProfile = false, bool includeAddress = false, bool includeDonor = false, bool includeFundraiser = false)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.Set<UserEntity>().AsQueryable();

            // If we need to include the profile
            if (includeProfile)
            {
                // If we also need the address, use ThenInclude
                if (includeAddress)
                    query = query.Include(u => u.Profile)
                                 .ThenInclude(p => p.Address);
                else
                    query = query.Include(u => u.Profile);
            }

            if (includeDonor)
                query = query.Include(u => u.Donor);

            if (includeFundraiser)
                query = query.Include(u => u.Fundraiser);

            return await query.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
        }


        public async Task<UserEntity?> GetUserByTokenAsync(string token)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var user = await context.Set<UserEntity>()
                .Include(u => u.Profile)  // Include the Profile
                .FirstOrDefaultAsync(u => u.EmailVerificationToken == token && u.IsActive);
            return user;
        }

        public async Task<UserEntity?> GetByReferralCodeAsync(string referralCode, bool includeProfile = false, bool includeAddress = false, bool includeDonor = false, bool includeFundraiser = false)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.Set<UserEntity>().AsQueryable();

            // If we need to include the profile
            if (includeProfile)
            {
                // If we also need the address, use ThenInclude
                if (includeAddress)
                    query = query.Include(u => u.Profile)
                                 .ThenInclude(p => p.Address);
                else
                    query = query.Include(u => u.Profile);
            }

            if (includeDonor)
                query = query.Include(u => u.Donor);

            if (includeFundraiser)
                query = query.Include(u => u.Fundraiser);

            return await query.FirstOrDefaultAsync(u => u.Profile.ReferralCode == referralCode && u.IsActive);
        }
    }
}
