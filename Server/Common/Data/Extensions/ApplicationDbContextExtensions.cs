/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Features.Base.PaymentService.Data;
using msih.p4g.Server.Features.Base.ProfileService.Data;
using msih.p4g.Server.Features.Base.SettingsService.Data;
using msih.p4g.Server.Features.Base.SmsService.Data;
using msih.p4g.Server.Features.Base.UserService.Data;
using msih.p4g.Server.Features.CampaignService.Data;
using msih.p4g.Server.Features.DonationService.Data;
using msih.p4g.Server.Features.DonorService.Data;

namespace msih.p4g.Server.Common.Data.Extensions
{
    /// <summary>
    /// Extension methods to register and configure the unified ApplicationDbContext
    /// </summary>
    public static class ApplicationDbContextExtensions
    {
        /// <summary>
        /// Adds the unified ApplicationDbContext
        /// </summary>
        public static IServiceCollection AddUnifiedDbContext(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment hostEnvironment)
        {
            // Register the unified ApplicationDbContext
            DatabaseConfigurationHelper.AddConfiguredDbContext<ApplicationDbContext>(
                services, configuration, hostEnvironment);

            return services;
        }
    }
}
