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
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Shared.Models;

namespace msih.p4g.Tests.Server.Tests.Common.Data.Repositories
{
    [TestClass]
    public class GenericRepositoryTests
    {
        private TestDbContext _context;
        private GenericRepository<TestEntity, TestDbContext> _repository;
        
        [TestInitialize]
        public void TestInitialize()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;
                
            _context = new TestDbContext(options);
            _repository = new GenericRepository<TestEntity, TestDbContext>(_context);
            
            // Seed data
            _context.TestEntities.Add(new TestEntity 
            { 
                Name = "Active Entity", 
                IsActive = true, 
                IsDeleted = false,
                CreatedOn = DateTime.UtcNow.AddDays(-10),
                CreatedBy = "Seeder"
            });
            
            _context.TestEntities.Add(new TestEntity 
            { 
                Name = "Inactive Entity", 
                IsActive = false, 
                IsDeleted = false,
                CreatedOn = DateTime.UtcNow.AddDays(-10),
                CreatedBy = "Seeder"
            });
            
            _context.TestEntities.Add(new TestEntity 
            { 
                Name = "Deleted Entity", 
                IsActive = true, 
                IsDeleted = true,
                CreatedOn = DateTime.UtcNow.AddDays(-10),
                CreatedBy = "Seeder"
            });
            
            _context.SaveChanges();
        }
        
        [TestMethod]
        public async Task GetAllAsync_FiltersCorrectly()
        {
            // Act - Default behavior (active, not deleted)
            var defaultResult = await _repository.GetAllAsync();
            
            // Act - Include inactive
            var includeInactiveResult = await _repository.GetAllAsync(includeInactive: true);
            
            // Act - Include deleted
            var includeDeletedResult = await _repository.GetAllAsync(includeDeleted: true);
            
            // Act - Include both
            var includeBothResult = await _repository.GetAllAsync(includeInactive: true, includeDeleted: true);
            
            // Assert
            Assert.AreEqual(1, defaultResult.Count());
            Assert.AreEqual(2, includeInactiveResult.Count());
            Assert.AreEqual(2, includeDeletedResult.Count());
            Assert.AreEqual(3, includeBothResult.Count());
        }
        
        [TestMethod]
        public async Task AddAsync_SetsAuditProperties()
        {
            // Arrange
            var entity = new TestEntity { Name = "New Entity" };
            
            // Act
            var result = await _repository.AddAsync(entity, "TestUser");
            
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("TestUser", result.CreatedBy);
            Assert.IsTrue(result.IsActive);
            Assert.IsFalse(result.IsDeleted);
            Assert.IsTrue((DateTime.UtcNow - result.CreatedOn).TotalSeconds < 5);
            Assert.IsNull(result.ModifiedBy);
            Assert.IsNull(result.ModifiedOn);
        }
        
        [TestMethod]
        public async Task UpdateAsync_SetsAuditProperties()
        {
            // Arrange
            var entity = (await _repository.GetActiveOnlyAsync()).First();
            
            // Act
            entity.Name = "Updated Name";
            var result = await _repository.UpdateAsync(entity, "UpdateUser");
            
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Updated Name", result.Name);
            Assert.AreEqual("UpdateUser", result.ModifiedBy);
            Assert.IsTrue((DateTime.UtcNow - result.ModifiedOn.Value).TotalSeconds < 5);
            Assert.AreEqual("Seeder", result.CreatedBy); // Created properties should be preserved
        }
        
        [TestMethod]
        public async Task DeleteAsync_SoftDeletesEntity()
        {
            // Arrange
            var entity = (await _repository.GetActiveOnlyAsync()).First();
            
            // Act
            var result = await _repository.DeleteAsync(entity.Id, "DeleteUser");
            var retrieveResult = await _repository.GetByIdAsync(entity.Id, includeDeleted: true);
            var standardRetrieveResult = await _repository.GetByIdAsync(entity.Id);
            
            // Assert
            Assert.IsTrue(result);
            Assert.IsNotNull(retrieveResult);
            Assert.IsTrue(retrieveResult.IsDeleted);
            Assert.AreEqual("DeleteUser", retrieveResult.ModifiedBy);
            Assert.IsNull(standardRetrieveResult); // Should not be visible in standard query
        }
        
        [TestMethod]
        public async Task RestoreDeletedAsync_RestoresEntity()
        {
            // Arrange
            var deletedId = (await _repository.GetAllAsync(includeInactive: true, includeDeleted: true))
                .First(e => e.IsDeleted).Id;
            
            // Act
            var result = await _repository.RestoreDeletedAsync(deletedId, "RestoreUser");
            var retrieveResult = await _repository.GetByIdAsync(deletedId);
            
            // Assert
            Assert.IsTrue(result);
            Assert.IsNotNull(retrieveResult);
            Assert.IsFalse(retrieveResult.IsDeleted);
            Assert.AreEqual("RestoreUser", retrieveResult.ModifiedBy);
        }
        
        [TestMethod]
        public async Task SetActiveStatusAsync_TogglesActiveStatus()
        {
            // Arrange
            var entity = (await _repository.GetActiveOnlyAsync()).First();
            
            // Act - Deactivate
            var deactivateResult = await _repository.SetActiveStatusAsync(entity.Id, false, "StatusUser");
            var inactiveEntity = await _repository.GetByIdAsync(entity.Id, includeInactive: true);
            
            // Act - Reactivate
            var reactivateResult = await _repository.SetActiveStatusAsync(entity.Id, true, "StatusUser");
            var activeEntity = await _repository.GetByIdAsync(entity.Id);
            
            // Assert
            Assert.IsTrue(deactivateResult);
            Assert.IsTrue(reactivateResult);
            Assert.IsNotNull(inactiveEntity);
            Assert.IsNotNull(activeEntity);
            Assert.IsFalse(inactiveEntity.IsActive);
            Assert.IsTrue(activeEntity.IsActive);
        }
    }
    
    // Test entity for testing the repository
    public class TestEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
    }
    
    // Test DbContext for testing the repository
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }
        
        public DbSet<TestEntity> TestEntities { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<TestEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            });
        }
    }
}
