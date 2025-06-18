using System.Collections.Generic;
using System.Threading.Tasks;
using msih.p4g.Server.Features.Base.UserService.Models;
using msih.p4g.Server.Features.Base.UserService.Interfaces;

namespace msih.p4g.Server.Features.Base.UserService.Services
{
    /// <summary>
    /// Service for managing user operations
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <inheritdoc />
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _userRepository.GetByEmailAsync(email);
        }

        /// <summary>
        /// Gets a user by their ID
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve</param>
        /// <returns>The user if found, otherwise null</returns>
        public async Task<User?> GetByIdAsync(int userId)
        {
            return await _userRepository.GetByIdAsync(userId);
        }

        /// <inheritdoc />
        public async Task<User> AddAsync(User user, string createdBy = "System")
        {
            return await _userRepository.AddAsync(user, createdBy);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<User>> GetAllAsync(bool includeInactive = false)
        {
            return await _userRepository.GetAllAsync(includeInactive: includeInactive);
        }

        /// <inheritdoc />
        public async Task UpdateAsync(User user, string modifiedBy = "System")
        {
            await _userRepository.UpdateAsync(user, modifiedBy);
        }

        /// <inheritdoc />
        public async Task SetActiveAsync(int userId, bool isActive, string modifiedBy = "System")
        {
            await _userRepository.SetActiveStatusAsync(userId, isActive, modifiedBy);
        }
    }
}
