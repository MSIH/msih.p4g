/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.DonationService.Interfaces;
using msih.p4g.Server.Features.DonationService.Models;
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
        // Add Donation-specific repository methods here if needed
    }
}
