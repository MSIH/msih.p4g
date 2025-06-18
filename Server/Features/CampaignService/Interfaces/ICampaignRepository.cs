/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.CampaignService.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace msih.p4g.Server.Features.CampaignService.Interfaces
{
    /// <summary>
    /// Repository interface for Campaign entity
    /// </summary>
    public interface ICampaignRepository : IGenericRepository<Campaign>
    {
        // Add Campaign-specific repository methods here if needed
    }
}
