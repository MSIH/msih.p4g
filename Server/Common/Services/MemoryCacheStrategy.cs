/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using msih.p4g.Server.Common.Interfaces;

namespace msih.p4g.Server.Common.Services
{
    /// <summary>
    /// Memory-based cache strategy implementation using IMemoryCache
    /// </summary>
    public class MemoryCacheStrategy : ICacheStrategy
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ConcurrentDictionary<string, object> _keyTracker;

        /// <summary>
        /// Initializes a new instance of MemoryCacheStrategy
        /// </summary>
        /// <param name="memoryCache">The IMemoryCache instance to use</param>
        public MemoryCacheStrategy(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _keyTracker = new ConcurrentDictionary<string, object>();
        }

        /// <summary>
        /// Gets a cached value by key
        /// </summary>
        /// <typeparam name="T">The type of the cached value</typeparam>
        /// <param name="key">The cache key</param>
        /// <returns>The cached value if found, otherwise null</returns>
        public Task<T?> GetAsync<T>(string key) where T : class
        {
            if (string.IsNullOrEmpty(key))
                return Task.FromResult<T?>(null);

            var result = _memoryCache.Get<T>(key);
            return Task.FromResult(result);
        }

        /// <summary>
        /// Sets a value in the cache with optional expiration
        /// </summary>
        /// <typeparam name="T">The type of the value to cache</typeparam>
        /// <param name="key">The cache key</param>
        /// <param name="value">The value to cache</param>
        /// <param name="expiration">Optional expiration time. If null, cache indefinitely</param>
        /// <returns>Task representing the async operation</returns>
        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            if (string.IsNullOrEmpty(key) || value == null)
                return Task.CompletedTask;

            var options = new MemoryCacheEntryOptions
            {
                // Set up callback to remove from key tracker when evicted
                PostEvictionCallbacks = { new PostEvictionCallbackRegistration
                {
                    EvictionCallback = (k, v, reason, state) => _keyTracker.TryRemove(k.ToString()!, out _)
                }}
            };

            if (expiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiration.Value;
            }

            _memoryCache.Set(key, value, options);
            _keyTracker.TryAdd(key, true);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Removes a specific cache entry by key
        /// </summary>
        /// <param name="key">The cache key to remove</param>
        /// <returns>Task representing the async operation</returns>
        public Task RemoveAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
                return Task.CompletedTask;

            _memoryCache.Remove(key);
            _keyTracker.TryRemove(key, out _);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Removes cache entries that match the specified pattern
        /// This is useful for cache invalidation when entities are modified
        /// Pattern supports simple wildcard matching with '*'
        /// </summary>
        /// <param name="pattern">The pattern to match cache keys (supports wildcards)</param>
        /// <returns>Task representing the async operation</returns>
        public Task RemoveByPatternAsync(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                return Task.CompletedTask;

            // Convert simple wildcard pattern to regex-like matching
            var keysToRemove = _keyTracker.Keys
                .Where(key => MatchesPattern(key, pattern))
                .ToList();

            foreach (var key in keysToRemove)
            {
                _memoryCache.Remove(key);
                _keyTracker.TryRemove(key, out _);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Simple pattern matching that supports wildcards (*)
        /// </summary>
        /// <param name="input">The string to test</param>
        /// <param name="pattern">The pattern with optional wildcards</param>
        /// <returns>True if the input matches the pattern</returns>
        private static bool MatchesPattern(string input, string pattern)
        {
            // Handle simple cases
            if (pattern == "*") return true;
            if (!pattern.Contains('*')) return input == pattern;

            // Split pattern by wildcards and check each part
            var parts = pattern.Split('*', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return true;

            var currentIndex = 0;
            for (var i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                var index = input.IndexOf(part, currentIndex, StringComparison.OrdinalIgnoreCase);
                
                if (index == -1) return false;
                
                // For the first part, it should match from the beginning if pattern doesn't start with *
                if (i == 0 && !pattern.StartsWith('*') && index != 0) return false;
                
                currentIndex = index + part.Length;
            }

            // For the last part, it should match to the end if pattern doesn't end with *
            return pattern.EndsWith('*') || currentIndex == input.Length;
        }
    }
}