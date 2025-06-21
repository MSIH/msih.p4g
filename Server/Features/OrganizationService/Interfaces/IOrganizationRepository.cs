/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.OrganizationService.Models;

namespace msih.p4g.Server.Features.OrganizationService.Interfaces
{
    /// <summary>
    /// Repository interface for Organization entity
    /// </summary>
    public interface IOrganizationRepository : IGenericRepository<Organization>
    {
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
        /// Gets all organizations with their campaigns and donations included
        /// </summary>
        /// <param name="includeInactive">Whether to include inactive organizations</param>
        /// <returns>All organizations with their campaigns and donations</returns>
        Task<IEnumerable<Organization>> GetAllWithRelatedDataAsync(bool includeInactive = false);
    }
}
