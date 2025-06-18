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
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.DonationService.Models;

namespace msih.p4g.Server.Features.DonationService.Interfaces
{
    /// <summary>
    /// Repository interface for Donation entity
    /// </summary>
    public interface IDonationRepository : IGenericRepository<Donation>
    {
        // Add Donation-specific repository methods here if needed
    }
}
