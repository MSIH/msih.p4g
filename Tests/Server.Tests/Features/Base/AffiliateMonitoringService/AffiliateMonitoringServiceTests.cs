// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Features.Base.AffiliateMonitoringService.Services;
using msih.p4g.Server.Features.Base.EmailService.Interfaces;
using msih.p4g.Server.Features.Base.ProfileService.Interfaces;
using msih.p4g.Server.Features.Base.ProfileService.Model;
using msih.p4g.Server.Features.Base.UserService.Interfaces;
using msih.p4g.Server.Features.Base.UserService.Models;
using msih.p4g.Server.Features.DonationService.Models;
using msih.p4g.Server.Features.DonorService.Model;
using msih.p4g.Server.Features.FundraiserService.Interfaces;
using msih.p4g.Server.Features.FundraiserService.Model;

namespace Tests.Server.Tests.Features.Base.AffiliateMonitoringService
{
    [TestClass]
    public class AffiliateMonitoringServiceTests
    {
        private ApplicationDbContext _context;
        private Mock<IProfileService> _mockProfileService;
        private Mock<IFundraiserService> _mockFundraiserService;
        private Mock<IUserService> _mockUserService;
        private Mock<IEmailService> _mockEmailService;
        private Mock<ILogger<msih.p4g.Server.Features.Base.AffiliateMonitoringService.Services.AffiliateMonitoringService>> _mockLogger;
        private msih.p4g.Server.Features.Base.AffiliateMonitoringService.Services.AffiliateMonitoringService _service;

        [TestInitialize]
        public void TestInitialize()
        {
            // Setup in-memory database for testing
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _mockProfileService = new Mock<IProfileService>();
            _mockFundraiserService = new Mock<IFundraiserService>();
            _mockUserService = new Mock<IUserService>();
            _mockEmailService = new Mock<IEmailService>();
            _mockLogger = new Mock<ILogger<msih.p4g.Server.Features.Base.AffiliateMonitoringService.Services.AffiliateMonitoringService>>();

            _service = new msih.p4g.Server.Features.Base.AffiliateMonitoringService.Services.AffiliateMonitoringService(
                _context,
                _mockProfileService.Object,
                _mockFundraiserService.Object,
                _mockUserService.Object,
                _mockEmailService.Object,
                _mockLogger.Object
            );
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _context.Dispose();
        }

        [TestMethod]
        public async Task CountUnqualifiedAccountsAsync_WithUnqualifiedDonors_ReturnsCorrectCount()
        {
            // Arrange
            var referralCode = "TEST123";
            var donors = new[]
            {
                new Donor { Id = 1, UserId = 1, ReferralCode = referralCode, IsActive = true },
                new Donor { Id = 2, UserId = 2, ReferralCode = referralCode, IsActive = true },
                new Donor { Id = 3, UserId = 3, ReferralCode = referralCode, IsActive = true }
            };

            // Add one donation to the first donor (making them qualified)
            var donation = new Donation { Id = 1, DonorId = 1, DonationAmount = 100 };
            donors[0].Donations.Add(donation);

            _context.Donors.AddRange(donors);
            _context.Donations.Add(donation);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.CountUnqualifiedAccountsAsync(referralCode);

            // Assert
            Assert.AreEqual(2, result); // 2 donors without donations
        }

        [TestMethod]
        public async Task SuspendAffiliateAsync_ValidFundraiser_ReturnsTrueAndUpdateFundraiser()
        {
            // Arrange
            var fundraiser = new Fundraiser 
            { 
                Id = 1, 
                UserId = 1,
                IsSuspended = false
            };
            var reason = "Test suspension";

            _mockFundraiserService.Setup(x => x.UpdateAsync(It.IsAny<Fundraiser>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.SuspendAffiliateAsync(fundraiser, reason);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(fundraiser.IsSuspended);
            Assert.AreEqual(reason, fundraiser.SuspensionReason);
            Assert.IsNotNull(fundraiser.SuspendedDate);
            _mockFundraiserService.Verify(x => x.UpdateAsync(fundraiser), Times.Once);
        }

        [TestMethod]
        public async Task CheckAffiliateAfterDonorCreationAsync_FirstTwoAccountsUnqualified_SuspendsAffiliate()
        {
            // Arrange
            var referralCode = "TEST123";
            var user = new User { Id = 1, Email = "test@example.com" };
            var profile = new Profile 
            { 
                Id = 1, 
                UserId = 1, 
                ReferralCode = referralCode, 
                IsActive = true,
                FirstName = "Test",
                LastName = "User",
                User = user
            };
            var fundraiser = new Fundraiser
            {
                Id = 1,
                UserId = 1,
                IsSuspended = false
            };

            // Create 2 unqualified donors
            var donors = new[]
            {
                new Donor { Id = 1, UserId = 1, ReferralCode = referralCode, IsActive = true },
                new Donor { Id = 2, UserId = 2, ReferralCode = referralCode, IsActive = true }
            };

            _context.Profiles.Add(profile);
            _context.Users.Add(user);
            _context.Donors.AddRange(donors);
            await _context.SaveChangesAsync();
            
            _mockProfileService.Setup(x => x.GetByReferralCodeAsync(referralCode))
                .ReturnsAsync(profile);
            _mockFundraiserService.Setup(x => x.GetByUserIdAsync(1))
                .ReturnsAsync(fundraiser);
            _mockFundraiserService.Setup(x => x.UpdateAsync(It.IsAny<Fundraiser>()))
                .Returns(Task.CompletedTask);
            _mockEmailService.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CheckAffiliateAfterDonorCreationAsync(referralCode);

            // Assert
            Assert.IsTrue(result);
            _mockFundraiserService.Verify(x => x.UpdateAsync(It.IsAny<Fundraiser>()), Times.Once);
            _mockEmailService.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task CheckAffiliateAfterDonorCreationAsync_MoreThanNineUnqualified_SuspendsAffiliate()
        {
            // Arrange
            var referralCode = "TEST123";
            var user = new User { Id = 1, Email = "test@example.com" };
            var profile = new Profile 
            { 
                Id = 1, 
                UserId = 1, 
                ReferralCode = referralCode, 
                IsActive = true,
                FirstName = "Test",
                LastName = "User",
                User = user
            };
            var fundraiser = new Fundraiser
            {
                Id = 1,
                UserId = 1,
                IsSuspended = false
            };

            // Create 10 unqualified donors
            var donors = new List<Donor>();
            for (int i = 1; i <= 10; i++)
            {
                donors.Add(new Donor { Id = i, UserId = i, ReferralCode = referralCode, IsActive = true });
            }

            _context.Profiles.Add(profile);
            _context.Users.Add(user);
            _context.Donors.AddRange(donors);
            await _context.SaveChangesAsync();
            
            _mockProfileService.Setup(x => x.GetByReferralCodeAsync(referralCode))
                .ReturnsAsync(profile);
            _mockFundraiserService.Setup(x => x.GetByUserIdAsync(1))
                .ReturnsAsync(fundraiser);
            _mockFundraiserService.Setup(x => x.UpdateAsync(It.IsAny<Fundraiser>()))
                .Returns(Task.CompletedTask);
            _mockEmailService.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CheckAffiliateAfterDonorCreationAsync(referralCode);

            // Assert
            Assert.IsTrue(result);
            _mockFundraiserService.Verify(x => x.UpdateAsync(It.IsAny<Fundraiser>()), Times.Once);
            _mockEmailService.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task CheckAffiliateAfterDonorCreationAsync_QualifiedAccounts_DoesNotSuspend()
        {
            // Arrange
            var referralCode = "TEST123";
            var user = new User { Id = 1, Email = "test@example.com" };
            var profile = new Profile 
            { 
                Id = 1, 
                UserId = 1, 
                ReferralCode = referralCode, 
                IsActive = true,
                FirstName = "Test",
                LastName = "User",
                User = user
            };
            var fundraiser = new Fundraiser
            {
                Id = 1,
                UserId = 1,
                IsSuspended = false
            };

            // Create 2 qualified donors (with donations)
            var donors = new[]
            {
                new Donor { Id = 1, UserId = 1, ReferralCode = referralCode, IsActive = true },
                new Donor { Id = 2, UserId = 2, ReferralCode = referralCode, IsActive = true }
            };

            var donations = new[]
            {
                new Donation { Id = 1, DonorId = 1, DonationAmount = 100 },
                new Donation { Id = 2, DonorId = 2, DonationAmount = 50 }
            };

            donors[0].Donations.Add(donations[0]);
            donors[1].Donations.Add(donations[1]);

            _context.Profiles.Add(profile);
            _context.Users.Add(user);
            _context.Donors.AddRange(donors);
            _context.Donations.AddRange(donations);
            await _context.SaveChangesAsync();
            
            _mockProfileService.Setup(x => x.GetByReferralCodeAsync(referralCode))
                .ReturnsAsync(profile);
            _mockFundraiserService.Setup(x => x.GetByUserIdAsync(1))
                .ReturnsAsync(fundraiser);

            // Act
            var result = await _service.CheckAffiliateAfterDonorCreationAsync(referralCode);

            // Assert
            Assert.IsFalse(result);
            _mockFundraiserService.Verify(x => x.UpdateAsync(It.IsAny<Fundraiser>()), Times.Never);
            _mockEmailService.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}