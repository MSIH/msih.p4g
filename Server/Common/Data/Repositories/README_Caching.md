# GenericRepository Caching Implementation

This document explains how to use the opt-in caching functionality in the GenericRepository.

## Overview

The GenericRepository now supports optional caching through a pluggable cache strategy interface. This allows repositories to enable or disable caching as needed without changing their core functionality.

## Architecture

### ICacheStrategy Interface

The `ICacheStrategy` interface provides the contract for caching implementations:

```csharp
public interface ICacheStrategy
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
    Task RemoveAsync(string key);
    Task RemoveByPatternAsync(string pattern);
}
```

### MemoryCacheStrategy Implementation

The included `MemoryCacheStrategy` uses `IMemoryCache` for in-memory caching with configurable expiration.

## Usage

### Without Caching (Default Behavior)

```csharp
// Traditional usage - no caching
public class UserRepository : GenericRepository<User>
{
    public UserRepository(IDbContextFactory<ApplicationDbContext> contextFactory) 
        : base(contextFactory)
    {
    }
}
```

### With Caching (Opt-in)

```csharp
// With caching enabled
public class UserRepository : GenericRepository<User>
{
    public UserRepository(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        ICacheStrategy cacheStrategy) 
        : base(contextFactory, cacheStrategy)
    {
    }
}
```

### Dependency Injection Setup

```csharp
// In Program.cs or Startup.cs
services.AddMemoryCache();
services.AddScoped<ICacheStrategy, MemoryCacheStrategy>();
services.AddScoped<UserRepository>();
```

### Using Custom Expiration

```csharp
// Custom cache strategy with specific expiration
public class UserRepository : GenericRepository<User>
{
    public UserRepository(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        ICacheStrategy cacheStrategy) 
        : base(contextFactory, cacheStrategy)
    {
    }

    public override async Task<User?> GetByIdAsync(int id, bool includeInactive = false)
    {
        // Custom expiration for this specific operation
        var result = await base.GetByIdAsync(id, includeInactive);
        
        if (_cacheStrategy != null && result != null)
        {
            var cacheKey = GetCacheKeyById(id, includeInactive);
            await _cacheStrategy.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30));
        }
        
        return result;
    }
}
```

## Cache Key Strategy

The repository generates cache keys using the following patterns:

- **GetByIdAsync**: `{EntityType}:Id:{id}:IncludeInactive:{includeInactive}`
- **GetAllAsync**: `{EntityType}:All:IncludeInactive:{includeInactive}`
- **FindAsync**: `{EntityType}:Find:{predicateHash}:IncludeInactive:{includeInactive}`
- **GetActiveOnlyAsync**: `{EntityType}:ActiveOnly`
- **GetInactiveOnlyAsync**: `{EntityType}:InactiveOnly`

## Cache Invalidation

Cache invalidation occurs automatically during write operations:

### AddAsync
- Invalidates: Collection-level caches (GetAll, ActiveOnly, InactiveOnly, Find patterns)

### UpdateAsync
- Invalidates: All entity-specific caches AND collection-level caches

### SetActiveStatusAsync
- Invalidates: All entity-specific caches AND collection-level caches

### DeleteAsync
- Invalidates: All entity-specific caches AND collection-level caches

## Cached Operations

The following repository methods support caching when a cache strategy is provided:

✅ **Read Operations (with caching)**
- `GetByIdAsync`
- `GetAllAsync`
- `FindAsync`
- `GetActiveOnlyAsync`
- `GetInactiveOnlyAsync`
- `ExistsAsync`

✅ **Write Operations (with cache invalidation)**
- `AddAsync`
- `UpdateAsync`
- `SetActiveStatusAsync`
- `DeleteAsync`

## Performance Considerations

### Benefits
- Reduced database queries for frequently accessed data
- Improved response times for read operations
- Configurable expiration for different data types

### Trade-offs
- Memory usage for cached data
- Cache invalidation overhead on write operations
- Potential for stale data if caching issues occur

## Best Practices

### When to Use Caching
- ✅ Frequently read, infrequently updated data
- ✅ Reference data (lookup tables, settings)
- ✅ Data that's expensive to compute or retrieve
- ✅ High-traffic read scenarios

### When NOT to Use Caching
- ❌ Real-time data requirements
- ❌ Data that changes frequently
- ❌ Large datasets that consume significant memory
- ❌ Simple operations where caching overhead exceeds benefits

### Configuration Recommendations

```csharp
// Configure memory cache options
services.Configure<MemoryCacheOptions>(options =>
{
    options.SizeLimit = 1000; // Limit number of entries
});

// Register cache strategy with specific configuration
services.AddScoped<ICacheStrategy>(provider =>
{
    var memoryCache = provider.GetRequiredService<IMemoryCache>();
    return new MemoryCacheStrategy(memoryCache);
});
```

## Custom Cache Strategy Implementation

You can implement your own cache strategy for different backends:

```csharp
public class RedisCacheStrategy : ICacheStrategy
{
    private readonly IDistributedCache _distributedCache;
    
    public RedisCacheStrategy(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }
    
    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        var json = await _distributedCache.GetStringAsync(key);
        return json != null ? JsonSerializer.Deserialize<T>(json) : null;
    }
    
    // ... implement other methods
}
```

## Monitoring and Debugging

### Cache Hit Metrics
Consider implementing metrics to monitor cache effectiveness:

```csharp
public class InstrumentedCacheStrategy : ICacheStrategy
{
    private readonly ICacheStrategy _inner;
    private readonly ILogger _logger;
    
    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        var result = await _inner.GetAsync<T>(key);
        _logger.LogDebug("Cache {Status} for key: {Key}", 
            result != null ? "HIT" : "MISS", key);
        return result;
    }
    
    // ... wrap other methods similarly
}
```

## Testing

The repository supports both cached and non-cached testing scenarios:

```csharp
[TestMethod]
public async Task TestWithoutCache()
{
    var repository = new UserRepository(contextFactory); // No cache
    var result = await repository.GetByIdAsync(1);
    // Test behavior without caching
}

[TestMethod] 
public async Task TestWithCache()
{
    var cacheStrategy = new MemoryCacheStrategy(new MemoryCache(new MemoryCacheOptions()));
    var repository = new UserRepository(contextFactory, cacheStrategy); // With cache
    var result = await repository.GetByIdAsync(1);
    // Test behavior with caching
}
```