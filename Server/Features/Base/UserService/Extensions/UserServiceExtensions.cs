/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

using msih.p4g.Server.Common.Utilities;
using msih.p4g.Server.Features.Base.UserService.Interfaces;
using msih.p4g.Server.Features.Base.UserService.Repositories;
using msih.p4g.Server.Features.Base.UserService.Services;

namespace msih.p4g.Server.Features.Base.UserService.Extensions
{
    /// <summary>
    /// Extension methods for registering user services with the dependency injection container.
    /// </summary>
    public static class UserServiceExtensions
    {
        /// <summary>
        /// Adds user services including repository, business logic, email verification, and admin initialization services to the DI container.
        /// </summary>
        /// <param name="services">The service collection to add services to</param>
        /// <returns>The service collection for method chaining</returns>
        public static IServiceCollection AddUserServices(this IServiceCollection services)
        {
            // Register UserRepository and UserService for DI
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, Services.UserService>();

            // Register email verification service
            services.AddScoped<IEmailVerificationService, EmailVerificationService>();

            // Register admin initialization service
            services.AddScoped<AdminInitializationService>();

            // Register referral link helper utility
            services.AddScoped<ReferralLinkHelper>();

            return services;
        }
    }
}
