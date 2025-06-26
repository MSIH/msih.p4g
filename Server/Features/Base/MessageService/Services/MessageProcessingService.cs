// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using msih.p4g.Server.Features.Base.MessageService.Interfaces;
using msih.p4g.Server.Features.Base.SettingsService.Interfaces;

namespace msih.p4g.Server.Features.Base.MessageService.Services
{
    /// <summary>
    /// Background service that processes scheduled messages and retries failed messages with different intervals
    /// </summary>
    public class MessageProcessingService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MessageProcessingService> _logger;
        private readonly ISettingsService _settingsService;
        private TimeSpan _scheduledMessageInterval;
        private TimeSpan _failedMessageRetryInterval;
        private DateTime _lastFailedMessageRetryTime;

        public MessageProcessingService(
                IServiceProvider serviceProvider,
                ILogger<MessageProcessingService> logger,
                ISettingsService settingsService)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _lastFailedMessageRetryTime = DateTime.UtcNow;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await LoadIntervalSettingsAsync();

            _logger.LogInformation("Message processing service started at: {time} with scheduled interval: {scheduledInterval} and retry interval: {retryInterval}",
                DateTimeOffset.Now, _scheduledMessageInterval, _failedMessageRetryInterval);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Always process scheduled messages (every 5 minutes by default)
                    await ProcessScheduledMessagesAsync(stoppingToken);

                    // Check if it's time to retry failed messages (every 8 hours by default)
                    if (DateTime.UtcNow - _lastFailedMessageRetryTime >= _failedMessageRetryInterval)
                    {
                        await ProcessFailedMessagesAsync(stoppingToken);
                        _lastFailedMessageRetryTime = DateTime.UtcNow;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing messages");
                }

                // Wait for the shorter interval (scheduled messages)
                await Task.Delay(_scheduledMessageInterval, stoppingToken);
            }

            _logger.LogInformation("Message processing service stopped at: {time}", DateTimeOffset.Now);
        }

        /// <summary>
        /// Loads interval settings from the settings service
        /// </summary>
        private async Task LoadIntervalSettingsAsync()
        {
            try
            {
                // Load scheduled message interval (default: 5 minutes)
                const string scheduledIntervalKey = "MessageService:ScheduledProcessingIntervalMinutes";
                const int defaultScheduledIntervalMinutes = 5;

                var scheduledIntervalSetting = await _settingsService.GetValueAsync(scheduledIntervalKey);
                if (int.TryParse(scheduledIntervalSetting, out int scheduledIntervalMinutes) && scheduledIntervalMinutes > 0)
                {
                    _scheduledMessageInterval = TimeSpan.FromMinutes(scheduledIntervalMinutes);
                    _logger.LogInformation("Using configured scheduled message processing interval: {minutes} minutes", scheduledIntervalMinutes);
                }
                else
                {
                    _scheduledMessageInterval = TimeSpan.FromMinutes(defaultScheduledIntervalMinutes);
                    _logger.LogWarning("Invalid or missing scheduled message processing interval setting. Using default: {minutes} minutes", defaultScheduledIntervalMinutes);
                    await _settingsService.SetValueAsync(scheduledIntervalKey, defaultScheduledIntervalMinutes.ToString(), "MessageProcessingService");
                }

                // Load failed message retry interval (default: 8 hours)
                const string retryIntervalKey = "MessageService:FailedMessageRetryIntervalMinutes";
                const int defaultRetryIntervalMinutes = 480;

                var retryIntervalSetting = await _settingsService.GetValueAsync(retryIntervalKey);
                if (int.TryParse(retryIntervalSetting, out int retryIntervalMinutes) && retryIntervalMinutes > 0)
                {
                    _failedMessageRetryInterval = TimeSpan.FromMinutes(retryIntervalMinutes);
                    _logger.LogInformation("Using configured failed message retry interval: {minutes} minutes", retryIntervalMinutes);
                }
                else
                {
                    _failedMessageRetryInterval = TimeSpan.FromMinutes(defaultRetryIntervalMinutes);
                    _logger.LogWarning("Invalid or missing failed message retry interval setting. Using default: {minutes} minutes", defaultRetryIntervalMinutes);
                    await _settingsService.SetValueAsync(retryIntervalKey, defaultRetryIntervalMinutes.ToString(), "MessageProcessingService");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading processing intervals from settings. Using defaults: 5 minutes for scheduled, 480 minutes for retries.");
                _scheduledMessageInterval = TimeSpan.FromMinutes(5);
                _failedMessageRetryInterval = TimeSpan.FromMinutes(480);
            }
        }

        /// <summary>
        /// Processes scheduled messages that are due to be sent
        /// </summary>
        private async Task ProcessScheduledMessagesAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var messageService = scope.ServiceProvider.GetRequiredService<IMessageService>();

            try
            {
                _logger.LogDebug("Processing scheduled messages at {time}", DateTimeOffset.Now);
                int processedCount = await messageService.ProcessScheduledMessagesAsync(50);

                if (processedCount > 0)
                {
                    _logger.LogInformation("Successfully processed {count} scheduled messages", processedCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing scheduled messages");
            }
        }

        /// <summary>
        /// Processes failed messages for retry
        /// </summary>
        private async Task ProcessFailedMessagesAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var messageService = scope.ServiceProvider.GetRequiredService<IMessageService>();

            try
            {
                _logger.LogDebug("Processing failed messages for retry at {time}", DateTimeOffset.Now);
                int processedCount = await messageService.ProcessFailedMessagesAsync(25);

                if (processedCount > 0)
                {
                    _logger.LogInformation("Successfully processed {count} failed messages for retry", processedCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing failed messages");
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
