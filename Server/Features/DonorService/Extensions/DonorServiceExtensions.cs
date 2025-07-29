/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

using msih.p4g.Server.Features.DonorService.Interfaces;
using msih.p4g.Server.Features.DonorService.Repositories;
using msih.p4g.Server.Features.DonorService.Services;

namespace msih.p4g.Server.Features.DonorService.Extensions
{
    /// <summary>
    /// Extension methods for registering donor services with the dependency injection container.
    /// </summary>
    public static class DonorServiceExtensions
    {
        /// <summary>
        /// Adds donor services including repository and business logic services to the DI container.
        /// </summary>
        /// <param name="services">The service collection to add services to</param>
        /// <returns>The service collection for method chaining</returns>
        public static IServiceCollection AddDonorServices(this IServiceCollection services)
        {
            // Register DonorRepository and DonorService for DI
            services.AddScoped<IDonorRepository, DonorRepository>();
            services.AddScoped<IDonorService, Services.DonorService>();

            return services;
        }
    }
}
