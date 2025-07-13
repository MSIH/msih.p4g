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
using msih.p4g.Server.Features.OrganizationService.Models;

namespace msih.p4g.Server.Features.OrganizationService.Interfaces
{
    /// <summary>
    /// Service interface for Organization operations
    /// </summary>
    public interface IOrganizationService
    {
        /// <summary>
        /// Gets all organizations
        /// </summary>
        /// <param name="includeInactive">Whether to include inactive organizations</param>
        /// <returns>All organizations matching the criteria</returns>
        Task<IEnumerable<Organization>> GetAllAsync(bool includeInactive = false);

        /// <summary>
        /// Gets all organizations with their related campaigns and donations
        /// </summary>
        /// <param name="includeInactive">Whether to include inactive organizations</param>
        /// <returns>All organizations with their related data</returns>
        Task<IEnumerable<Organization>> GetAllWithRelatedDataAsync(bool includeInactive = false);

        /// <summary>
        /// Gets an organization by its ID
        /// </summary>
        /// <param name="id">The organization ID</param>
        /// <param name="includeInactive">Whether to include inactive organizations</param>
        /// <returns>The organization with the specified ID, or null if not found</returns>
        Task<Organization?> GetByIdAsync(int id, bool includeInactive = false);

        /// <summary>
        /// Gets an organization by its tax ID (EIN)
        /// </summary>
        /// <param name="taxId">The tax ID to search for</param>
        /// <param name="includeInactive">Whether to include inactive organizations</param>
        /// <returns>The organization with the specified tax ID, or null if not found</returns>
        Task<Organization?> GetByTaxIdAsync(string taxId, bool includeInactive = false);

        /// <summary>
        /// Gets an organization with its campaigns and donations included
        /// </summary>
        /// <param name="id">The organization ID</param>
        /// <param name="includeInactive">Whether to include inactive campaigns</param>
        /// <returns>The organization with campaigns and donations, or null if not found</returns>
        Task<Organization?> GetWithRelatedDataAsync(int id, bool includeInactive = false);

        /// <summary>
        /// Adds a new organization
        /// </summary>
        /// <param name="organization">The organization to add</param>
        /// <param name="createdBy">The user who created the organization</param>
        /// <returns>The added organization</returns>
        Task<Organization> AddAsync(Organization organization, string createdBy = "OrganizationService");

        /// <summary>
        /// Updates an existing organization
        /// </summary>
        /// <param name="organization">The organization to update</param>
        /// <param name="modifiedBy">The user who modified the organization</param>
        /// <returns>The updated organization</returns>
        Task<Organization> UpdateAsync(Organization organization, string modifiedBy = "OrganizationService");

        /// <summary>
        /// Activates or deactivates an organization
        /// </summary>
        /// <param name="id">The ID of the organization</param>
        /// <param name="isActive">Whether the organization should be active</param>
        /// <param name="modifiedBy">The user who modified the organization</param>
        /// <returns>True if the organization was updated, false otherwise</returns>
        Task<bool> SetActiveStatusAsync(int id, bool isActive, string modifiedBy = "OrganizationService");
    }
}
