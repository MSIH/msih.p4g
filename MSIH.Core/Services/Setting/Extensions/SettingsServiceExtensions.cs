/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.Extensions.DependencyInjection;
using MSIH.Core.Services.Setting.Interfaces;
using MSIH.Core.Services.Setting.Repositories;
using MSIH.Core.Services.Setting.Services;

namespace MSIH.Core.Services.Setting.Extensions
{
    /// <summary>
    /// Extension methods for setting up settings services
    /// </summary>
    public static class SettingsServiceExtensions
    {
        /// <summary>
        /// Adds settings services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddSettingsServices(this IServiceCollection services)
        {
            // Register SettingRepository for DI
            services.AddSingleton<ISettingRepository, SettingRepository>();

            // Register SettingsService for DI
            services.AddScoped<ISettingsService, SettingsService>();

            // Register SettingsInitializer for DI
            services.AddScoped<SettingsInitializer>();

            return services;
        }
    }
}