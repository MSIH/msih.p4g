using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using msih.p4g.Server.Common.Data.Extensions;
using msih.p4g.Server.Features.Base.SmsService.Data;
using msih.p4g.Server.Features.Base.SmsService.Interfaces;
using msih.p4g.Server.Features.Base.SmsService.Services;
using msih.p4g.Shared.Models;
using System;

namespace msih.p4g.Server.Features.Base.SmsService.Extensions
{
    /// <summary>
    /// Extension methods for registering SMS service related services to the DI container
    /// </summary>
    public static class SmsServiceExtensions
    {
        /// <summary>
        /// Adds the SMS service and its dependencies to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddSmsServices(this IServiceCollection services, IConfiguration configuration)
        {            
            // Add SMS DbContext with the appropriate provider
            var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
            DatabaseConfigurationHelper.AddConfiguredDbContext<SmsDbContext>(
                services, 
                configuration, 
                isDevelopment);
            
            // Register services
            services.AddScoped<ISmsService, TwilioSmsService>();
            services.AddScoped<IValidatedPhoneNumberRepository, ValidatedPhoneNumberRepository>();
            services.AddScoped<PhoneValidationService>();
            
            // Register generic repositories
            services.AddGenericRepository<ValidatedPhoneNumber, SmsDbContext>();
            
            // Register migration applier as a hosted service
            services.AddHostedService<MigrationApplier>();

            return services;
        }
    }
}
