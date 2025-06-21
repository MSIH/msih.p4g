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
using msih.p4g.Server.Features.CampaignService.Model;

namespace msih.p4g.Server.Features.CampaignService.Interfaces
{
    /// <summary>
    /// Repository interface for Campaign entity
    /// </summary>
    public interface ICampaignRepository : IGenericRepository<Campaign>
    {
        /// <summary>
        /// Gets the default campaign
        /// </summary>
        /// <returns>The default campaign or null if none is set as default</returns>
        Task<Campaign?> GetDefaultCampaignAsync();

        /// <summary>
        /// Sets a campaign as the default campaign
        /// </summary>
        /// <param name="campaignId">The ID of the campaign to set as default</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task SetDefaultCampaignAsync(int campaignId);

        /// <summary>
        /// Gets all campaigns associated with a specific organization
        /// </summary>
        /// <param name="organizationId">The organization ID</param>
        /// <returns>List of campaigns for the specified organization</returns>
        Task<IEnumerable<Campaign>> GetCampaignsByOrganizationIdAsync(int organizationId);
        Task<Campaign?> GetByIdAsync(int id);
        Task<IEnumerable<Campaign>> GetAllAsync();

    }
}
