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
using MSIH.Core.Services.Message.Data;
using MSIH.Core.Services.Message.Interfaces;
using MSIH.Core.Services.Message.Repositories;
using MSIH.Core.Services.Message.Services;
using MessageEntity = MSIH.Core.Services.Message.Models.Message;
using MessageTemplate = MSIH.Core.Services.Message.Models.MessageTemplate;

namespace MSIH.Core.Services.Message.Extensions
{
    /// <summary>
    /// Extension methods for registering message service related services to the DI container
    /// </summary>
    public static class MessageServiceExtensions
    {
        /// <summary>
        /// Adds the message service and its dependencies to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <param name="hostEnvironment">The hosting environment</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddMessageServices(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment hostEnvironment)
        {
            // Register repositories
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<IMessageTemplateRepository, MessageTemplateRepository>();

            // Register generic repositories using the concrete implementations
            services.AddScoped<IGenericRepository<MessageEntity>>(provider => 
                (IGenericRepository<MessageEntity>)provider.GetRequiredService<IMessageRepository>());
            services.AddScoped<IGenericRepository<MessageTemplate>>(provider => 
                (IGenericRepository<MessageTemplate>)provider.GetRequiredService<IMessageTemplateRepository>());

            // Register the message service
            services.AddScoped<IMessageService, MessageService>();

            // Register migration applier as a hosted service
            services.AddHostedService<MigrationApplier>();
            
            // Register the background service for processing scheduled messages
            services.AddHostedService<MessageProcessingService>();

            return services;
        }
    }
}
