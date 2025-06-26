/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using msih.p4g.Server.Features.Base.EmailService.Interfaces;
using msih.p4g.Server.Features.Base.EmailService.Services;

namespace msih.p4g.Server.Features.Base.EmailService.Extensions
{
    public static class EmailServiceExtensions
    {
        /// <summary>
        /// Adds email services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddEmailServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register the email service based on configuration
            string emailProvider = configuration["EmailProvider"] ?? "SendGrid";
            
            switch (emailProvider.ToLowerInvariant())
            {
                case "aws":
                case "awsses":
                    services.AddScoped<IEmailService, AWSSESEmailService>();
                    break;
                
                case "smtp2go":
                    services.AddScoped<IEmailService, Smtp2GoEmailService>();
                    break;
                
                case "sendgrid":
                default:
                    services.AddScoped<IEmailService, SendGridEmailService>();
                    break;
            }

            return services;
        }
    }
}
