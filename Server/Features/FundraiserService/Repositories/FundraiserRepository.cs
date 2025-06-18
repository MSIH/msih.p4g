/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.FundraiserService.Interfaces;
using msih.p4g.Server.Features.FundraiserService.Model;

namespace msih.p4g.Server.Features.FundraiserService.Repositories
{
    /// <summary>
    /// Repository implementation for Fundraiser entity
    /// </summary>
    public class FundraiserRepository : GenericRepository<Fundraiser, ApplicationDbContext>, IFundraiserRepository
    {
        /// <summary>
        /// Initializes a new instance of the FundraiserRepository class
        /// </summary>
        /// <param name="context">The database context</param>
        public FundraiserRepository(ApplicationDbContext context) : base(context)
        {
        }
        
        // Add Fundraiser-specific repository methods here if needed
    }
}
