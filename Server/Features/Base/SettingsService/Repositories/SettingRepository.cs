/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.EntityFrameworkCore;
using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.Base.SettingsService.Interfaces;
using msih.p4g.Server.Features.Base.SettingsService.Model;

namespace msih.p4g.Server.Features.Base.SettingsService.Repositories
{
    /// <summary>
    /// Repository implementation for Setting entity
    /// </summary>
    public class SettingRepository : GenericRepository<Setting>, ISettingRepository
    {
        /// <summary>
        /// Initializes a new instance of the SettingRepository class
        /// </summary>
        /// <param name="contextFactory">The database context factory</param>
        public SettingRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : base(contextFactory)
        {
        }

        /// <summary>
        /// Gets a setting by its key
        /// </summary>
        /// <param name="key">The setting key</param>
        /// <returns>The setting if found, otherwise null</returns>
        public async Task<Setting?> GetByKeyAsync(string key)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<Setting>()
                .FirstOrDefaultAsync(s => s.Key == key && s.IsActive);
        }
    }
}
