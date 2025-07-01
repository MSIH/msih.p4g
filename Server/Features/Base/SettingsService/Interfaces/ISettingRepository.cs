/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.Base.SettingsService.Model;

namespace msih.p4g.Server.Features.Base.SettingsService.Interfaces
{
    /// <summary>
    /// Repository interface for Setting entity
    /// </summary>
    public interface ISettingRepository : IGenericRepository<Setting>
    {
        /// <summary>
        /// Gets a setting by its key
        /// </summary>
        /// <param name="key">The setting key</param>
        /// <returns>The setting if found, otherwise null</returns>
        Task<Setting?> GetByKeyAsync(string key);
    }
}
