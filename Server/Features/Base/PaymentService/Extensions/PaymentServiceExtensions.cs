/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.Base.PaymentService.Data;
using msih.p4g.Server.Features.Base.PaymentService.Interfaces;
using msih.p4g.Server.Features.Base.PaymentService.Models;
using msih.p4g.Server.Features.Base.PaymentService.Repositories;
using msih.p4g.Server.Features.Base.PaymentService.Services;

namespace msih.p4g.Server.Features.Base.PaymentService.Extensions
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
            
            // Register generic repositories
            services.AddScoped<IGenericRepository<PaymentTransaction>, GenericRepository<PaymentTransaction, ApplicationDbContext>>();
            
            // Register migration applier as a hosted service
            services.AddHostedService<MigrationApplier>();
            
            return services;
        }
    }
}
