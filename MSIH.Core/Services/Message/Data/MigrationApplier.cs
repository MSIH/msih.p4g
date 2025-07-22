/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MSIH.Core.Common.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MSIH.Core.Services.Message.Data
{
    /// <summary>
    /// Applies migrations for the message service during application startup
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
            _logger.LogInformation("Applying message database migrations...");
            
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            try
            {
                await dbContext.Database.MigrateAsync(cancellationToken);
                _logger.LogInformation("Message database migrations applied successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while applying message database migrations");
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
