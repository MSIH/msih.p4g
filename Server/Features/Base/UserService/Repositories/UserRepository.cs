// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

using Microsoft.EntityFrameworkCore;
using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.Base.UserService.Interfaces;
using msih.p4g.Server.Features.Base.UserService.Models;

namespace msih.p4g.Server.Features.Base.UserService.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : base(contextFactory)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<User>().FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
        }
        // All other CRUD methods are inherited from GenericRepository

        public async Task<User?> GetByEmailAsync(string email, bool includeProfile = false, bool includeAddress = false, bool includeDonor = false, bool includeFundraiser = false)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.Set<User>().AsQueryable();

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


        public async Task<User?> GetUserByTokenAsync(string token)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var user = await context.Set<User>().FirstOrDefaultAsync(u => u.EmailVerificationToken == token && u.IsActive);
            return user;
        }
    }
}
