/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.EntityFrameworkCore;
using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.OrganizationService.Interfaces;
using msih.p4g.Server.Features.OrganizationService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace msih.p4g.Server.Features.OrganizationService.Repositories
{
    /// <summary>
    /// Repository implementation for Organization entity
    /// </summary>
    public class OrganizationRepository : GenericRepository<Organization>, IOrganizationRepository
    {
        /// <summary>
        /// Initializes a new instance of the OrganizationRepository class
        /// </summary>
        /// <param name="contextFactory">The database context factory</param>
        public OrganizationRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : base(contextFactory)
        {
        }

        /// <inheritdoc />
        public async Task<Organization?> GetByTaxIdAsync(string taxId, bool includeInactive = false)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.Set<Organization>().AsQueryable();
            
            if (!includeInactive)
            {
                query = query.Where(o => o.IsActive);
            }
            
            return await query.FirstOrDefaultAsync(o => o.TaxId == taxId);
        }
        
        /// <inheritdoc />
        public async Task<Organization?> GetWithRelatedDataAsync(int id, bool includeInactive = false)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.Set<Organization>().AsQueryable();
            
            if (!includeInactive)
            {
                query = query.Where(o => o.IsActive);
            }
            
            return await query
                .Include(o => o.Campaigns.Where(c => includeInactive || c.IsActive))
                .Include(o => o.Donations.Where(d => includeInactive || d.IsActive))
                .FirstOrDefaultAsync(o => o.Id == id);
        }
        
        /// <inheritdoc />
        public async Task<IEnumerable<Organization>> GetAllWithRelatedDataAsync(bool includeInactive = false)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.Set<Organization>().AsQueryable();
            
            if (!includeInactive)
            {
                query = query.Where(o => o.IsActive);
            }
            
            return await query
                .Include(o => o.Campaigns.Where(c => includeInactive || c.IsActive))
                .Include(o => o.Donations.Where(d => includeInactive || d.IsActive))
                .ToListAsync();
        }
    }
}
