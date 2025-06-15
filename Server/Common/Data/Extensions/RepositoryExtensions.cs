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
    public static class RepositoryExtensions
    {
        /// <summary>
        /// Adds a generic repository for the specified entity type
        /// </summary>
        /// <typeparam name="TEntity">The entity type</typeparam>
        /// <typeparam name="TContext">The DbContext type</typeparam>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddGenericRepository<TEntity, TContext>(this IServiceCollection services)
            where TEntity : BaseEntity
            where TContext : DbContext
        {
            services.AddScoped<IGenericRepository<TEntity>, GenericRepository<TEntity, TContext>>();
            return services;
        }
    }
}
