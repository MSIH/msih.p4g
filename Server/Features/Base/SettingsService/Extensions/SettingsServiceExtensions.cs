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
using msih.p4g.Server.Features.Base.SettingsService.Repositories;
using msih.p4g.Server.Features.Base.SettingsService.Services;

namespace msih.p4g.Server.Features.Base.SettingsService.Extensions
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
            services.AddScoped<ISettingsService, Services.SettingsService>();

            // Register SettingsInitializer for DI
            services.AddScoped<SettingsInitializer>();

            return services;
        }
    }
}
