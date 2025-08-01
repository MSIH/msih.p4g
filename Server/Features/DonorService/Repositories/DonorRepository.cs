/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.EntityFrameworkCore;
using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.DonorService.Interfaces;
using msih.p4g.Server.Features.DonorService.Model;
using msih.p4g.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace msih.p4g.Server.Features.DonorService.Repositories
{
    /// <summary>
    /// Repository implementation for Donor entity
    /// </summary>
    public class DonorRepository : GenericRepository<Donor>, IDonorRepository
    {
        /// <summary>
        /// Initializes a new instance of the DonorRepository class
        /// </summary>
        /// <param name="contextFactory">The database context factory</param>
        public DonorRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : base(contextFactory)
        {
        }

        /// <inheritdoc />
        public async Task<List<Donor>> SearchAsync(string searchTerm)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<Donor>()
                .Where(d => d.PaymentProcessorDonorId != null && 
                       d.PaymentProcessorDonorId.Contains(searchTerm) && 
                       d.IsActive)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<List<Donor>> GetAllWithUserDataAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<Donor>()
                .Include(d => d.User)
                    .ThenInclude(u => u.Profile)
                .ToListAsync(); // Include all donors (active and inactive)
        }

        /// <inheritdoc />
        public async Task<PagedResult<Donor>> GetPaginatedWithUserDataAsync(PaginationParameters paginationParameters)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            IQueryable<Donor> query = context.Set<Donor>()
                .Include(d => d.User)
                    .ThenInclude(u => u.Profile);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(paginationParameters.SearchTerm))
            {
                var searchTerm = paginationParameters.SearchTerm.ToLower();
                query = query.Where(d => 
                    (d.PaymentProcessorDonorId != null && d.PaymentProcessorDonorId.ToLower().Contains(searchTerm)) ||
                    (d.User.Email != null && d.User.Email.ToLower().Contains(searchTerm)) ||
                    (d.User.Profile != null && d.User.Profile.FirstName != null && d.User.Profile.FirstName.ToLower().Contains(searchTerm)) ||
                    (d.User.Profile != null && d.User.Profile.LastName != null && d.User.Profile.LastName.ToLower().Contains(searchTerm))
                );
            }

            var totalCount = await query.CountAsync();
            
            var items = await query
                .Skip((paginationParameters.PageNumber - 1) * paginationParameters.PageSize)
                .Take(paginationParameters.PageSize)
                .ToListAsync();

            return new PagedResult<Donor>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = paginationParameters.PageNumber,
                PageSize = paginationParameters.PageSize
            };
        }
    }
}
