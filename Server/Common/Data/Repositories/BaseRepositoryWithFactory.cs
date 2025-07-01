// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

using Microsoft.EntityFrameworkCore;
using msih.p4g.Server.Common.Models;
using System.Linq.Expressions;

namespace msih.p4g.Server.Common.Data.Repositories
{
    /// <summary>
    /// Base repository implementation using DbContextFactory for thread-safe operations
    /// </summary>
    public abstract class BaseRepositoryWithFactory<T> : IGenericRepository<T> where T : class
    {
        protected readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        protected BaseRepositoryWithFactory(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public virtual async Task<T?> GetByIdAsync(int id, bool includeInactive = false)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var entity = await context.Set<T>().FindAsync(id);

            if (entity != null && entity is IAuditableEntity auditableEntity && !includeInactive && !auditableEntity.IsActive)
            {
                return null;
            }

            return entity;
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(bool includeInactive = false)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.Set<T>().AsQueryable();

            if (!includeInactive && typeof(IAuditableEntity).IsAssignableFrom(typeof(T)))
            {
                query = query.Where(e => ((IAuditableEntity)e).IsActive);
            }

            return await query.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, bool includeInactive = false)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.Set<T>().Where(predicate);

            if (!includeInactive && typeof(IAuditableEntity).IsAssignableFrom(typeof(T)))
            {
                query = query.Where(e => ((IAuditableEntity)e).IsActive);
            }

            return await query.ToListAsync();
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
            return true;
        }

        public virtual async Task<IEnumerable<T>> GetActiveOnlyAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.Set<T>().AsQueryable();

            if (typeof(IAuditableEntity).IsAssignableFrom(typeof(T)))
            {
                query = query.Where(e => ((IAuditableEntity)e).IsActive);
            }

            return await query.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> GetInactiveOnlyAsync()
        {
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

            return await query.ToListAsync();
        }

        public virtual async Task<bool> ExistsAsync(int id, bool includeInactive = false)
        {
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
            }
        }
    }
}
