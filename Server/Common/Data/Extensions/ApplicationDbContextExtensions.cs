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

namespace msih.p4g.Server.Common.Data.Extensions
{
    /// <summary>
    /// Extension methods to register and configure the unified ApplicationDbContext
    /// </summary>
    public static class ApplicationDbContextExtensions
    {
        /// <summary>
        /// Adds the unified ApplicationDbContext
        /// </summary>
        public static IServiceCollection AddUnifiedDbContext(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment hostEnvironment)
        {
            // Register the unified ApplicationDbContext
            DatabaseConfigurationHelper.AddConfiguredDbContext<ApplicationDbContext>(
                services, configuration, hostEnvironment);

            return services;
        }
    }
}
