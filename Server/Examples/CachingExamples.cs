/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.EntityFrameworkCore;
using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Common.Interfaces;
using msih.p4g.Server.Common.Models;

namespace msih.p4g.Server.Examples
{
    /// <summary>
    /// Example entity for demonstrating caching functionality
    /// </summary>
    public class ExampleEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Example repository without caching (traditional approach)
    /// </summary>
    public class ExampleRepositoryWithoutCache : GenericRepository<ExampleEntity>
    {
        public ExampleRepositoryWithoutCache(IDbContextFactory<ApplicationDbContext> contextFactory) 
            : base(contextFactory)
        {
            // No caching - uses traditional database-only approach
        }
    }

    /// <summary>
    /// Example repository with caching enabled (opt-in approach)
    /// </summary>
    public class ExampleRepositoryWithCache : GenericRepository<ExampleEntity>
    {
        public ExampleRepositoryWithCache(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            ICacheStrategy cacheStrategy) 
            : base(contextFactory, cacheStrategy)
        {
            // Caching enabled - all read operations will check cache first
            // Write operations will automatically invalidate relevant cache entries
        }

        /// <summary>
        /// Example of custom method with specific cache expiration
        /// </summary>
        public async Task<ExampleEntity?> GetByNameAsync(string name)
        {
            // Create a custom cache key for this specific operation
            var cacheKey = $"{typeof(ExampleEntity).Name}:Name:{name}";
            
            // Try cache first if available
            if (_cacheStrategy != null)
            {
                var cachedResult = await _cacheStrategy.GetAsync<ExampleEntity>(cacheKey);
                if (cachedResult != null)
                {
                    return cachedResult;
                }
            }

            // Fetch from database
            var entities = await FindAsync(e => e.Name == name);
            var entity = entities.FirstOrDefault();

            // Cache with custom 15-minute expiration
            if (_cacheStrategy != null && entity != null)
            {
                await _cacheStrategy.SetAsync(cacheKey, entity, TimeSpan.FromMinutes(15));
            }

            return entity;
        }

        /// <summary>
        /// Custom update method that invalidates specific cache entries
        /// </summary>
        public async Task<ExampleEntity> UpdateNameAsync(int id, string newName, string modifiedBy = "System")
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
                throw new ArgumentException($"Entity with ID {id} not found");

            // Store old name for cache invalidation
            var oldName = entity.Name;
            entity.Name = newName;

            // Update using base method (automatically invalidates entity and collection caches)
            var updatedEntity = await UpdateAsync(entity, modifiedBy);

            // Invalidate custom name-based cache entries
            if (_cacheStrategy != null)
            {
                await _cacheStrategy.RemoveAsync($"{typeof(ExampleEntity).Name}:Name:{oldName}");
                await _cacheStrategy.RemoveAsync($"{typeof(ExampleEntity).Name}:Name:{newName}");
            }

            return updatedEntity;
        }
    }

    /// <summary>
    /// Example service demonstrating dependency injection setup for caching
    /// </summary>
    public class ExampleService
    {
        private readonly ExampleRepositoryWithCache _repository;

        public ExampleService(ExampleRepositoryWithCache repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Example method that benefits from caching
        /// </summary>
        public async Task<IEnumerable<ExampleEntity>> GetPopularEntitiesAsync()
        {
            // This will be cached after first call, improving performance for subsequent requests
            return await _repository.GetActiveOnlyAsync();
        }

        /// <summary>
        /// Example method that shows cache invalidation in action
        /// </summary>
        public async Task<ExampleEntity> CreateNewEntityAsync(string name, string description)
        {
            var entity = new ExampleEntity 
            { 
                Name = name, 
                Description = description 
            };

            // This will automatically invalidate collection caches (GetActiveOnlyAsync, GetAllAsync, etc.)
            return await _repository.AddAsync(entity, "ExampleService");
        }
    }
}

/*
 * Dependency Injection Setup Example (Program.cs or Startup.cs):
 * 
 * // Register memory cache
 * services.AddMemoryCache();
 * 
 * // Register cache strategy
 * services.AddScoped<ICacheStrategy, MemoryCacheStrategy>();
 * 
 * // Register repository with caching
 * services.AddScoped<ExampleRepositoryWithCache>();
 * 
 * // Or without caching
 * services.AddScoped<ExampleRepositoryWithoutCache>();
 * 
 * // Register service
 * services.AddScoped<ExampleService>();
 * 
 * Usage Examples:
 * 
 * 1. First call - fetches from database and caches:
 *    var entities = await exampleService.GetPopularEntitiesAsync(); // Database hit + Cache store
 * 
 * 2. Subsequent calls - served from cache:
 *    var entities = await exampleService.GetPopularEntitiesAsync(); // Cache hit
 * 
 * 3. After creating new entity:
 *    await exampleService.CreateNewEntityAsync("New Item", "Description"); // Invalidates cache
 *    var entities = await exampleService.GetPopularEntitiesAsync(); // Database hit + Cache store (fresh data)
 */