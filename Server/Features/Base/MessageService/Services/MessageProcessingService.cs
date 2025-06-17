/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using msih.p4g.Server.Features.Base.MessageService.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace msih.p4g.Server.Features.Base.MessageService.Services
{
    /// <summary>
    /// Background service that processes scheduled messages and sends them when they are due
    /// </summary>
    public class MessageProcessingService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MessageProcessingService> _logger;
        private readonly TimeSpan _processingInterval = TimeSpan.FromMinutes(1);

        public MessageProcessingService(
            IServiceProvider serviceProvider,
            ILogger<MessageProcessingService> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Message processing service started at: {time}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessMessagesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing messages");
                }

                await Task.Delay(_processingInterval, stoppingToken);
            }

            _logger.LogInformation("Message processing service stopped at: {time}", DateTimeOffset.Now);
        }

        private async Task ProcessMessagesAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var messageService = scope.ServiceProvider.GetRequiredService<IMessageService>();

            try
            {
                _logger.LogDebug("Processing pending messages at {time}", DateTimeOffset.Now);
                int processedCount = await messageService.ProcessPendingMessagesAsync(50);
                
                if (processedCount > 0)
                {
                    _logger.LogInformation("Successfully processed {count} messages", processedCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing messages");
            }
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Message processing service is starting.");
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Message processing service is stopping.");
            return base.StopAsync(cancellationToken);
        }
    }
}
