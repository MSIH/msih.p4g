// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

using msih.p4g.Server.Features.CampaignService.Data;
using msih.p4g.Server.Features.CampaignService.Interfaces;
using msih.p4g.Server.Features.CampaignService.Repositories;
using msih.p4g.Server.Features.CampaignService.Services;

namespace msih.p4g.Server.Features.CampaignService.Extensions
{
    /// <summary>
    /// Extension methods for registering CampaignService and related dependencies
    /// </summary>
    public static class CampaignServiceExtensions
    {
        /// <summary>
        /// Adds CampaignService and related dependencies to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddCampaignServices(this IServiceCollection services)
        {
            // Register repository and service for DI
            services.AddSingleton<ICampaignRepository, CampaignRepository>();
            services.AddScoped<ICampaignService, Services.CampaignService>();

            // Register data seeder
            services.AddScoped<CampaignDataSeeder>();

            return services;
        }
    }
}
