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

namespace msih.p4g.Server.Features.Base.SettingsService.Services
{
    /// <summary>
    /// Initializes settings in the database from appsettings
    /// </summary>
    public class SettingsInitializer
    {
        private readonly ISettingsService _settingsService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SettingsInitializer> _logger;

        public SettingsInitializer(
            ISettingsService settingsService,
            IConfiguration configuration,
            ILogger<SettingsInitializer> logger)
        {
            _settingsService = settingsService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Initializes settings from appsettings.json into the database
        /// </summary>
        public async Task InitializeSettingsAsync()
        {
            try
            {
                _logger.LogInformation("Starting settings initialization...");

                // Initialize all connection strings
                await InitializeSectionAsync("ConnectionStrings");

                // Initialize SendGrid settings
                await InitializeSectionAsync("SendGrid");

                // Initialize AWS settings
                await InitializeSectionAsync("AWS:SES");

                // Initialize Braintree settings
                await InitializeSectionAsync("Braintree");
                await InitializeSectionAsync("MessageService");

                // Other general settings
                await InitializeSingleSettingAsync("UseLocalSqlite");
                await InitializeSingleSettingAsync("UseSqlServer");
                await InitializeSingleSettingAsync("BaseUrl");
                await InitializeSingleSettingAsync("DonationURL");
                await InitializeSingleSettingAsync("DefaultAdminAccount");

                _logger.LogInformation("Settings initialization completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing settings from appsettings");
            }
        }

        /// <summary>
        /// Initializes all settings in a specific section
        /// </summary>
        private async Task InitializeSectionAsync(string sectionPath)
        {
            _logger.LogInformation($"Initializing settings section: {sectionPath}");
            var section = _configuration.GetSection(sectionPath);

            if (!section.Exists())
            {
                _logger.LogWarning($"Section {sectionPath} not found in configuration");
                return;
            }

            foreach (var child in section.GetChildren())
            {
                string key = string.IsNullOrEmpty(sectionPath)
                    ? child.Key
                    : $"{sectionPath}:{child.Key}";

                string? value = child.Value;

                // If the child has children (is a section), recursively process it
                if (value == null && child.GetChildren().Any())
                {
                    await InitializeSectionAsync(key);
                }
                else
                {
                    await _settingsService.SetValueAsync(key, value);
                    _logger.LogInformation($"Initialized setting: {key}");
                }
            }
        }

        /// <summary>
        /// Initializes a single setting by key
        /// </summary>
        private async Task InitializeSingleSettingAsync(string key)
        {
            var value = _configuration[key];
            if (value != null)
            {
                await _settingsService.SetValueAsync(key, value);
                _logger.LogInformation($"Initialized setting: {key}");
            }
            else
            {
                _logger.LogWarning($"Setting {key} not found in configuration");
            }
        }
    }
}
