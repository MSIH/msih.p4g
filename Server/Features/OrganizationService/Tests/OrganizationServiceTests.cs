/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Xunit;
using Microsoft.EntityFrameworkCore;
using msih.p4g.Server.Features.OrganizationService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace msih.p4g.Server.Features.OrganizationService.Tests
{
    /// <summary>
    /// Unit tests for OrganizationService
    /// </summary>
    public class OrganizationServiceTests
    {
        /// <summary>
        /// Tests that a new organization can be added
        /// </summary>
        [Fact]
        public async Task AddAsync_ValidOrganization_ShouldAddOrganization()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"OrganizationService_AddAsync_{Guid.NewGuid()}")
                .Options;
            
            using var dbContext = new ApplicationDbContext(dbContextOptions);
            var repository = new OrganizationRepository(dbContext);
            var service = new Services.OrganizationService(repository);
            
            var organization = new Organization
            {
                LegalName = "Test Non-Profit",
                TaxId = "12-3456789",
                EmailAddress = "test@example.org",
                IsActive = true
            };
            
            // Act
            var result = await service.AddAsync(organization);
            
            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(0, result.Id);
            Assert.Equal("Test Non-Profit", result.LegalName);
            Assert.Equal("12-3456789", result.TaxId);
            
            // Verify the organization was added to the database
            var fromDb = await dbContext.Organizations.FindAsync(result.Id);
            Assert.NotNull(fromDb);
            Assert.Equal("Test Non-Profit", fromDb.LegalName);
        }
        
        /// <summary>
        /// Tests that an organization with a duplicate tax ID cannot be added
        /// </summary>
        [Fact]
        public async Task AddAsync_DuplicateTaxId_ShouldThrowException()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"OrganizationService_AddAsync_Duplicate_{Guid.NewGuid()}")
                .Options;
            
            using var dbContext = new ApplicationDbContext(dbContextOptions);
            var repository = new OrganizationRepository(dbContext);
            var service = new Services.OrganizationService(repository);
            
            var organization1 = new Organization
            {
                LegalName = "First Non-Profit",
                TaxId = "12-3456789",
                EmailAddress = "first@example.org",
                IsActive = true
            };
            
            var organization2 = new Organization
            {
                LegalName = "Second Non-Profit",
                TaxId = "12-3456789", // Same tax ID
                EmailAddress = "second@example.org",
                IsActive = true
            };
            
            // Act & Assert
            await service.AddAsync(organization1);
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddAsync(organization2));
        }
        
        /// <summary>
        /// Tests that all organizations can be retrieved
        /// </summary>
        [Fact]
        public async Task GetAllAsync_ShouldReturnAllActiveOrganizations()
        {
            // Arrange
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"OrganizationService_GetAllAsync_{Guid.NewGuid()}")
                .Options;
            
            using var dbContext = new ApplicationDbContext(dbContextOptions);
            var repository = new OrganizationRepository(dbContext);
            var service = new Services.OrganizationService(repository);
            
            // Add some organizations
            dbContext.Organizations.AddRange(
                new Organization
                {
                    LegalName = "Active Org 1",
                    TaxId = "12-3456789",
                    EmailAddress = "active1@example.org",
                    IsActive = true
                },
                new Organization
                {
                    LegalName = "Active Org 2",
                    TaxId = "98-7654321",
                    EmailAddress = "active2@example.org",
                    IsActive = true
                },
                new Organization
                {
                    LegalName = "Inactive Org",
                    TaxId = "45-6789123",
                    EmailAddress = "inactive@example.org",
                    IsActive = false
                }
            );
            await dbContext.SaveChangesAsync();
            
            // Act
            var activeOrgs = (await service.GetAllAsync(includeInactive: false)).ToList();
            var allOrgs = (await service.GetAllAsync(includeInactive: true)).ToList();
            
            // Assert
            Assert.Equal(2, activeOrgs.Count);
            Assert.Equal(3, allOrgs.Count);
            Assert.All(activeOrgs, org => Assert.True(org.IsActive));
            Assert.Contains(allOrgs, org => !org.IsActive);
        }
    }
}
