/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using msih.p4g.Server.Features.CampaignService.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace msih.p4g.Server.Features.CampaignService.Interfaces
{
    /// <summary>
    /// Service interface for Campaign operations
    /// </summary>
    public interface ICampaignService
    {
        /// <summary>
        /// Gets all active campaigns
        /// </summary>
        /// <returns>List of campaigns</returns>
        Task<IEnumerable<Campaign>> GetAllAsync();
        
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
        /// Deletes a campaign (soft delete)
        /// </summary>
        /// <param name="id">The ID of the campaign to delete</param>
        /// <returns>True if deleted, false if not found</returns>
        Task<bool> DeleteAsync(int id);
    }
}
