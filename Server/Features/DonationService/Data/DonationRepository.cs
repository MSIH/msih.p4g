/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.DonationService.Models;
using Microsoft.EntityFrameworkCore;
using msih.p4g.Server.Common.Data;

namespace msih.p4g.Server.Features.DonationService.Data
{
    /// <summary>
    /// Concrete repository for Donation entity
    /// </summary>
    public class DonationRepository : GenericRepository<Donation, ApplicationDbContext>, IDonationRepository
    {
        public DonationRepository(ApplicationDbContext context) : base(context)
        {
        }
        // Add Donation-specific repository methods here if needed
    }
}
