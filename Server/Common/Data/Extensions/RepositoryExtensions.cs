/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.EntityFrameworkCore;
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Common.Models;

namespace msih.p4g.Server.Common.Data.Extensions
{
    /// <summary>
    /// Extension methods for registering repositories in the service collection
    /// </summary>
    /// <remarks>
    /// This class is currently not being used in the project. It is kept for potential future use.
    /// Generic repositories are registered directly using services.AddScoped in Program.cs and service extension classes.
    /// </remarks>
    public static class RepositoryExtensions
    {
        /// <summary>
        /// Adds a generic repository for the specified entity type
        /// </summary>
        /// <typeparam name="TEntity">The entity type</typeparam>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection</returns>
        /// <remarks>
        /// This method is currently not being used in the project. It is kept for potential future use.
        /// </remarks>
        public static IServiceCollection AddGenericRepository<TEntity>(this IServiceCollection services)
            where TEntity : class
        {
            services.AddScoped<IGenericRepository<TEntity>, GenericRepository<TEntity>>();
            return services;
        }
    }
}
