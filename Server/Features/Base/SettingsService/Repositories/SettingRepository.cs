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
using Microsoft.EntityFrameworkCore;
using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Common.Interfaces;
using msih.p4g.Server.Features.Base.SettingsService.Interfaces;
using msih.p4g.Server.Features.Base.SettingsService.Model;
using System.Linq.Expressions;

namespace msih.p4g.Server.Features.Base.SettingsService.Repositories
{
    /// <summary>
    /// Repository implementation for Setting entity
    /// </summary>
    public class SettingRepository : GenericRepository<Setting>, ISettingRepository
    {
        /// <summary>
        /// Initializes a new instance of the SettingRepository class
        /// </summary>
        /// <param name="contextFactory">The database context factory</param>
        public SettingRepository(IDbContextFactory<ApplicationDbContext> contextFactory,
        ICacheStrategy cacheStrategy)
        : base(contextFactory, cacheStrategy)
        {
        }

        /// <summary>
        /// Gets a setting by its key
        /// </summary>
        /// <param name="key">The setting key</param>
        /// <returns>The setting if found, otherwise null</returns>
        public async Task<Setting?> GetByKeyAsync(string key)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<Setting>()
                .FirstOrDefaultAsync(s => s.Key == key && s.IsActive);
        }

        /// <summary>
        /// Overrides FindAsync to implement Setting-specific predicate parsing and cache key generation
        /// </summary>
        /// <param name="predicate">The filter expression</param>
        /// <param name="includeInactive">Whether to include inactive entities</param>
        /// <returns>Settings matching the predicate and criteria</returns>
        public override async Task<IEnumerable<Setting>> FindAsync(Expression<Func<Setting, bool>> predicate, bool includeInactive = false)
        {
            string? keyValue = null;

            // Special handling for Setting entity and s => s.Key == key
            if (predicate.Body is BinaryExpression binaryExpr)
            {
                if (binaryExpr.Left is MemberExpression memberExpr && memberExpr.Member.Name == "Key")
                {
                    object? value = null;
                    if (binaryExpr.Right is ConstantExpression constExpr)
                    {
                        value = constExpr.Value;
                    }
                    else if (binaryExpr.Right is MemberExpression rightMember)
                    {
                        // Handles closure variables (e.g., s => s.Key == key)
                        var objectMember = Expression.Convert(rightMember, typeof(object));
                        var getterLambda = Expression.Lambda<Func<object>>(objectMember);
                        value = getterLambda.Compile().Invoke();
                    }
                    keyValue = value?.ToString();
                }
            }

            string cacheKey;
            if (keyValue != null)
            {
                cacheKey = $"{_entityTypeName}:Find:Key:{keyValue}:IncludeInactive:{includeInactive}";
            }
            else
            {
                var predicateHash = predicate.ToString().GetHashCode().ToString();
                cacheKey = GetCacheKeyForFind(predicateHash, includeInactive);
            }

            if (_cacheStrategy != null)
            {
                var cachedEntities = await _cacheStrategy.GetAsync<List<Setting>>(cacheKey);
                if (cachedEntities != null)
                {
                    return cachedEntities;
                }
            }

            // Fetch from database
            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.Set<Setting>().Where(predicate);

            if (!includeInactive)
            {
                query = query.Where(e => e.IsActive);
            }

            var entities = await query.ToListAsync();

            // Cache the result if strategy is available
            if (_cacheStrategy != null)
            {
                await _cacheStrategy.SetAsync(cacheKey, entities);
            }

            return entities;
        }
    }
}
