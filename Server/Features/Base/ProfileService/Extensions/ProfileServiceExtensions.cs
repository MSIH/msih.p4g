/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

using msih.p4g.Server.Features.Base.ProfileService.Interfaces;
using msih.p4g.Server.Features.Base.ProfileService.Repositories;
using msih.p4g.Server.Features.Base.ProfileService.Services;

namespace msih.p4g.Server.Features.Base.ProfileService.Extensions
{
    /// <summary>
    /// Extension methods for registering profile services with the dependency injection container.
    /// </summary>
    public static class ProfileServiceExtensions
    {
        /// <summary>
        /// Adds profile services including repository and business logic services to the DI container.
        /// </summary>
        /// <param name="services">The service collection to add services to</param>
        /// <returns>The service collection for method chaining</returns>
        public static IServiceCollection AddProfileServices(this IServiceCollection services)
        {
            // Register ProfileRepository and ProfileService for DI
            services.AddScoped<IProfileRepository, ProfileRepository>();
            services.AddScoped<IProfileService, Services.ProfileService>();

            return services;
        }
    }
}
