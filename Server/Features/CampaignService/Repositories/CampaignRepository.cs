// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.EntityFrameworkCore;
using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Common.Interfaces;
using msih.p4g.Server.Features.CampaignService.Interfaces;
using msih.p4g.Server.Features.CampaignService.Model;

namespace msih.p4g.Server.Features.CampaignService.Repositories
{
    /// <summary>
    /// Repository implementation for Campaign entity
    /// </summary>
    public class CampaignRepository : GenericRepository<Campaign>, ICampaignRepository
    {
        /// <summary>
        /// Initializes a new instance of the CampaignRepository class
        /// </summary>
        /// <param name="contextFactory">The database context factory</param>
        public CampaignRepository(IDbContextFactory<ApplicationDbContext> contextFactory,
        ICacheStrategy cacheStrategy)
        : base(contextFactory, cacheStrategy)
        {
        }

        /// <inheritdoc />
        public async Task<Campaign?> GetDefaultCampaignAsync()
        {
            // Create a custom cache key for this specific operation
            var cacheKey = $"{typeof(Campaign).Name}_Default";

            // Try cache first if available
            if (_cacheStrategy != null)
            {
                var cachedResult = await _cacheStrategy.GetAsync<Campaign>(cacheKey);
                if (cachedResult != null)
                {
                    return cachedResult;
                }
            }

            using var context = await _contextFactory.CreateDbContextAsync();
            var defaultCampaign = await context.Campaigns
                .FirstOrDefaultAsync(c => c.IsDefault && c.IsActive);

            if (_cacheStrategy != null && defaultCampaign != null)
            {
                await _cacheStrategy.SetAsync(cacheKey, defaultCampaign);
            }

            return defaultCampaign;
        }

        /// <inheritdoc />
        public async Task SetDefaultCampaignAsync(int campaignId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var campaigns = await context.Campaigns.ToListAsync();
            foreach (var campaign in campaigns)
            {
                campaign.IsDefault = campaign.Id == campaignId;
            }
            await context.SaveChangesAsync();

            // Invalidate all campaign-related cache entries after update
            if (_cacheStrategy != null)
            {
                // Remove the default campaign cache
                var defaultCacheKey = $"{typeof(Campaign).Name}_Default";
                await _cacheStrategy.RemoveAsync(defaultCacheKey);

                // Invalidate collection-level cache (e.g., GetAll, ActiveOnly, InactiveOnly)
                await InvalidateCollectionCacheAsync();

                // Invalidate cache for each affected campaign entity
                foreach (var campaign in campaigns)
                {
                    await InvalidateEntityCacheAsync(campaign.Id);
                }
            }
        }

        // Add Campaign-specific repository methods here if needed
    }
}
