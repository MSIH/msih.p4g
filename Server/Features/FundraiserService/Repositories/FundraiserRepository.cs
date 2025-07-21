/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.FundraiserService.Interfaces;
using msih.p4g.Server.Features.FundraiserService.Model;
using msih.p4g.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace msih.p4g.Server.Features.FundraiserService.Repositories
{
    /// <summary>
    /// Repository implementation for Fundraiser entity
    /// </summary>
    public class FundraiserRepository : GenericRepository<Fundraiser>, IFundraiserRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        /// <summary>
        /// Initializes a new instance of the FundraiserRepository class
        /// </summary>
        /// <param name="contextFactory">The database context factory</param>
        public FundraiserRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : base(contextFactory)
        {
            _contextFactory = contextFactory;
        }
        
        /// <summary>
        /// Gets a fundraiser by user ID
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The fundraiser if found, otherwise null</returns>
        public async Task<Fundraiser?> GetByUserIdAsync(int userId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<Fundraiser>()
                .FirstOrDefaultAsync(f => f.UserId == userId && f.IsActive);
        }

        /// <inheritdoc />
        public async Task<PagedResult<Fundraiser>> GetPaginatedWithUserDataAsync(PaginationParameters paginationParameters)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            IQueryable<Fundraiser> query = context.Set<Fundraiser>()
                .Include(f => f.User)
                .Include(f => f.User.Profile);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(paginationParameters.SearchTerm))
            {
                var searchTerm = paginationParameters.SearchTerm.ToLower();
                query = query.Where(f => 
                    (f.PayoutAccount != null && f.PayoutAccount.ToLower().Contains(searchTerm)) ||
                    (f.SuspensionReason != null && f.SuspensionReason.ToLower().Contains(searchTerm)) ||
                    (f.User.Email != null && f.User.Email.ToLower().Contains(searchTerm)) ||
                    (f.User.Profile != null && f.User.Profile.FirstName != null && f.User.Profile.FirstName.ToLower().Contains(searchTerm)) ||
                    (f.User.Profile != null && f.User.Profile.LastName != null && f.User.Profile.LastName.ToLower().Contains(searchTerm))
                );
            }

            var totalCount = await query.CountAsync();
            
            var items = await query
                .Skip((paginationParameters.PageNumber - 1) * paginationParameters.PageSize)
                .Take(paginationParameters.PageSize)
                .ToListAsync();

            return new PagedResult<Fundraiser>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = paginationParameters.PageNumber,
                PageSize = paginationParameters.PageSize
            };
        }
    }
}
