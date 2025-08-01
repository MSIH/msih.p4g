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
using msih.p4g.Server.Features.CampaignService.Model;

namespace msih.p4g.Server.Features.CampaignService.Interfaces
{
    /// <summary>
    /// Service interface for Campaign operations
    /// </summary>
    public interface ICampaignService
    {
        /// <summary>
        /// Gets all campaigns
        /// </summary>
        /// <param name="includeInactive">Whether to include inactive campaigns</param>
        /// <returns>List of campaigns</returns>
        Task<IEnumerable<Campaign>> GetAllAsync(bool includeInactive = false);

        /// <summary>
        /// Gets a campaign by ID
        /// </summary>
        /// <param name="id">The campaign ID</param>
        /// <returns>The campaign or null if not found</returns>
        Task<Campaign?> GetByIdAsync(int id);

        /// <summary>
        /// Adds a new campaign
        /// </summary>
        /// <param name="campaign">The campaign to add</param>
        /// <returns>The added campaign with its ID</returns>
        Task<Campaign> AddAsync(Campaign campaign);

        /// <summary>
        /// Updates an existing campaign
        /// </summary>
        /// <param name="campaign">The campaign to update</param>
        /// <returns>True if updated, false if not found</returns>
        Task<bool> UpdateAsync(Campaign campaign);

        /// <summary>
        /// Sets the active status of a campaign
        /// </summary>
        /// <param name="id">The ID of the campaign</param>
        /// <param name="isActive">The new active status</param>
        /// <param name="modifiedBy">Who modified the campaign</param>
        /// <returns>True if updated, false if not found</returns>
        Task<bool> SetActiveAsync(int id, bool isActive, string modifiedBy = "CampaignService");

        /// <summary>
        /// Gets the default campaign
        /// </summary>
        /// <returns>The default campaign or null if none is set as default</returns>
        Task<Campaign?> GetDefaultCampaignAsync();

        /// <summary>
        /// Sets a campaign as the default campaign
        /// </summary>
        /// <param name="campaignId">The ID of the campaign to set as default</param>
        /// <returns>True if the operation was successful, false otherwise</returns>
        Task<bool> SetDefaultCampaignAsync(int campaignId);
    }
}
