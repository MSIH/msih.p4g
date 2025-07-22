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

using Microsoft.Extensions.Logging;
using MSIH.Core.Services.Setting.Interfaces;
using MSIH.Core.Services.User.Interfaces;
using MSIH.Core.Services.User.Models;

namespace MSIH.Core.Services.User.Services
{
    /// <summary>
    /// Configuration options for admin initialization
    /// </summary>
    public class AdminInitializationOptions
    {
        public string? DefaultAdminAccount { get; set; }
    }

    /// <summary>
    /// Service responsible for initializing admin accounts on application startup
    /// </summary>
    public class AdminInitializationService
    {
        private readonly IUserService _userService;
        private readonly ISettingsService _settingsService;
        private readonly ILogger<AdminInitializationService> _logger;

        public AdminInitializationService(
            IUserService userService,
            ISettingsService settingsService,
            ILogger<AdminInitializationService> logger)
        {
            _userService = userService;
            _settingsService = settingsService;
            _logger = logger;
        }

        /// <summary>
        /// Initializes the default admin account based on configuration
        /// </summary>
        public async Task InitializeDefaultAdminAsync()
        {
            try
            {
                var defaultAdminEmail = await _settingsService.GetValueAsync("DefaultAdminAccount");

                if (string.IsNullOrWhiteSpace(defaultAdminEmail))
                {
                    _logger.LogInformation("No DefaultAdminAccount configured in settings");
                    return;
                }

                _logger.LogInformation("Checking for default admin account: {Email}", defaultAdminEmail);

                // Check if the user exists
                var existingUser = await _userService.GetByEmailAsync(defaultAdminEmail);

                if (existingUser == null)
                {
                    _logger.LogInformation("User {Email} does not exist, skipping admin role assignment", defaultAdminEmail);
                    return;
                }

                // Check if user is already an admin
                if (existingUser.Role == UserRole.Admin)
                {
                    _logger.LogInformation("User {Email} is already an admin", defaultAdminEmail);
                    return;
                }

                // Promote user to admin
                existingUser.ChangeRole(UserRole.Admin);
                await _userService.UpdateAsync(existingUser, "AdminInitializationService");

                _logger.LogInformation("Successfully promoted user {Email} to admin role", defaultAdminEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while initializing default admin account");
                // Don't throw the exception to prevent application startup failure
            }
        }
    }
}
