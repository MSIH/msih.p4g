/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.CampaignService.Interfaces;
using msih.p4g.Server.Features.CampaignService.Model;
using Microsoft.EntityFrameworkCore;

namespace msih.p4g.Server.Features.CampaignService.Repositories
{
    /// <summary>
    /// Repository implementation for Campaign entity
    /// </summary>
    public class CampaignRepository : GenericRepository<Campaign, ApplicationDbContext>, ICampaignRepository
    {
        /// <summary>
        /// Initializes a new instance of the CampaignRepository class
        /// </summary>
        /// <param name="context">The database context</param>
        public CampaignRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <inheritdoc />
        public async Task<Campaign?> GetDefaultCampaignAsync()
        {
            return await _context.Campaigns
                .FirstOrDefaultAsync(c => c.IsDefault && c.IsActive);
        }

        /// <inheritdoc />
        public async Task SetDefaultCampaignAsync(int campaignId)
        {
            var campaigns = await _context.Campaigns.ToListAsync();
            foreach (var campaign in campaigns)
            {
                campaign.IsDefault = campaign.Id == campaignId;
            }
            await _context.SaveChangesAsync();
        }

        // Add Campaign-specific repository methods here if needed
    }
}
