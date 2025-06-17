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

namespace msih.p4g.Server.Common.Data
{
    /// <summary>
    /// Design-time factory for ApplicationDbContext to support EF Core migrations
    /// </summary>
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            DatabaseConfigurationHelper.ConfigureDbContextOptions(
                optionsBuilder,
                configuration,
                isDevelopment: Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
