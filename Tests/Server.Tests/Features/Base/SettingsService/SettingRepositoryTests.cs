/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Features.Base.SettingsService.Repositories;
using msih.p4g.Server.Features.Base.SettingsService.Model;
using Moq;
using msih.p4g.Server.Common.Interfaces;

namespace msih.p4g.Tests.Server.Tests.Features.Base.SettingsService
{
    [TestClass]
    public class SettingRepositoryTests
    {
        private IDbContextFactory<ApplicationDbContext> _contextFactory;
        private SettingRepository _repository;
        private Mock<ICacheStrategy> _mockCacheStrategy;
        
        [TestInitialize]
        public void TestInitialize()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"SettingsTestDb_{Guid.NewGuid()}")
                .Options;

            var contextFactory = new Mock<IDbContextFactory<ApplicationDbContext>>();
            contextFactory.Setup(x => x.CreateDbContextAsync(default))
                .ReturnsAsync(() => new ApplicationDbContext(options));
            
            _contextFactory = contextFactory.Object;
            _mockCacheStrategy = new Mock<ICacheStrategy>();
            _repository = new SettingRepository(_contextFactory, _mockCacheStrategy.Object);
            
            // Seed data using a temporary context
            using var context = new ApplicationDbContext(options);
            context.Settings.Add(new Setting 
            { 
                Key = "TestKey1", 
                Value = "TestValue1",
                IsActive = true,
                CreatedBy = "Seeder",
                CreatedOn = DateTime.UtcNow.AddDays(-10)
            });
            
            context.Settings.Add(new Setting 
            { 
                Key = "TestKey2", 
                Value = "TestValue2",
                IsActive = true,
                CreatedBy = "Seeder",
                CreatedOn = DateTime.UtcNow.AddDays(-10)
            });

            context.Settings.Add(new Setting 
            { 
                Key = "InactiveKey", 
                Value = "InactiveValue",
                IsActive = false,
                CreatedBy = "Seeder",
                CreatedOn = DateTime.UtcNow.AddDays(-10)
            });
            
            context.SaveChanges();
        }
        
        [TestMethod]
        public async Task FindAsync_WithKeyPredicate_GeneratesSpecializedCacheKey()
        {
            // Arrange
            var testKey = "TestKey1";
            _mockCacheStrategy.Setup(x => x.GetAsync<System.Collections.Generic.List<Setting>>(It.IsAny<string>()))
                .ReturnsAsync((System.Collections.Generic.List<Setting>)null);
            
            // Act
            var results = await _repository.FindAsync(s => s.Key == testKey);
            
            // Assert
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Count());
            Assert.AreEqual(testKey, results.First().Key);
            
            // Verify that the specialized cache key was used
            _mockCacheStrategy.Verify(
                x => x.GetAsync<System.Collections.Generic.List<Setting>>(
                    $"Setting:Find:Key:{testKey}:IncludeInactive:False"),
                Times.Once);
        }

        [TestMethod]
        public async Task FindAsync_WithKeyPredicate_IncludeInactive_GeneratesCorrectCacheKey()
        {
            // Arrange
            var testKey = "TestKey1";
            _mockCacheStrategy.Setup(x => x.GetAsync<System.Collections.Generic.List<Setting>>(It.IsAny<string>()))
                .ReturnsAsync((System.Collections.Generic.List<Setting>)null);
            
            // Act
            var results = await _repository.FindAsync(s => s.Key == testKey, includeInactive: true);
            
            // Assert
            Assert.IsNotNull(results);
            
            // Verify that the specialized cache key was used with includeInactive: true
            _mockCacheStrategy.Verify(
                x => x.GetAsync<System.Collections.Generic.List<Setting>>(
                    $"Setting:Find:Key:{testKey}:IncludeInactive:True"),
                Times.Once);
        }
        
        [TestMethod]
        public async Task FindAsync_WithGenericPredicate_UsesGenericCacheKey()
        {
            // Arrange
            _mockCacheStrategy.Setup(x => x.GetAsync<System.Collections.Generic.List<Setting>>(It.IsAny<string>()))
                .ReturnsAsync((System.Collections.Generic.List<Setting>)null);
            
            // Act
            var results = await _repository.FindAsync(s => s.Value.Contains("Test"));
            
            // Assert
            Assert.IsNotNull(results);
            
            // Verify that a generic cache key was used (not the specialized Key-based one)
            _mockCacheStrategy.Verify(
                x => x.GetAsync<System.Collections.Generic.List<Setting>>(
                    It.Is<string>(key => key.StartsWith("Setting:Find:") && !key.Contains("Key:"))),
                Times.Once);
        }

        [TestMethod]
        public async Task GetByKeyAsync_ReturnsCorrectSetting()
        {
            // Arrange
            var testKey = "TestKey1";
            
            // Act
            var result = await _repository.GetByKeyAsync(testKey);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(testKey, result.Key);
            Assert.AreEqual("TestValue1", result.Value);
        }

        [TestMethod]
        public async Task GetByKeyAsync_OnlyReturnsActiveSetting()
        {
            // Arrange
            var inactiveKey = "InactiveKey";
            
            // Act
            var result = await _repository.GetByKeyAsync(inactiveKey);
            
            // Assert
            Assert.IsNull(result);
        }
    }
}