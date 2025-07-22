/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

using MSIH.Core.Common.Data;
using MSIH.Core.Common.Data.Repositories;
using MSIH.Core.Common.Models;
using MSIH.Core.Services.Profile.Interfaces;
using Microsoft.EntityFrameworkCore;
using ProfileEntity = MSIH.Core.Services.Profile.Model.Profile;

namespace MSIH.Core.Services.Profile.Repositories
{
    public class ProfileRepository : GenericRepository<ProfileEntity>, IProfileRepository
    {
        public ProfileRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : base(contextFactory)
        {
        }

        /// <inheritdoc />
        public async Task<List<ProfileEntity>> GetAllWithUserDataAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<ProfileEntity>()
                .Include(p => p.User)
                .ToListAsync(); // Include all profiles (active and inactive)
        }

        /// <inheritdoc />
        public async Task<PagedResult<ProfileEntity>> GetPaginatedWithUserDataAsync(PaginationParameters paginationParameters)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            IQueryable<ProfileEntity> query = context.Set<ProfileEntity>()
                .Include(p => p.User);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(paginationParameters.SearchTerm))
            {
                var searchTerm = paginationParameters.SearchTerm.ToLower();
                query = query.Where(p => 
                    (p.FirstName != null && p.FirstName.ToLower().Contains(searchTerm)) ||
                    (p.LastName != null && p.LastName.ToLower().Contains(searchTerm)) ||
                    (p.ReferralCode != null && p.ReferralCode.ToLower().Contains(searchTerm)) ||
                    (p.MobileNumber != null && p.MobileNumber.ToLower().Contains(searchTerm)) ||
                    (p.User.Email != null && p.User.Email.ToLower().Contains(searchTerm))
                );
            }

            var totalCount = await query.CountAsync();
            
            var items = await query
                .Skip((paginationParameters.PageNumber - 1) * paginationParameters.PageSize)
                .Take(paginationParameters.PageSize)
                .ToListAsync();

            return new PagedResult<ProfileEntity>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = paginationParameters.PageNumber,
                PageSize = paginationParameters.PageSize
            };
        }
    }
}
