/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MSIH.Core.Common.Data.Repositories
{
    /// <summary>
    /// Generic repository interface for CRUD operations with support for active status
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Gets all entities
        /// </summary>
        /// <param name="includeInactive">Whether to include inactive entities</param>
        /// <returns>All entities matching the criteria</returns>
        Task<IEnumerable<TEntity>> GetAllAsync(bool includeInactive = false);
        
        /// <summary>
        /// Gets entities that match the specified predicate
        /// </summary>
        /// <param name="predicate">The filter expression</param>
        /// <param name="includeInactive">Whether to include inactive entities</param>
        /// <returns>Entities matching the predicate and criteria</returns>
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, bool includeInactive = false);
        
        /// <summary>
        /// Gets an entity by its ID
        /// </summary>
        /// <param name="id">The entity ID</param>
        /// <param name="includeInactive">Whether to include inactive entities</param>
        /// <returns>The entity with the specified ID, or null if not found</returns>
        Task<TEntity?> GetByIdAsync(int id, bool includeInactive = false);
        
        /// <summary>
        /// Adds a new entity
        /// </summary>
        /// <param name="entity">The entity to add</param>
        /// <param name="createdBy">The user who created the entity</param>
        /// <returns>The added entity</returns>
        Task<TEntity> AddAsync(TEntity entity, string createdBy = "System");
        
        /// <summary>
        /// Updates an existing entity
        /// </summary>
        /// <param name="entity">The entity to update</param>
        /// <param name="modifiedBy">The user who modified the entity</param>
        /// <returns>The updated entity</returns>
        Task<TEntity> UpdateAsync(TEntity entity, string modifiedBy = "System");
        
        /// <summary>
        /// Activates or deactivates an entity
        /// </summary>
        /// <param name="id">The ID of the entity</param>
        /// <param name="isActive">Whether the entity should be active</param>
        /// <param name="modifiedBy">The user who modified the entity</param>
        /// <returns>True if the entity was updated, false otherwise</returns>
        Task<bool> SetActiveStatusAsync(int id, bool isActive, string modifiedBy = "System");
        
        /// <summary>
        /// Gets only active entities
        /// </summary>
        /// <returns>Active entities</returns>
        Task<IEnumerable<TEntity>> GetActiveOnlyAsync();
        
        /// <summary>
        /// Gets only inactive entities
        /// </summary>
        /// <returns>Inactive entities</returns>
        Task<IEnumerable<TEntity>> GetInactiveOnlyAsync();
        
        /// <summary>
        /// Checks if an entity with the specified ID exists
        /// </summary>
        /// <param name="id">The entity ID</param>
        /// <param name="includeInactive">Whether to include inactive entities</param>
        /// <returns>True if the entity exists, false otherwise</returns>
        Task<bool> ExistsAsync(int id, bool includeInactive = false);
    }
}