/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

using msih.p4g.Server.Features.Base.W9FormService.Interfaces;
using msih.p4g.Server.Features.Base.W9FormService.Services;

namespace msih.p4g.Server.Features.Base.W9FormService.Extensions
{
    /// <summary>
    /// Extension methods for registering W9 form services with the dependency injection container.
    /// </summary>
    public static class W9FormServiceExtensions
    {
        /// <summary>
        /// Adds W9 form services for tax form processing and management to the DI container.
        /// </summary>
        /// <param name="services">The service collection to add services to</param>
        /// <returns>The service collection for method chaining</returns>
        public static IServiceCollection AddW9FormServices(this IServiceCollection services)
        {
            // Register W9FormService for DI
            services.AddScoped<IW9FormService, msih.p4g.Server.Features.Base.W9FormService.Services.W9FormService>();

            return services;
        }
    }
}
