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
using msih.p4g.Server.Features.OrganizationService.Interfaces;
using msih.p4g.Server.Features.OrganizationService.Repositories;

namespace msih.p4g.Server.Features.OrganizationService
{
    /// <summary>
    /// Instructions for registering OrganizationService directly in Program.cs
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add the following code to Program.cs to register the Organization service
        /// and its dependencies directly:
        /// 
        /// // Register OrganizationRepository and OrganizationService for DI
        /// builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        /// builder.Services.AddScoped<IOrganizationService, OrganizationService>();
        /// </summary>
        /// <remarks>
        /// This class exists only for reference. Add the code directly to Program.cs
        /// instead of using an extension method.
        /// Do used at this time but reference for future development.
        /// </remarks>
        public static IServiceCollection AddOrganizationService(this IServiceCollection services)
        {
            // Register repository and service for DI
            services.AddScoped<IOrganizationRepository, OrganizationRepository>();
            services.AddScoped<IOrganizationService, Services.OrganizationService>(); // Fixed: Fully qualified name used to resolve ambiguity

            return services;
        }
    }
}
