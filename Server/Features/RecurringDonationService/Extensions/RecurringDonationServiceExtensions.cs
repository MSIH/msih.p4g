/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

using msih.p4g.Server.Features.RecurringDonationService.Interfaces;
using msih.p4g.Server.Features.RecurringDonationService.Repositories;
using msih.p4g.Server.Features.RecurringDonationService.Services;

namespace msih.p4g.Server.Features.RecurringDonationService.Extensions
{
    /// <summary>
    /// Extension methods for registering recurring donation services.
    /// </summary>
    public static class RecurringDonationServiceExtensions
    {
        /// <summary>
        /// Adds recurring donation services to the service collection.
        /// </summary>
        public static IServiceCollection AddRecurringDonationServices(this IServiceCollection services)
        {
            // Register repository
            services.AddScoped<IRecurringDonationRepository, RecurringDonationRepository>();

            // Register service
            services.AddScoped<IRecurringDonationService, Services.RecurringDonationService>();

            // Register background service
            services.AddHostedService<RecurringDonationProcessingService>();

            return services;
        }
    }
}