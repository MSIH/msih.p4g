// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

using msih.p4g.Server.Features.Base.AffiliateMonitoringService.Interfaces;
using msih.p4g.Server.Features.Base.AffiliateMonitoringService.Services;

namespace msih.p4g.Server.Features.Base.AffiliateMonitoringService.Extensions
{
    /// <summary>
    /// Extension methods for configuring affiliate monitoring services
    /// </summary>
    public static class AffiliateMonitoringServiceExtensions
    {
        /// <summary>
        /// Adds affiliate monitoring services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddAffiliateMonitoringServices(this IServiceCollection services)
        {
            // Register the affiliate monitoring service
            services.AddScoped<IAffiliateMonitoringService, Services.AffiliateMonitoringService>();

            return services;
        }
    }
}