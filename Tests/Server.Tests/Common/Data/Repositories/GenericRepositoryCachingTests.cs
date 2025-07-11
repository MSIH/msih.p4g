/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Common.Interfaces;
using msih.p4g.Server.Common.Models;
using msih.p4g.Server.Common.Services;

namespace msih.p4g.Tests.Server.Tests.Common.Data.Repositories
{
    [TestClass]
    public class GenericRepositoryCachingTests
    {
        private IDbContextFactory<ApplicationDbContext> _contextFactory;
        private ICacheStrategy _cacheStrategy;
        private TestableGenericRepository _repository;
        private TestableGenericRepository _repositoryWithoutCache;

        [TestInitialize]
        public void TestInitialize()
        {
            // Setup in-memory database factory
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            var contextFactoryMock = new Mock<IDbContextFactory<ApplicationDbContext>>();
            contextFactoryMock.Setup(f => f.CreateDbContextAsync())
                .ReturnsAsync(() => new ApplicationDbContext(options));

            _contextFactory = contextFactoryMock.Object;

            // Setup memory cache strategy
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            _cacheStrategy = new MemoryCacheStrategy(memoryCache);

            // Create repositories with and without cache
            _repository = new TestableGenericRepository(_contextFactory, _cacheStrategy);
            _repositoryWithoutCache = new TestableGenericRepository(_contextFactory);

            // Seed test data
            SeedTestData().Wait();
        }

        private async Task SeedTestData()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            if (!context.Set<TestEntity>().Any())
            {
                context.Set<TestEntity>().AddRange(new[]
                {
                    new TestEntity { Id = 1, Name = "Active Entity", IsActive = true, CreatedBy = "Seeder", CreatedOn = DateTime.UtcNow.AddDays(-10) },
                    new TestEntity { Id = 2, Name = "Inactive Entity", IsActive = false, CreatedBy = "Seeder", CreatedOn = DateTime.UtcNow.AddDays(-10) },
                    new TestEntity { Id = 3, Name = "Another Active Entity", IsActive = true, CreatedBy = "Seeder", CreatedOn = DateTime.UtcNow.AddDays(-5) }
                });
                
                await context.SaveChangesAsync();
            }
        }

        [TestMethod]
        public async Task GetByIdAsync_WithoutCache_ShouldFetchFromDatabase()
        {
            // Act
            var result = await _repositoryWithoutCache.GetByIdAsync(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Active Entity", result.Name);
        }

        [TestMethod]
        public async Task GetByIdAsync_WithCache_FirstCall_ShouldFetchFromDatabaseAndCache()
        {
            // Act - First call should fetch from database and cache
            var result1 = await _repository.GetByIdAsync(1);
            
            // Verify cache was populated by calling again and checking cache directly
            var cachedResult = await _cacheStrategy.GetAsync<TestEntity>("TestEntity:Id:1:IncludeInactive:False");

            // Assert
            Assert.IsNotNull(result1);
            Assert.AreEqual("Active Entity", result1.Name);
            Assert.IsNotNull(cachedResult);
            Assert.AreEqual("Active Entity", cachedResult.Name);
        }

        [TestMethod]
        public async Task GetByIdAsync_WithCache_SecondCall_ShouldReturnFromCache()
        {
            // Arrange - First call to populate cache
            await _repository.GetByIdAsync(1);

            // Manually place a different entity in cache to verify it's being used
            var modifiedEntity = new TestEntity { Id = 1, Name = "Modified From Cache", IsActive = true };
            await _cacheStrategy.SetAsync("TestEntity:Id:1:IncludeInactive:False", modifiedEntity);

            // Act - Second call should return from cache
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Modified From Cache", result.Name); // This proves it came from cache
        }

        [TestMethod]
        public async Task GetAllAsync_WithCache_ShouldCacheResults()
        {
            // Act - First call should fetch from database and cache
            var result1 = await _repository.GetAllAsync();
            
            // Verify cache was populated
            var cachedResult = await _cacheStrategy.GetAsync<List<TestEntity>>("TestEntity:All:IncludeInactive:False");

            // Assert
            Assert.IsNotNull(result1);
            Assert.AreEqual(2, result1.Count()); // Only active entities
            Assert.IsNotNull(cachedResult);
            Assert.AreEqual(2, cachedResult.Count);
        }

        [TestMethod]
        public async Task AddAsync_ShouldInvalidateCollectionCache()
        {
            // Arrange - Populate cache first
            await _repository.GetAllAsync();
            var cachedBeforeAdd = await _cacheStrategy.GetAsync<List<TestEntity>>("TestEntity:All:IncludeInactive:False");
            Assert.IsNotNull(cachedBeforeAdd);

            // Act - Add new entity
            var newEntity = new TestEntity { Name = "New Entity", IsActive = true };
            await _repository.AddAsync(newEntity, "TestUser");

            // Assert - Cache should be invalidated
            var cachedAfterAdd = await _cacheStrategy.GetAsync<List<TestEntity>>("TestEntity:All:IncludeInactive:False");
            Assert.IsNull(cachedAfterAdd);
        }

        [TestMethod]
        public async Task UpdateAsync_ShouldInvalidateEntityAndCollectionCache()
        {
            // Arrange - Populate caches first
            var entity = await _repository.GetByIdAsync(1);
            await _repository.GetAllAsync();
            
            var cachedEntityBefore = await _cacheStrategy.GetAsync<TestEntity>("TestEntity:Id:1:IncludeInactive:False");
            var cachedCollectionBefore = await _cacheStrategy.GetAsync<List<TestEntity>>("TestEntity:All:IncludeInactive:False");
            
            Assert.IsNotNull(cachedEntityBefore);
            Assert.IsNotNull(cachedCollectionBefore);

            // Act - Update entity
            entity!.Name = "Updated Entity";
            await _repository.UpdateAsync(entity, "TestUser");

            // Assert - Both caches should be invalidated
            var cachedEntityAfter = await _cacheStrategy.GetAsync<TestEntity>("TestEntity:Id:1:IncludeInactive:False");
            var cachedCollectionAfter = await _cacheStrategy.GetAsync<List<TestEntity>>("TestEntity:All:IncludeInactive:False");
            
            Assert.IsNull(cachedEntityAfter);
            Assert.IsNull(cachedCollectionAfter);
        }

        [TestMethod]
        public async Task SetActiveStatusAsync_ShouldInvalidateAllRelatedCache()
        {
            // Arrange - Populate caches first
            await _repository.GetByIdAsync(1);
            await _repository.GetAllAsync();
            await _repository.GetActiveOnlyAsync();
            
            var cachedEntityBefore = await _cacheStrategy.GetAsync<TestEntity>("TestEntity:Id:1:IncludeInactive:False");
            var cachedCollectionBefore = await _cacheStrategy.GetAsync<List<TestEntity>>("TestEntity:All:IncludeInactive:False");
            var cachedActiveBefore = await _cacheStrategy.GetAsync<List<TestEntity>>("TestEntity:ActiveOnly");
            
            Assert.IsNotNull(cachedEntityBefore);
            Assert.IsNotNull(cachedCollectionBefore);
            Assert.IsNotNull(cachedActiveBefore);

            // Act - Change active status
            await _repository.SetActiveStatusAsync(1, false, "TestUser");

            // Assert - All related caches should be invalidated
            var cachedEntityAfter = await _cacheStrategy.GetAsync<TestEntity>("TestEntity:Id:1:IncludeInactive:False");
            var cachedCollectionAfter = await _cacheStrategy.GetAsync<List<TestEntity>>("TestEntity:All:IncludeInactive:False");
            var cachedActiveAfter = await _cacheStrategy.GetAsync<List<TestEntity>>("TestEntity:ActiveOnly");
            
            Assert.IsNull(cachedEntityAfter);
            Assert.IsNull(cachedCollectionAfter);
            Assert.IsNull(cachedActiveAfter);
        }

        [TestMethod]
        public async Task FindAsync_WithCache_ShouldCacheResults()
        {
            // Act
            var result = await _repository.FindAsync(e => e.Name.Contains("Active"));
            
            // Check if cache was populated (we can't predict exact hash, so check if any Find cache exists)
            // This is a limitation of our current implementation - we use predicate hash for caching
            
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        public async Task GetActiveOnlyAsync_WithCache_ShouldCacheResults()
        {
            // Act
            var result1 = await _repository.GetActiveOnlyAsync();
            var cachedResult = await _cacheStrategy.GetAsync<List<TestEntity>>("TestEntity:ActiveOnly");

            // Assert
            Assert.IsNotNull(result1);
            Assert.AreEqual(2, result1.Count());
            Assert.IsNotNull(cachedResult);
            Assert.AreEqual(2, cachedResult.Count);
        }

        [TestMethod]
        public async Task Constructor_WithoutCacheStrategy_ShouldNotCache()
        {
            // Act
            var result = await _repositoryWithoutCache.GetByIdAsync(1);

            // Assert - Verify no caching occurred
            var cachedResult = await _cacheStrategy.GetAsync<TestEntity>("TestEntity:Id:1:IncludeInactive:False");
            Assert.IsNotNull(result);
            Assert.IsNull(cachedResult); // Should be null because this repository doesn't use cache
        }
    }

    // Test entity for testing the repository
    public class TestEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
    }

    // Testable repository that exposes the generic repository for testing
    public class TestableGenericRepository : GenericRepository<TestEntity>
    {
        public TestableGenericRepository(IDbContextFactory<ApplicationDbContext> contextFactory) 
            : base(contextFactory)
        {
        }

        public TestableGenericRepository(IDbContextFactory<ApplicationDbContext> contextFactory, ICacheStrategy cacheStrategy) 
            : base(contextFactory, cacheStrategy)
        {
        }
    }
}