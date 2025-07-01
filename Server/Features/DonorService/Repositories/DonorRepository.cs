/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.EntityFrameworkCore;
using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.DonorService.Interfaces;
using msih.p4g.Server.Features.DonorService.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace msih.p4g.Server.Features.DonorService.Repositories
{
    /// <summary>
    /// Repository implementation for Donor entity
    /// </summary>
    public class DonorRepository : GenericRepository<Donor>, IDonorRepository
    {
        /// <summary>
        /// Initializes a new instance of the DonorRepository class
        /// </summary>
        /// <param name="contextFactory">The database context factory</param>
        public DonorRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : base(contextFactory)
        {
        }

        /// <inheritdoc />
        public async Task<List<Donor>> SearchAsync(string searchTerm)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<Donor>()
                .Where(d => d.PaymentProcessorDonorId != null && 
                       d.PaymentProcessorDonorId.Contains(searchTerm) && 
                       d.IsActive)
                .ToListAsync();
        }
    }
}
