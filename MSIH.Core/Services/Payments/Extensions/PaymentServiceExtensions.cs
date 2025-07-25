/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MSIH.Core.Common.Data;
using MSIH.Core.Common.Data.Repositories;
using MSIH.Core.Services.Payments.Data;
using MSIH.Core.Services.Payments.Interfaces;
using MSIH.Core.Services.Payments.Models;
using MSIH.Core.Services.Payments.Repositories;
using MSIH.Core.Services.Payments.Services;

namespace MSIH.Core.Services.Payments.Extensions
{
    /// <summary>
    /// Extension methods for registering payment service components
    /// </summary>
    public static class PaymentServiceExtensions
    {
        /// <summary>
        /// Adds payment services to the dependency injection container
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <param name="hostEnvironment">The hosting environment</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddPaymentServices(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment hostEnvironment)
        {
            // Register repositories
            services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();

            // Register payment service implementations
            services.AddScoped<BraintreePaymentService>();

            // Register the payment service factory
            services.AddScoped<IPaymentServiceFactory, PaymentServiceFactory>();

            // Register the concrete repository for generic use (PaymentTransactionRepository already inherits from GenericRepository)
            services.AddScoped<IGenericRepository<PaymentTransaction>>(provider =>
                (IGenericRepository<PaymentTransaction>)provider.GetRequiredService<IPaymentTransactionRepository>());

            // Register migration applier as a hosted service
            services.AddHostedService<MigrationApplier>();

            return services;
        }
    }
}
