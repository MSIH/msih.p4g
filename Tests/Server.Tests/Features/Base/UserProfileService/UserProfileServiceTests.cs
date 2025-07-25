using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MSIH.Core.Services.Profile.Interfaces;
using MSIH.Core.Services.Profile.Model;
using MSIH.Core.Services.User.Interfaces;
using MSIH.Core.Services.User.Models;
using MSIH.Core.Services.UserProfile.Services;
using System.Threading.Tasks;
using MSIH.Core.Services.UserProfile.Interfaces;

namespace Tests.Server.Tests.Features.Base.UserProfileService
{
    [TestClass]
    public class UserProfileServiceTests
    {
        private Mock<IUserService> _mockUserService;
        private Mock<IProfileService> _mockProfileService;
        private MSIH.Core.Services.UserProfile.Services.UserProfileService _userProfileService;

        [TestInitialize]
        public void Setup()
        {
            _mockUserService = new Mock<IUserService>();
            _mockProfileService = new Mock<IProfileService>();

            _userProfileService = new MSIH.Core.Services.UserProfile.Services.UserProfileService(_mockUserService.Object, _mockProfileService.Object);
        }

        [TestMethod]
        public async Task CreateUserWithProfileAsync_ValidUserAndProfile_SetsUserIdAndCallsServices()
        {
            // Arrange
            var user = new User { Email = "test@example.com", Role = UserRole.Donor };
            var profile = new Profile { FirstName = "Test", LastName = "User" };

            var createdUser = new User { Id = 123, Email = "test@example.com", Role = UserRole.Donor };
            var expectedProfile = new Profile
            {
                Id = 456,
                UserId = 123,
                FirstName = "Test",
                LastName = "User",
                ReferralCode = "ABC123"
            };

            _mockUserService.Setup(s => s.AddAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(createdUser);

            _mockProfileService.Setup(s => s.AddAsync(It.IsAny<Profile>(), It.IsAny<string>()))
                .ReturnsAsync(expectedProfile);

            // Act
            var result = await _userProfileService.CreateUserWithProfileAsync(user, profile);

            // Assert
            Assert.AreEqual(expectedProfile, result);

            // Verify the profile's UserId was set correctly
            _mockProfileService.Verify(s => s.AddAsync(
                It.Is<Profile>(p => p.UserId == createdUser.Id),
                It.IsAny<string>()
            ));

            // Verify the services were called in the correct order
            _mockUserService.Verify(s => s.AddAsync(user, It.IsAny<string>()), Times.Once);
            _mockProfileService.Verify(s => s.AddAsync(It.IsAny<Profile>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task GetProfileByUserEmailAsync_UserFound_ReturnsProfile()
        {
            // Arrange
            var email = "test@example.com";
            var user = new User { Id = 123, Email = email };
            var expectedProfile = new Profile { Id = 456, UserId = 123 };

            _mockUserService.Setup(s => s.GetByEmailAsync(email))
                .ReturnsAsync(user);

            _mockProfileService.Setup(s => s.GetByIdAsync(user.Id))
                .ReturnsAsync(expectedProfile);

            // Act
            var result = await _userProfileService.GetProfileByUserEmailAsync(email);

            // Assert
            Assert.AreEqual(expectedProfile, result);
            _mockUserService.Verify(s => s.GetByEmailAsync(email), Times.Once);
            _mockProfileService.Verify(s => s.GetByIdAsync(user.Id), Times.Once);
        }

        [TestMethod]
        public async Task GetProfileByUserEmailAsync_UserNotFound_ReturnsNull()
        {
            // Arrange
            var email = "notfound@example.com";

            _mockUserService.Setup(s => s.GetByEmailAsync(email))
                .ReturnsAsync((User)null);

            // Act
            var result = await _userProfileService.GetProfileByUserEmailAsync(email);

            // Assert
            Assert.IsNull(result);
            _mockUserService.Verify(s => s.GetByEmailAsync(email), Times.Once);
            _mockProfileService.Verify(s => s.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }
    }
}
