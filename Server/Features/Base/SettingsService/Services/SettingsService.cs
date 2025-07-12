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
using msih.p4g.Server.Features.Base.SettingsService.Interfaces;
using msih.p4g.Server.Features.Base.SettingsService.Model;

namespace msih.p4g.Server.Features.Base.SettingsService.Services
{
    /// <summary>
    /// Implementation of ISettingsService using the setting repository
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private readonly ISettingRepository _settingsRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SettingsService> _logger;

        public SettingsService(ISettingRepository settingsRepository, IConfiguration configuration, ILogger<SettingsService> logger)
        {
            _settingsRepository = settingsRepository;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Gets a setting value by key, checking DB, then appsettings, then environment variables.
        /// If not found in DB, creates the key in DB with the resolved value (if any).
        /// </summary>
        public async Task<string?> GetValueAsync(string key)
        {
            var setting = (await _settingsRepository.FindAsync(s => s.Key == key)).FirstOrDefault();

            // If setting exists in DB, return its value (even if null)
            if (setting != null)
                return setting.Value;

            // Setting doesn't exist in DB, check other sources
            string? resolvedValue = null;

            // Try appsettings.json
            var appSettingValue = _configuration[key];
            if (!string.IsNullOrEmpty(appSettingValue))
            {
                resolvedValue = appSettingValue;
            }
            else
            {
                // Try environment variable
                var envValue = System.Environment.GetEnvironmentVariable(key);
                if (!string.IsNullOrEmpty(envValue))
                {
                    resolvedValue = envValue;
                }
            }

            // Create DB entry with the resolved value (or null if not found anywhere)
            await _settingsRepository.AddAsync(new Setting { Key = key, Value = resolvedValue });
            return resolvedValue;
        }

        /// <summary>
        /// Sets or updates a setting value in the DB.
        /// </summary>
        public async Task SetValueAsync(string key, string? value, string modifiedBy = "SettingsService")
        {
            var setting = (await _settingsRepository.FindAsync(s => s.Key == key)).FirstOrDefault();
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

        /// <summary>
        /// Cleans up duplicate settings and ensures data integrity
        /// </summary>
        public async Task<SettingsCleanupResult> CleanupSettingsAsync()
        {
            var result = new SettingsCleanupResult();

            try
            {
                _logger.LogInformation("Starting settings cleanup process...");

                // Step 1: Identify duplicates
                var duplicates = await IdentifyDuplicatesAsync();
                result.DuplicatesFound = duplicates.Count;
                _logger.LogInformation($"Found {duplicates.Count} duplicate setting keys");

                // Step 2: Remove duplicates (keep the most recent)
                result.DuplicatesRemoved = await RemoveDuplicatesAsync(duplicates);
                _logger.LogInformation($"Removed {result.DuplicatesRemoved} duplicate entries");

                // Step 3: Clean up invalid entries
                result.InvalidEntriesRemoved = await RemoveInvalidEntriesAsync();
                _logger.LogInformation($"Removed {result.InvalidEntriesRemoved} invalid entries");

                // Step 4: Verify cleanup
                await VerifyCleanupAsync();

                result.Success = true;
                _logger.LogInformation("Settings cleanup completed successfully");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Error during settings cleanup");
            }

            return result;
        }

        /// <summary>
        /// Identifies duplicate setting keys
        /// </summary>
        private async Task<List<string>> IdentifyDuplicatesAsync()
        {
            var allSettings = await _settingsRepository.GetAllAsync(includeInactive: true);

            return allSettings
                .GroupBy(s => s.Key)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
        }

        /// <summary>
        /// Removes duplicate entries, keeping the most recent one
        /// </summary>
        private async Task<int> RemoveDuplicatesAsync(List<string> duplicateKeys)
        {
            int removedCount = 0;

            foreach (var key in duplicateKeys)
            {
                var duplicateEntries = (await _settingsRepository.FindAsync(s => s.Key == key, includeInactive: true))
                    .OrderByDescending(s => s.ModifiedOn ?? s.CreatedOn)
                    .ToList();

                // Keep the first (most recent) entry, remove the rest
                var entriesToRemove = duplicateEntries.Skip(1).ToList();

                foreach (var entry in entriesToRemove)
                {
                    // Since ISettingRepository doesn't have Remove methods, we'll deactivate instead
                    await _settingsRepository.SetActiveStatusAsync(entry.Id, false, "System");
                    removedCount++;
                }

                if (entriesToRemove.Count > 0)
                {
                    _logger.LogInformation($"Deactivated {entriesToRemove.Count} duplicate entries for key: {key}");
                }
            }

            return removedCount;
        }

        /// <summary>
        /// Removes invalid entries (null/empty keys, etc.)
        /// </summary>
        private async Task<int> RemoveInvalidEntriesAsync()
        {
            var allSettings = await _settingsRepository.GetAllAsync(includeInactive: true);
            var invalidEntries = allSettings
                .Where(s => string.IsNullOrWhiteSpace(s.Key))
                .ToList();

            int removedCount = 0;
            foreach (var entry in invalidEntries)
            {
                // Deactivate invalid entries since we can't delete them
                await _settingsRepository.SetActiveStatusAsync(entry.Id, false, "System");
                removedCount++;
            }

            if (removedCount > 0)
            {
                _logger.LogInformation($"Deactivated {removedCount} invalid entries");
            }

            return removedCount;
        }

        /// <summary>
        /// Verifies the cleanup was successful
        /// </summary>
        private async Task VerifyCleanupAsync()
        {
            var duplicatesAfterCleanup = await IdentifyDuplicatesAsync();

            if (duplicatesAfterCleanup.Count > 0)
            {
                throw new InvalidOperationException($"Cleanup failed: {duplicatesAfterCleanup.Count} duplicates still exist");
            }

            var activeSettings = await _settingsRepository.GetActiveOnlyAsync();
            var totalActiveSettings = activeSettings.Count();
            _logger.LogInformation($"Cleanup verification successful. Total active settings: {totalActiveSettings}");
        }

        /// <summary>
        /// Result of the settings cleanup operation
        /// </summary>
        public class SettingsCleanupResult
        {
            public bool Success { get; set; }
            public int DuplicatesFound { get; set; }
            public int DuplicatesRemoved { get; set; }
            public int InvalidEntriesRemoved { get; set; }
            public string? ErrorMessage { get; set; }
        }

        /// <summary>
        /// Summary of the settings table state
        /// </summary>
        public class SettingsTableSummary
        {
            public int TotalEntries { get; set; }
            public int UniqueKeys { get; set; }
            public int DuplicateKeys { get; set; }
            public bool HasDuplicates { get; set; }
        }
    }
}
