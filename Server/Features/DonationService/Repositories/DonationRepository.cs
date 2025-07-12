/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.DonationService.Interfaces;
using msih.p4g.Server.Features.DonationService.Models;
using msih.p4g.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace msih.p4g.Server.Features.DonationService.Repositories
{
    /// <summary>
    /// Concrete repository for Donation entity
    /// </summary>
    public class DonationRepository : GenericRepository<Donation>, IDonationRepository
    {
        public DonationRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : base(contextFactory)
        {
        }

        /// <summary>
        /// Gets paginated donations for a specific user by email.
        /// </summary>
        public async Task<PagedResult<Donation>> GetPagedByUserEmailAsync(string email, PaginationParameters parameters)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var query = context.Donations
                .Include(d => d.Donor)
                    .ThenInclude(donor => donor.User)
                .Include(d => d.Campaign)
                .Where(d => d.Donor.User.Email == email);

            // Apply filters
            if (!string.IsNullOrWhiteSpace(parameters.FilterType))
            {
                query = parameters.FilterType.ToLower() switch
                {
                    "onetime" => query.Where(d => !d.IsMonthly && !d.IsAnnual),
                    "monthly" => query.Where(d => d.IsMonthly),
                    "annual" => query.Where(d => d.IsAnnual),
                    "recurring" => query.Where(d => d.IsMonthly || d.IsAnnual),
                    _ => query
                };
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination and ordering
            var items = await query
                .OrderByDescending(d => d.CreatedOn)
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return new PagedResult<Donation>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }

        /// <summary>
        /// Searches for paginated donations for a specific user by email.
        /// </summary>
        public async Task<PagedResult<Donation>> SearchPagedByUserEmailAsync(string email, PaginationParameters parameters)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var query = context.Donations
                .Include(d => d.Donor)
                    .ThenInclude(donor => donor.User)
                .Include(d => d.Campaign)
                .Where(d => d.Donor.User.Email == email);

            // Apply search
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                var searchTerm = parameters.SearchTerm.ToLower();
                query = query.Where(d => 
                    (d.DonationMessage != null && d.DonationMessage.ToLower().Contains(searchTerm)) ||
                    (d.CampaignCode != null && d.CampaignCode.ToLower().Contains(searchTerm)) ||
                    (d.ReferralCode != null && d.ReferralCode.ToLower().Contains(searchTerm)) ||
                    (d.Campaign != null && d.Campaign.Title != null && d.Campaign.Title.ToLower().Contains(searchTerm)) ||
                    d.DonationAmount.ToString().Contains(searchTerm));
            }

            // Apply filters
            if (!string.IsNullOrWhiteSpace(parameters.FilterType))
            {
                query = parameters.FilterType.ToLower() switch
                {
                    "onetime" => query.Where(d => !d.IsMonthly && !d.IsAnnual),
                    "monthly" => query.Where(d => d.IsMonthly),
                    "annual" => query.Where(d => d.IsAnnual),
                    "recurring" => query.Where(d => d.IsMonthly || d.IsAnnual),
                    _ => query
                };
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination and ordering
            var items = await query
                .OrderByDescending(d => d.CreatedOn)
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return new PagedResult<Donation>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }
    }
}
