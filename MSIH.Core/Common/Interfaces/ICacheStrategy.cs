/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using System;
using System.Threading.Tasks;

namespace MSIH.Core.Common.Interfaces
{
    /// <summary>
    /// Interface for pluggable caching strategies used by repositories
    /// </summary>
    public interface ICacheStrategy
    {
        /// <summary>
        /// Gets a cached value by key
        /// </summary>
        /// <typeparam name="T">The type of the cached value</typeparam>
        /// <param name="key">The cache key</param>
        /// <returns>The cached value if found, otherwise null</returns>
        Task<T?> GetAsync<T>(string key) where T : class;

        /// <summary>
        /// Sets a value in the cache with optional expiration
        /// </summary>
        /// <typeparam name="T">The type of the value to cache</typeparam>
        /// <param name="key">The cache key</param>
        /// <param name="value">The value to cache</param>
        /// <param name="expiration">Optional expiration time. If null, cache indefinitely</param>
        /// <returns>Task representing the async operation</returns>
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;

        /// <summary>
        /// Removes a specific cache entry by key
        /// </summary>
        /// <param name="key">The cache key to remove</param>
        /// <returns>Task representing the async operation</returns>
        Task RemoveAsync(string key);

        /// <summary>
        /// Removes cache entries that match the specified pattern
        /// This is useful for cache invalidation when entities are modified
        /// </summary>
        /// <param name="pattern">The pattern to match cache keys (supports wildcards)</param>
        /// <returns>Task representing the async operation</returns>
        Task RemoveByPatternAsync(string pattern);
    }
}