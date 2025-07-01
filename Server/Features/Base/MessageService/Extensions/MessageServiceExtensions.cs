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
using msih.p4g.Server.Features.Base.MessageService.Data;
using msih.p4g.Server.Features.Base.MessageService.Interfaces;
using msih.p4g.Server.Features.Base.MessageService.Models;
using msih.p4g.Server.Features.Base.MessageService.Repositories;
using msih.p4g.Server.Features.Base.MessageService.Services;

namespace msih.p4g.Server.Features.Base.MessageService.Extensions
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
            services.AddScoped<IGenericRepository<Message>>(provider => 
                (IGenericRepository<Message>)provider.GetRequiredService<IMessageRepository>());
            services.AddScoped<IGenericRepository<MessageTemplate>>(provider => 
                (IGenericRepository<MessageTemplate>)provider.GetRequiredService<IMessageTemplateRepository>());

            // Register the message service
            services.AddScoped<IMessageService, MessageService.Services.MessageService>();

            // Register migration applier as a hosted service
            services.AddHostedService<MigrationApplier>();
            
            // Register the background service for processing scheduled messages
            services.AddHostedService<MessageProcessingService>();

            return services;
        }
    }
}
