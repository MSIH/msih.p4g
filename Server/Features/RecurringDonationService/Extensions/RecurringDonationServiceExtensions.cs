/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

using Microsoft.EntityFrameworkCore;
using msih.p4g.Server.Features.RecurringDonationService.Data;
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
        public static IServiceCollection AddRecurringDonationServices(
            this IServiceCollection services,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            // Add DbContext
            var connectionString = configuration.GetConnectionString("DefaultConnection") ?? 
                                 throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            services.AddDbContext<RecurringDonationDbContext>(options =>
            {
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), mySqlOptions =>
                {
                    mySqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null);
                });

                if (environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
            });

            // Register repository
            services.AddScoped<IRecurringDonationRepository, RecurringDonationRepository>();

            // Register service
            services.AddScoped<IRecurringDonationService, Services.RecurringDonationService>();

            // Register background service
            services.AddHostedService<RecurringDonationProcessingService>();

            return services;
        }

        /// <summary>
        /// Ensures the recurring donation database is created and migrated.
        /// </summary>
        public static async Task EnsureRecurringDonationDatabaseAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<RecurringDonationDbContext>();
            
            try
            {
                await context.Database.EnsureCreatedAsync();
                // Note: In production, you might want to use proper migrations instead of EnsureCreated
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<RecurringDonationDbContext>>();
                logger.LogError(ex, "An error occurred while ensuring the recurring donation database");
                throw;
            }
        }
    }
}