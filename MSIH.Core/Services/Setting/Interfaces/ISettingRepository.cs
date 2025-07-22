/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using MSIH.Core.Common.Data.Repositories;
using MSIH.Core.Services.Setting.Models;

namespace MSIH.Core.Services.Setting.Interfaces
{
    /// <summary>
    /// Repository interface for Setting entity
    /// </summary>
    public interface ISettingRepository : IGenericRepository<Models.Setting>
    {
        /// <summary>
        /// Gets a setting by its key
        /// </summary>
        /// <param name="key">The setting key</param>
        /// <returns>The setting if found, otherwise null</returns>
        Task<Models.Setting?> GetByKeyAsync(string key);
    }
}