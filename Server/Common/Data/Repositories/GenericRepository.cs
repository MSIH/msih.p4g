/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using msih.p4g.Server.Common.Models;

namespace msih.p4g.Server.Common.Data.Repositories
{
    /// <summary>
    /// Generic repository implementation for CRUD operations with support for active status
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <typeparam name="TContext">The DbContext type</typeparam>
    public class GenericRepository<TEntity, TContext> : IGenericRepository<TEntity> 
        where TEntity : BaseEntity
        where TContext : DbContext
    {
        protected readonly TContext _context;
        protected readonly DbSet<TEntity> _dbSet;
        
        public GenericRepository(TContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<TEntity>();
        }
        
        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(bool includeInactive = false)
        {
            var query = _dbSet.AsQueryable();
            
            // Apply filter for IsActive
            if (!includeInactive)
            {
                query = query.Where(e => e.IsActive);
            }
            
            return await query.ToListAsync();
        }
        
        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> FindAsync(
            Expression<Func<TEntity, bool>> predicate, 
            bool includeInactive = false)
        {
            var query = _dbSet.Where(predicate);
            
            // Apply filter for IsActive
            if (!includeInactive)
            {
                query = query.Where(e => e.IsActive);
            }
            
            return await query.ToListAsync();
        }
        
        /// <inheritdoc />
        public virtual async Task<TEntity> GetByIdAsync(int id, bool includeInactive = false)
        {
            var query = _dbSet.Where(e => e.Id == id);
            
            // Apply filter for IsActive
            if (!includeInactive)
            {
                query = query.Where(e => e.IsActive);
            }
            
            return await query.FirstOrDefaultAsync();
        }
        
        /// <inheritdoc />
        public virtual async Task<TEntity> AddAsync(TEntity entity, string createdBy = "System")
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            
            // Set audit properties
            entity.CreatedOn = DateTime.UtcNow;
            entity.CreatedBy = createdBy;
            entity.IsActive = true;
            
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            
            return entity;
        }
        
        /// <inheritdoc />
        public virtual async Task<TEntity> UpdateAsync(TEntity entity, string modifiedBy = "System")
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            
            // Set audit properties
            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = modifiedBy;
            
            _context.Entry(entity).State = EntityState.Modified;
            
            // Preserve created properties
            _context.Entry(entity).Property(e => e.CreatedOn).IsModified = false;
            _context.Entry(entity).Property(e => e.CreatedBy).IsModified = false;
            
            await _context.SaveChangesAsync();
            
            return entity;
        }
        
        /// <inheritdoc />
        public virtual async Task<bool> DeleteAsync(int id, string modifiedBy = "System")
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null)
            {
                return false;
            }
            
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
            
            return true;
        }
        
        /// <inheritdoc />
        public virtual async Task<bool> SetActiveStatusAsync(int id, bool isActive, string modifiedBy = "System")
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null)
            {
                return false;
            }
            
            entity.IsActive = isActive;
            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = modifiedBy;
            
            await _context.SaveChangesAsync();
            
            return true;
        }
        
        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> GetActiveOnlyAsync()
        {
            return await _dbSet
                .Where(e => e.IsActive)
                .ToListAsync();
        }
        
        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> GetInactiveOnlyAsync()
        {
            return await _dbSet
                .Where(e => !e.IsActive)
                .ToListAsync();
        }
        
        /// <inheritdoc />
        public virtual async Task<bool> ExistsAsync(int id, bool includeInactive = false)
        {
            var query = _dbSet.Where(e => e.Id == id);
            
            // Apply filter for IsActive
            if (!includeInactive)
            {
                query = query.Where(e => e.IsActive);
            }
            
            return await query.AnyAsync();
        }
    }
}
