/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.Base.ProfileService.Interfaces;
using msih.p4g.Server.Features.Base.ProfileService.Model;
using msih.p4g.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace msih.p4g.Server.Features.Base.ProfileService.Repositories
{
    public class ProfileRepository : GenericRepository<Profile>, IProfileRepository
    {
        public ProfileRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : base(contextFactory)
        {
        }

        /// <inheritdoc />
        public async Task<List<Profile>> GetAllWithUserDataAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<Profile>()
                .Include(p => p.User)
                .ToListAsync(); // Include all profiles (active and inactive)
        }

        /// <inheritdoc />
        public async Task<PagedResult<Profile>> GetPaginatedWithUserDataAsync(PaginationParameters paginationParameters)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            IQueryable<Profile> query = context.Set<Profile>()
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

            return new PagedResult<Profile>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = paginationParameters.PageNumber,
                PageSize = paginationParameters.PageSize
            };
        }
    }
}
