/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using msih.p4g.Server.Common.Data.Extensions;
using System;
using System.IO;

namespace msih.p4g.Server.Features.Base.PaymentService.Data
{
    /// <summary>
    /// Design-time factory for PaymentDbContext to support EF Core migrations
    /// </summary>
    public class PaymentDbContextFactory : IDesignTimeDbContextFactory<PaymentDbContext>
    {
        /// <summary>
        /// Creates a new instance of PaymentDbContext for design-time operations
        /// </summary>
        public PaymentDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
                .Build();
                
            var optionsBuilder = new DbContextOptionsBuilder<PaymentDbContext>();
            
            // Configure DbContext options based on environment
            DatabaseConfigurationHelper.ConfigureDbContextOptions(
                optionsBuilder,
                configuration,
                isDevelopment: Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development");
                
            return new PaymentDbContext(optionsBuilder.Options);
        }
    }
}