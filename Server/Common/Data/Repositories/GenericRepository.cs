// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

using Microsoft.EntityFrameworkCore;
using msih.p4g.Server.Common.Interfaces;
using msih.p4g.Server.Common.Models;
using System.Linq.Expressions;

namespace msih.p4g.Server.Common.Data.Repositories
{
    /// <summary>
    /// Base repository implementation using DbContextFactory for thread-safe operations
    /// with optional caching strategy support
    /// </summary>
    public abstract class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        protected readonly ICacheStrategy? _cacheStrategy;
        protected readonly string _entityTypeName;

        /// <summary>
        /// Initializes a new instance of GenericRepository without caching
        /// </summary>
        /// <param name="contextFactory">The database context factory</param>
        public GenericRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
            : this(contextFactory, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of GenericRepository with optional caching strategy
        /// </summary>
        /// <param name="contextFactory">The database context factory</param>
        /// <param name="cacheStrategy">Optional caching strategy for repository operations</param>
        public GenericRepository(IDbContextFactory<ApplicationDbContext> contextFactory, ICacheStrategy? cacheStrategy = null)
        {
        }

        /// <summary>
        /// Initializes a new instance of GenericRepository with optional caching strategy
        /// </summary>
        /// <param name="contextFactory">The database context factory</param>
        /// <param name="cacheStrategy">Optional caching strategy for repository operations</param>
        public GenericRepository(IDbContextFactory<ApplicationDbContext> contextFactory, ICacheStrategy? cacheStrategy = null)
        {
            _contextFactory = contextFactory;
            _cacheStrategy = cacheStrategy;
            _entityTypeName = typeof(T).Name;
        }

        /// <summary>
        /// Generates a cache key for entity by ID operations
        /// </summary>
        /// <param name="id">The entity ID</param>
        /// <param name="includeInactive">Whether inactive entities are included</param>
        /// <returns>The cache key</returns>
        protected virtual string GetCacheKeyById(int id, bool includeInactive = false)
        {
            return $"{_entityTypeName}:Id:{id}:IncludeInactive:{includeInactive}";
        }

        /// <summary>
        /// Generates a cache key for GetAll operations
        /// </summary>
        /// <param name="includeInactive">Whether inactive entities are included</param>
        /// <returns>The cache key</returns>
        protected virtual string GetCacheKeyForAll(bool includeInactive = false)
        {
            return $"{_entityTypeName}:All:IncludeInactive:{includeInactive}";
        }

        /// <summary>
        /// Generates a cache key for Find operations
        /// </summary>
        /// <param name="predicateHash">Hash of the predicate expression</param>
        /// <param name="includeInactive">Whether inactive entities are included</param>
        /// <returns>The cache key</returns>
        protected virtual string GetCacheKeyForFind(string predicateHash, bool includeInactive = false)
        {
            return $"{_entityTypeName}:Find:{predicateHash}:IncludeInactive:{includeInactive}";
        }

        /// <summary>
        /// Generates a cache key for active-only operations
        /// </summary>
        /// <returns>The cache key</returns>
        protected virtual string GetCacheKeyForActiveOnly()
        {
            return $"{_entityTypeName}:ActiveOnly";
        }

        /// <summary>
        /// Generates a cache key for inactive-only operations
        /// </summary>
        /// <returns>The cache key</returns>
        protected virtual string GetCacheKeyForInactiveOnly()
        {
            return $"{_entityTypeName}:InactiveOnly";
        }

        /// <summary>
        /// Invalidates all cache entries for this entity type
        /// </summary>
        /// <returns>Task representing the async operation</returns>
        protected virtual async Task InvalidateAllCacheAsync()
        {
            if (_cacheStrategy != null)
            {
                await _cacheStrategy.RemoveByPatternAsync($"{_entityTypeName}:*");
            }
        }

        /// <summary>
        /// Invalidates cache entries for a specific entity ID
        /// </summary>
        /// <param name="id">The entity ID</param>
        /// <returns>Task representing the async operation</returns>
        protected virtual async Task InvalidateEntityCacheAsync(int id)
        {
            if (_cacheStrategy != null)
            {
                await _cacheStrategy.RemoveByPatternAsync($"{_entityTypeName}:Id:{id}:*");
            }
        }

        /// <summary>
        /// Invalidates collection-level cache entries (GetAll, ActiveOnly, InactiveOnly)
        /// </summary>
        /// <returns>Task representing the async operation</returns>
        protected virtual async Task InvalidateCollectionCacheAsync()
        {
            if (_cacheStrategy != null)
            {
                await _cacheStrategy.RemoveByPatternAsync($"{_entityTypeName}:All:*");
                await _cacheStrategy.RemoveByPatternAsync($"{_entityTypeName}:ActiveOnly");
                await _cacheStrategy.RemoveByPatternAsync($"{_entityTypeName}:InactiveOnly");
                await _cacheStrategy.RemoveByPatternAsync($"{_entityTypeName}:Find:*");
            }
        }

        public virtual async Task<T?> GetByIdAsync(int id, bool includeInactive = false)
        {
            // Try cache first if strategy is available
            if (_cacheStrategy != null)
            {
                var cacheKey = GetCacheKeyById(id, includeInactive);
                var cachedEntity = await _cacheStrategy.GetAsync<T>(cacheKey);
                if (cachedEntity != null)
                {
                    return cachedEntity;
                }
            }

            // Fetch from database
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Set<T>().FindAsync(id);

            if (entity != null && entity is IAuditableEntity auditableEntity && !includeInactive && !auditableEntity.IsActive)
            {
                return null;
            }

            // Cache the result if strategy is available and entity is found
            if (_cacheStrategy != null && entity != null)
            {
                var cacheKey = GetCacheKeyById(id, includeInactive);
                await _cacheStrategy.SetAsync(cacheKey, entity);
            }

            return entity;
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(bool includeInactive = false)
        {
            // Try cache first if strategy is available
            if (_cacheStrategy != null)
            {
                var cacheKey = GetCacheKeyForAll(includeInactive);
                var cachedEntities = await _cacheStrategy.GetAsync<List<T>>(cacheKey);
                if (cachedEntities != null)
                {
                    return cachedEntities;
                }
            }

            // Fetch from database
            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.Set<T>().AsQueryable();

            if (!includeInactive && typeof(IAuditableEntity).IsAssignableFrom(typeof(T)))
            {
                query = query.Where(e => ((IAuditableEntity)e).IsActive);
            }

            var entities = await query.ToListAsync();

            // Cache the result if strategy is available
            if (_cacheStrategy != null)
            {
                var cacheKey = GetCacheKeyForAll(includeInactive);
                await _cacheStrategy.SetAsync(cacheKey, entities);
            }

            return entities;
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, bool includeInactive = false)
        {
            var predicateHash = predicate.ToString().GetHashCode().ToString();
            var cacheKey = GetCacheKeyForFind(predicateHash, includeInactive);

            if (_cacheStrategy != null)
            {
                var cachedEntities = await _cacheStrategy.GetAsync<List<T>>(cacheKey);
                if (cachedEntities != null)
                {
                    return cachedEntities;
                }
            }

            // Fetch from database
            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.Set<T>().Where(predicate);

            if (!includeInactive && typeof(IAuditableEntity).IsAssignableFrom(typeof(T)))
            {
                query = query.Where(e => ((IAuditableEntity)e).IsActive);
            }

            var entities = await query.ToListAsync();

            // Cache the result if strategy is available
            if (_cacheStrategy != null)
            {
                await _cacheStrategy.SetAsync(cacheKey, entities);
            }

            return entities;
        }

        public virtual async Task<T> AddAsync(T entity, string createdBy = "System")
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            if (entity is IAuditableEntity auditableEntity)
            {
                auditableEntity.CreatedBy = createdBy;
                auditableEntity.CreatedOn = DateTime.UtcNow;
                auditableEntity.IsActive = true;
            }

            context.Set<T>().Add(entity);
            await context.SaveChangesAsync();

            // Invalidate collection-level cache after successful add
            await InvalidateCollectionCacheAsync();

            return entity;
        }

        public virtual async Task<T> UpdateAsync(T entity, string modifiedBy = "System")
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            if (entity is IAuditableEntity auditableEntity)
            {
                auditableEntity.ModifiedBy = modifiedBy;
                auditableEntity.ModifiedOn = DateTime.UtcNow;
            }

            context.Set<T>().Update(entity);
            await context.SaveChangesAsync();

            // Invalidate all cache for this entity after successful update
            if (entity is BaseEntity baseEntity)
            {
                await InvalidateEntityCacheAsync(baseEntity.Id);
            }
            await InvalidateCollectionCacheAsync();

            return entity;
        }

        public virtual async Task<bool> SetActiveStatusAsync(int id, bool isActive, string modifiedBy = "System")
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Set<T>().FindAsync(id);

            if (entity == null || entity is not IAuditableEntity auditableEntity)
            {
                return false;
            }

            auditableEntity.IsActive = isActive;
            auditableEntity.ModifiedBy = modifiedBy;
            auditableEntity.ModifiedOn = DateTime.UtcNow;

            await context.SaveChangesAsync();

            // Invalidate all cache for this entity after successful status change
            await InvalidateEntityCacheAsync(id);
            await InvalidateCollectionCacheAsync();

            return true;
        }

        public virtual async Task<IEnumerable<T>> GetActiveOnlyAsync()
        {
            // Try cache first if strategy is available
            if (_cacheStrategy != null)
            {
                var cacheKey = GetCacheKeyForActiveOnly();
                var cachedEntities = await _cacheStrategy.GetAsync<List<T>>(cacheKey);
                if (cachedEntities != null)
                {
                    return cachedEntities;
                }
            }

            // Fetch from database
            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.Set<T>().AsQueryable();

            if (typeof(IAuditableEntity).IsAssignableFrom(typeof(T)))
            {
                query = query.Where(e => ((IAuditableEntity)e).IsActive);
            }

            var entities = await query.ToListAsync();

            // Cache the result if strategy is available
            if (_cacheStrategy != null)
            {
                var cacheKey = GetCacheKeyForActiveOnly();
                await _cacheStrategy.SetAsync(cacheKey, entities);
            }

            return entities;
        }

        public virtual async Task<IEnumerable<T>> GetInactiveOnlyAsync()
        {
            // Try cache first if strategy is available
            if (_cacheStrategy != null)
            {
                var cacheKey = GetCacheKeyForInactiveOnly();
                var cachedEntities = await _cacheStrategy.GetAsync<List<T>>(cacheKey);
                if (cachedEntities != null)
                {
                    return cachedEntities;
                }
            }

            // Fetch from database
            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.Set<T>().AsQueryable();

            if (typeof(IAuditableEntity).IsAssignableFrom(typeof(T)))
            {
                query = query.Where(e => !((IAuditableEntity)e).IsActive);
            }
            else
            {
                // If entity doesn't implement IAuditableEntity, return empty collection
                return new List<T>();
            }

            var entities = await query.ToListAsync();

            // Cache the result if strategy is available
            if (_cacheStrategy != null)
            {
                var cacheKey = GetCacheKeyForInactiveOnly();
                await _cacheStrategy.SetAsync(cacheKey, entities);
            }

            return entities;
        }

        public virtual async Task<bool> ExistsAsync(int id, bool includeInactive = false)
        {
            // Try to get from cache first if strategy is available
            if (_cacheStrategy != null)
            {
                var cacheKey = GetCacheKeyById(id, includeInactive);
                var cachedEntity = await _cacheStrategy.GetAsync<T>(cacheKey);
                if (cachedEntity != null)
                {
                    return true;
                }
            }

            // Check database
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Set<T>().FindAsync(id);

            if (entity == null)
            {
                return false;
            }

            if (!includeInactive && entity is IAuditableEntity auditableEntity && !auditableEntity.IsActive)
            {
                return false;
            }

            return true;
        }

        public virtual async Task DeleteAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Set<T>().FindAsync(id);
            if (entity != null)
            {
                context.Set<T>().Remove(entity);
                await context.SaveChangesAsync();

                // Invalidate all cache for this entity after successful deletion
                await InvalidateEntityCacheAsync(id);
                await InvalidateCollectionCacheAsync();
            }
        }
    }
}
