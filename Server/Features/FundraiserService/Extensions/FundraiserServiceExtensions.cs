/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

using msih.p4g.Server.Features.FundraiserService.Interfaces;
using msih.p4g.Server.Features.FundraiserService.Repositories;
using msih.p4g.Server.Features.FundraiserService.Services;

namespace msih.p4g.Server.Features.FundraiserService.Extensions
{
    /// <summary>
    /// Extension methods for registering fundraiser services with the dependency injection container.
    /// </summary>
    public static class FundraiserServiceExtensions
    {
        /// <summary>
        /// Adds fundraiser services including repository, business logic, and statistics services to the DI container.
        /// </summary>
        /// <param name="services">The service collection to add services to</param>
        /// <returns>The service collection for method chaining</returns>
        public static IServiceCollection AddFundraiserServices(this IServiceCollection services)
        {
            // Register FundraiserRepository and FundraiserService for DI
            services.AddScoped<IFundraiserRepository, FundraiserRepository>();
            services.AddScoped<IFundraiserService, msih.p4g.Server.Features.FundraiserService.Services.FundraiserService>();

            // Register FundraiserStatisticsRepository and FundraiserStatisticsService for DI
            services.AddScoped<IFundraiserStatisticsRepository, FundraiserStatisticsRepository>();
            services.AddScoped<IFundraiserStatisticsService, FundraiserStatisticsService>();

            return services;
        }
    }
}
