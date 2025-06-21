/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using msih.p4g.Server.Features.Base.PayoutService.Interfaces;
using msih.p4g.Server.Features.Base.PayoutService.Models.Configuration;
using msih.p4g.Server.Features.Base.PayoutService.Services;
using System;

namespace msih.p4g.Server.Features.Base.PayoutService.Extensions
{
    /// <summary>
    /// Extension methods to add PayPal payout services to the DI container
    /// </summary>
    public static class PayoutServiceExtensions
    {
        /// <summary>
        /// Add PayPal payout services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <param name="environment">The hosting environment</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddPayoutServices(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            // Register configuration
            services.Configure<PayPalOptions>(configuration.GetSection(PayPalOptions.SectionName));

            // Register PayPal API client with HttpClient
            services.AddHttpClient<IPayPalApiClient, PayPalApiClient>();

            // Register repositories
            services.AddScoped<IPayoutRepository, PayoutRepository>();

            // Register services
            services.AddScoped<IPayoutService, PayPalPayoutService>();

            return services;
        }
    }
}
