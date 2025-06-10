using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using msih.p4g.Server.Features.Base.SmsService.Data;
using msih.p4g.Server.Features.Base.SmsService.Services;
using msih.p4g.Shared.Models;

namespace msih.p4g.Tests.Server.Tests.Features.Base.SmsService
{
    [TestClass]
    public class ValidatedPhoneNumberRepositoryTests
    {
        private SmsDbContext _context;
        private ValidatedPhoneNumberRepository _repository;
        
        [TestInitialize]
        public void TestInitialize()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<SmsDbContext>()
                .UseInMemoryDatabase(databaseName: $"SmsTestDb_{Guid.NewGuid()}")
                .Options;
                
            _context = new SmsDbContext(options);
            _repository = new ValidatedPhoneNumberRepository(_context);
            
            // Seed data
            _context.ValidatedPhoneNumbers.Add(new ValidatedPhoneNumber 
            { 
                PhoneNumber = "+12125551234", 
                IsValid = true,
                IsMobile = true,
                Carrier = "Test Carrier",
                CountryCode = "US",
                ValidatedOn = DateTime.UtcNow.AddDays(-10),
                IsActive = true,
                IsDeleted = false,
                CreatedBy = "Seeder"
            });
            
            _context.ValidatedPhoneNumbers.Add(new ValidatedPhoneNumber 
            { 
                PhoneNumber = "+12125559876", 
                IsValid = true,
                IsMobile = false,
                Carrier = "Landline Carrier",
                CountryCode = "US",
                ValidatedOn = DateTime.UtcNow.AddDays(-5),
                IsActive = false,
                IsDeleted = false,
                CreatedBy = "Seeder"
            });
            
            _context.ValidatedPhoneNumbers.Add(new ValidatedPhoneNumber 
            { 
                PhoneNumber = "+12125550000", 
                IsValid = false,
                CountryCode = "US",
                ValidatedOn = DateTime.UtcNow.AddDays(-2),
                IsActive = true,
                IsDeleted = true,
                CreatedBy = "Seeder"
            });
            
            _context.SaveChanges();
        }
        
        [TestMethod]
        public async Task GetByPhoneNumberAsync_ReturnsCorrectPhoneNumber()
        {
            // Arrange
            string phoneNumber = "+12125551234";
            
            // Act
            var result = await _repository.GetByPhoneNumberAsync(phoneNumber);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(phoneNumber, result.PhoneNumber);
            Assert.IsTrue(result.IsMobile);
            Assert.AreEqual("Test Carrier", result.Carrier);
        }
        
        [TestMethod]
        public async Task GetByPhoneNumberAsync_DoesNotReturnDeletedPhoneNumbers()
        {
            // Arrange
            string phoneNumber = "+12125550000";
            
            // Act
            var result = await _repository.GetByPhoneNumberAsync(phoneNumber);
            
            // Assert
            Assert.IsNull(result);
        }
        
        [TestMethod]
        public async Task AddOrUpdateAsync_AddNewPhoneNumber()
        {
            // Arrange
            var newPhoneNumber = new ValidatedPhoneNumber 
            { 
                PhoneNumber = "+12125557777", 
                IsValid = true,
                IsMobile = true,
                Carrier = "New Carrier",
                CountryCode = "US",
                ValidatedOn = DateTime.UtcNow,
                CreatedBy = "Test"
            };
            
            // Act
            var result = await _repository.AddOrUpdateAsync(newPhoneNumber);
            var retrievedPhone = await _repository.GetByPhoneNumberAsync("+12125557777");
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(retrievedPhone);
            Assert.AreEqual("+12125557777", retrievedPhone.PhoneNumber);
            Assert.IsTrue(retrievedPhone.IsValid);
            Assert.IsTrue(retrievedPhone.IsMobile);
            Assert.AreEqual("New Carrier", retrievedPhone.Carrier);
            Assert.IsTrue(retrievedPhone.IsActive);
            Assert.IsFalse(retrievedPhone.IsDeleted);
        }
        
        [TestMethod]
        public async Task AddOrUpdateAsync_UpdatesExistingPhoneNumber()
        {
            // Arrange
            var existingPhoneNumber = "+12125551234";
            var updatedValidation = new ValidatedPhoneNumber 
            { 
                PhoneNumber = existingPhoneNumber, 
                IsValid = true,
                IsMobile = false,
                Carrier = "Updated Carrier",
                CountryCode = "US",
                ValidatedOn = DateTime.UtcNow,
                CreatedBy = "Test"
            };
            
            // Act
            var result = await _repository.AddOrUpdateAsync(updatedValidation);
            var retrievedPhone = await _repository.GetByPhoneNumberAsync(existingPhoneNumber);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(retrievedPhone);
            Assert.AreEqual(existingPhoneNumber, retrievedPhone.PhoneNumber);
            Assert.IsFalse(retrievedPhone.IsMobile);
            Assert.AreEqual("Updated Carrier", retrievedPhone.Carrier);
            Assert.AreEqual("Seeder", retrievedPhone.CreatedBy); // Original creator preserved
            Assert.IsNotNull(retrievedPhone.ModifiedBy); // Modified by was set
            Assert.IsNotNull(retrievedPhone.ModifiedOn); // Modified date was set
        }
        
        [TestMethod]
        public async Task GetAllAsync_FiltersCorrectly()
        {
            // Act - Get only active records (not deleted)
            var activeResults = await ((Server.Common.Data.Repositories.IGenericRepository<ValidatedPhoneNumber>)_repository)
                .GetAllAsync(includeInactive: false, includeDeleted: false);
                
            // Act - Include inactive records (not deleted)
            var includeInactiveResults = await ((Server.Common.Data.Repositories.IGenericRepository<ValidatedPhoneNumber>)_repository)
                .GetAllAsync(includeInactive: true, includeDeleted: false);
                
            // Act - Include deleted records
            var includeDeletedResults = await ((Server.Common.Data.Repositories.IGenericRepository<ValidatedPhoneNumber>)_repository)
                .GetAllAsync(includeInactive: false, includeDeleted: true);
                
            // Act - Include both inactive and deleted
            var includeAllResults = await ((Server.Common.Data.Repositories.IGenericRepository<ValidatedPhoneNumber>)_repository)
                .GetAllAsync(includeInactive: true, includeDeleted: true);
            
            // Assert
            Assert.AreEqual(1, activeResults.Count());
            Assert.AreEqual(2, includeInactiveResults.Count());
            Assert.AreEqual(2, includeDeletedResults.Count());
            Assert.AreEqual(3, includeAllResults.Count());
        }
        
        [TestMethod]
        public async Task SoftDelete_WorksCorrectly()
        {
            // Arrange
            var phoneNumber = "+12125551234";
            var phoneToDelete = await _repository.GetByPhoneNumberAsync(phoneNumber);
            
            // Act - Delete
            var deleteResult = await ((Server.Common.Data.Repositories.IGenericRepository<ValidatedPhoneNumber>)_repository)
                .DeleteAsync(phoneToDelete.Id, "DeleteUser");
                
            // Try to get the deleted record
            var afterDeleteResult = await _repository.GetByPhoneNumberAsync(phoneNumber);
            
            // Get with includeDeleted flag
            var withDeletedResult = await ((Server.Common.Data.Repositories.IGenericRepository<ValidatedPhoneNumber>)_repository)
                .GetByIdAsync(phoneToDelete.Id, includeInactive: true, includeDeleted: true);
            
            // Assert
            Assert.IsTrue(deleteResult);
            Assert.IsNull(afterDeleteResult); // Should not be returned in normal query
            Assert.IsNotNull(withDeletedResult); // Should be returned when includeDeleted is true
            Assert.IsTrue(withDeletedResult.IsDeleted);
            Assert.AreEqual("DeleteUser", withDeletedResult.ModifiedBy);
        }
    }
}
