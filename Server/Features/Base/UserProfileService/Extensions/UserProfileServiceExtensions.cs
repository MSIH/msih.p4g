/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

using msih.p4g.Server.Features.Base.UserProfileService.Interfaces;
using msih.p4g.Server.Features.Base.UserProfileService.Services;

namespace msih.p4g.Server.Features.Base.UserProfileService.Extensions
{
    /// <summary>
    /// Extension methods for registering user profile services with the dependency injection container.
    /// </summary>
    public static class UserProfileServiceExtensions
    {
        /// <summary>
        /// Adds user profile services for coordinating User and Profile operations to the DI container.
        /// </summary>
        /// <param name="services">The service collection to add services to</param>
        /// <returns>The service collection for method chaining</returns>
        public static IServiceCollection AddUserProfileServices(this IServiceCollection services)
        {
            // Register UserProfileService for coordinating User and Profile operations
            services.AddScoped<IUserProfileService, Services.UserProfileService>();

            return services;
        }
    }
}
