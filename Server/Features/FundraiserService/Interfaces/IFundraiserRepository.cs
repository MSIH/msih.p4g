/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.FundraiserService.Model;
using System.Threading.Tasks;

namespace msih.p4g.Server.Features.FundraiserService.Interfaces
{
    /// <summary>
    /// Repository interface for Fundraiser entity
    /// </summary>
    public interface IFundraiserRepository : IGenericRepository<Fundraiser>
    {
        /// <summary>
        /// Gets a fundraiser by user ID
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The fundraiser if found, otherwise null</returns>
        Task<Fundraiser?> GetByUserIdAsync(int userId);
    }
}
