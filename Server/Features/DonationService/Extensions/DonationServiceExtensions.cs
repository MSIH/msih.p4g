/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

using msih.p4g.Server.Features.DonationService.Interfaces;
using msih.p4g.Server.Features.DonationService.Repositories;


namespace msih.p4g.Server.Features.DonationService.Extensions
{
    /// <summary>
    /// Extension methods for registering donation services.
    /// </summary>
    public static class DonationServiceExtensions
    {
        /// <summary>
        /// Adds all donation services to the DI container.
        /// </summary>
        public static IServiceCollection AddDonationServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            // Register repository and service
            services.AddScoped<IDonationRepository, DonationRepository>();
            services.AddScoped<IDonationService, Services.DonationService>();

            // Register background service for automatic recurring donation processing
            services.AddHostedService<Services.RecurringDonationProcessingService>();

            return services;
        }

        /// <summary>
        /// Adds the recurring donation background processing service.
        /// </summary>
        public static IServiceCollection AddRecurringDonationProcessing(this IServiceCollection services)
        {
            // Register background service for automatic recurring donation processing
            services.AddHostedService<Services.RecurringDonationProcessingService>();

            return services;
        }
    }
}
