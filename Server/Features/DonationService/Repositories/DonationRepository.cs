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
using msih.p4g.Server.Features.DonationService.Interfaces;
using msih.p4g.Server.Features.DonationService.Models;

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

        // get donation by email and include campaign name
        public async Task<Donation?> GetByDonorIdAsync(int id, bool includeCampaignName = false)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.Set<Donation>().AsQueryable();
            if (includeCampaignName)
            {
                query = query.Include(d => d.Campaign);
            }
            return await query.FirstOrDefaultAsync(d => d.Id == id);
        }
    }
}
