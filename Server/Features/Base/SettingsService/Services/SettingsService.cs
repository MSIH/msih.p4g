/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.Base.SettingsService.Interfaces;
using msih.p4g.Shared.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace msih.p4g.Server.Features.Base.SettingsService.Services
{
    /// <summary>
    /// Implementation of ISettingsService using the generic repository
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private readonly IGenericRepository<Setting> _settingsRepository;
        private readonly IConfiguration _configuration;

        public SettingsService(IGenericRepository<Setting> settingsRepository, IConfiguration configuration)
        {
            _settingsRepository = settingsRepository;
            _configuration = configuration;
        }

        /// <summary>
        /// Gets a setting value by key, checking DB, then appsettings, then environment variables.
        /// If not found in DB, creates the key in DB with the resolved value (if any).
        /// </summary>
        public async Task<string?> GetValueAsync(string key)
        {
            var setting = (await _settingsRepository.FindAsync(s => s.Key == key)).FirstOrDefault();
            if (setting != null)
                return setting.Value;

            // Try appsettings.json
            var appSettingValue = _configuration[key];
            if (!string.IsNullOrEmpty(appSettingValue))
            {
                // Populate DB for future use
                await _settingsRepository.AddAsync(new Setting { Key = key, Value = appSettingValue });
                return appSettingValue;
            }

            // Try environment variable
            var envValue = System.Environment.GetEnvironmentVariable(key);
            if (!string.IsNullOrEmpty(envValue))
            {
                await _settingsRepository.AddAsync(new Setting { Key = key, Value = envValue });
                return envValue;
            }

            // Optionally, create a DB entry with null value if not found anywhere
            await _settingsRepository.AddAsync(new Setting { Key = key, Value = null });
            return null;
        }

        /// <summary>
        /// Sets or updates a setting value in the DB.
        /// </summary>
        public async Task SetValueAsync(string key, string? value, string modifiedBy = "System")
        {
            var setting = (await _settingsRepository.FindAsync(s => s.Key == key, true, true)).FirstOrDefault();
            if (setting == null)
            {
                await _settingsRepository.AddAsync(new Setting { Key = key, Value = value }, modifiedBy);
            }
            else
            {
                setting.Value = value;
                await _settingsRepository.UpdateAsync(setting, modifiedBy);
            }
        }

        /// <summary>
        /// Gets all settings from the DB as a dictionary.
        /// </summary>
        public async Task<IReadOnlyDictionary<string, string?>> GetAllAsync()
        {
            var all = await _settingsRepository.GetAllAsync();
            return all.ToDictionary(s => s.Key, s => s.Value);
        }
    }
}

