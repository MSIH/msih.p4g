/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using msih.p4g.Server.Common.Services;

namespace msih.p4g.Tests.Server.Tests.Common.Services
{
    [TestClass]
    public class MemoryCacheStrategyTests
    {
        private IMemoryCache _memoryCache;
        private MemoryCacheStrategy _cacheStrategy;

        [TestInitialize]
        public void TestInitialize()
        {
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _cacheStrategy = new MemoryCacheStrategy(_memoryCache);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _memoryCache?.Dispose();
        }

        [TestMethod]
        public async Task SetAsync_And_GetAsync_ShouldWork()
        {
            // Arrange
            var key = "test-key";
            var value = new TestCacheItem { Name = "Test", Value = 123 };

            // Act
            await _cacheStrategy.SetAsync(key, value);
            var result = await _cacheStrategy.GetAsync<TestCacheItem>(key);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(value.Name, result.Name);
            Assert.AreEqual(value.Value, result.Value);
        }

        [TestMethod]
        public async Task GetAsync_WithNonExistentKey_ShouldReturnNull()
        {
            // Act
            var result = await _cacheStrategy.GetAsync<TestCacheItem>("non-existent-key");

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task SetAsync_WithExpiration_ShouldExpireAfterTime()
        {
            // Arrange
            var key = "expiring-key";
            var value = new TestCacheItem { Name = "Expiring", Value = 456 };
            var expiration = TimeSpan.FromMilliseconds(100);

            // Act
            await _cacheStrategy.SetAsync(key, value, expiration);
            
            // Verify it exists immediately
            var immediateResult = await _cacheStrategy.GetAsync<TestCacheItem>(key);
            Assert.IsNotNull(immediateResult);

            // Wait for expiration
            await Task.Delay(150);

            // Verify it's expired
            var expiredResult = await _cacheStrategy.GetAsync<TestCacheItem>(key);
            Assert.IsNull(expiredResult);
        }

        [TestMethod]
        public async Task RemoveAsync_ShouldRemoveSpecificKey()
        {
            // Arrange
            var key1 = "key1";
            var key2 = "key2";
            var value1 = new TestCacheItem { Name = "Value1", Value = 1 };
            var value2 = new TestCacheItem { Name = "Value2", Value = 2 };

            await _cacheStrategy.SetAsync(key1, value1);
            await _cacheStrategy.SetAsync(key2, value2);

            // Act
            await _cacheStrategy.RemoveAsync(key1);

            // Assert
            var result1 = await _cacheStrategy.GetAsync<TestCacheItem>(key1);
            var result2 = await _cacheStrategy.GetAsync<TestCacheItem>(key2);

            Assert.IsNull(result1);
            Assert.IsNotNull(result2);
        }

        [TestMethod]
        public async Task RemoveByPatternAsync_ShouldRemoveMatchingKeys()
        {
            // Arrange
            var keys = new[]
            {
                "Entity:Id:1",
                "Entity:Id:2",
                "Entity:All:false",
                "DifferentEntity:Id:1"
            };

            foreach (var key in keys)
            {
                await _cacheStrategy.SetAsync(key, new TestCacheItem { Name = key, Value = 1 });
            }

            // Act
            await _cacheStrategy.RemoveByPatternAsync("Entity:*");

            // Assert
            Assert.IsNull(await _cacheStrategy.GetAsync<TestCacheItem>("Entity:Id:1"));
            Assert.IsNull(await _cacheStrategy.GetAsync<TestCacheItem>("Entity:Id:2"));
            Assert.IsNull(await _cacheStrategy.GetAsync<TestCacheItem>("Entity:All:false"));
            Assert.IsNotNull(await _cacheStrategy.GetAsync<TestCacheItem>("DifferentEntity:Id:1"));
        }

        [TestMethod]
        public async Task RemoveByPatternAsync_WithSpecificPattern_ShouldOnlyRemoveMatching()
        {
            // Arrange
            var keys = new[]
            {
                "Entity:Id:1:IncludeInactive:true",
                "Entity:Id:1:IncludeInactive:false",
                "Entity:Id:2:IncludeInactive:true",
                "Entity:All:IncludeInactive:false"
            };

            foreach (var key in keys)
            {
                await _cacheStrategy.SetAsync(key, new TestCacheItem { Name = key, Value = 1 });
            }

            // Act
            await _cacheStrategy.RemoveByPatternAsync("Entity:Id:1:*");

            // Assert
            Assert.IsNull(await _cacheStrategy.GetAsync<TestCacheItem>("Entity:Id:1:IncludeInactive:true"));
            Assert.IsNull(await _cacheStrategy.GetAsync<TestCacheItem>("Entity:Id:1:IncludeInactive:false"));
            Assert.IsNotNull(await _cacheStrategy.GetAsync<TestCacheItem>("Entity:Id:2:IncludeInactive:true"));
            Assert.IsNotNull(await _cacheStrategy.GetAsync<TestCacheItem>("Entity:All:IncludeInactive:false"));
        }

        [TestMethod]
        public void Constructor_WithNullMemoryCache_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new MemoryCacheStrategy(null!));
        }

        [TestMethod]
        public async Task SetAsync_WithNullValue_ShouldNotCache()
        {
            // Act
            await _cacheStrategy.SetAsync("test-key", null!);
            var result = await _cacheStrategy.GetAsync<TestCacheItem>("test-key");

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetAsync_WithEmptyKey_ShouldReturnNull()
        {
            // Act
            var result1 = await _cacheStrategy.GetAsync<TestCacheItem>("");
            var result2 = await _cacheStrategy.GetAsync<TestCacheItem>(null!);

            // Assert
            Assert.IsNull(result1);
            Assert.IsNull(result2);
        }
    }

    // Test class for caching
    public class TestCacheItem
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}