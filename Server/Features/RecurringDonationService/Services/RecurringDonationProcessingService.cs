/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

using msih.p4g.Server.Features.RecurringDonationService.Interfaces;

namespace msih.p4g.Server.Features.RecurringDonationService.Services
{
    /// <summary>
    /// Background service that processes recurring donations automatically.
    /// </summary>
    public class RecurringDonationProcessingService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RecurringDonationProcessingService> _logger;
        private readonly TimeSpan _processInterval = TimeSpan.FromHours(1); // Process every hour

        public RecurringDonationProcessingService(
            IServiceProvider serviceProvider,
            ILogger<RecurringDonationProcessingService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Recurring Donation Processing Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessRecurringDonationsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing recurring donations: {ErrorMessage}", ex.Message);
                }

                // Wait for the next processing interval
                try
                {
                    await Task.Delay(_processInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected when the service is being stopped
                    break;
                }
            }

            _logger.LogInformation("Recurring Donation Processing Service stopped");
        }

        private async Task ProcessRecurringDonationsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var recurringDonationService = scope.ServiceProvider.GetRequiredService<IRecurringDonationService>();

            try
            {
                _logger.LogDebug("Starting recurring donation processing cycle");

                var processedCount = await recurringDonationService.ProcessDueRecurringDonationsAsync();

                if (processedCount > 0)
                {
                    _logger.LogInformation("Processed {ProcessedCount} recurring donations successfully", processedCount);
                }
                else
                {
                    _logger.LogDebug("No recurring donations were due for processing");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing recurring donations: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Stopping Recurring Donation Processing Service...");
            await base.StopAsync(stoppingToken);
        }
    }
}