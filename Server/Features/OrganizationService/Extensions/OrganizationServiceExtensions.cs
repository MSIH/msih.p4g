/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

using msih.p4g.Server.Features.OrganizationService.Data;
using msih.p4g.Server.Features.OrganizationService.Interfaces;
using msih.p4g.Server.Features.OrganizationService.Repositories;
using msih.p4g.Server.Features.OrganizationService.Services;

namespace msih.p4g.Server.Features.OrganizationService.Extensions
{
    /// <summary>
    /// Extension methods for registering organization services with the dependency injection container.
    /// </summary>
    public static class OrganizationServiceExtensions
    {
        /// <summary>
        /// Adds organization services including repository, business logic, and data seeding services to the DI container.
        /// </summary>
        /// <param name="services">The service collection to add services to</param>
        /// <returns>The service collection for method chaining</returns>
        public static IServiceCollection AddOrganizationServices(this IServiceCollection services)
        {
            // Register OrganizationRepository and OrganizationService for DI
            services.AddSingleton<IOrganizationRepository, OrganizationRepository>();
            services.AddScoped<IOrganizationService, msih.p4g.Server.Features.OrganizationService.Services.OrganizationService>();

            // Register the organization data seeder
            services.AddScoped<OrganizationDataSeeder>();

            return services;
        }
    }
}
