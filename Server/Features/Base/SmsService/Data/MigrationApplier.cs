/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using msih.p4g.Server.Common.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace msih.p4g.Server.Features.Base.SmsService.Data
{
    /// <summary>
    /// Applies migrations for the SMS service during application startup
    /// </summary>
    public class MigrationApplier : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MigrationApplier> _logger;

        public MigrationApplier(
            IServiceProvider serviceProvider,
            ILogger<MigrationApplier> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Applying SMS database migrations...");
            
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            try
            {
                await dbContext.Database.MigrateAsync(cancellationToken);
                _logger.LogInformation("SMS database migrations applied successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while applying SMS database migrations");
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
