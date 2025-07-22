/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

using static MSIH.Core.Services.Setting.Services.SettingsService;

namespace MSIH.Core.Services.Setting.Interfaces
{
    /// <summary>
    /// Service for managing application settings (Email, SMS, etc.)
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// Gets a setting value by key, checking DB, then appsettings, then environment variables.
        /// </summary>
        Task<string?> GetValueAsync(string key);

        /// <summary>
        /// Sets or updates a setting value in the DB.
        /// </summary>
        Task SetValueAsync(string key, string? value, string modifiedBy = "SettingsService");

        /// <summary>
        /// Gets all settings from the DB as a dictionary.
        /// </summary>
        Task<IReadOnlyDictionary<string, string?>> GetAllAsync();

        /// <summary>
        /// CleanSettingsAsync
        ///
        /// Cleans up settings by removing any that are not in the current configuration.
        /// </summary>
        Task<SettingsCleanupResult> CleanupSettingsAsync();
    }
}