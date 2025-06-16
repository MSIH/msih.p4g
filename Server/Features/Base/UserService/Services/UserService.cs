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

        /// <inheritdoc />
        public async Task<User> AddAsync(User user, string createdBy = "System")
        {
            return await _userRepository.AddAsync(user, createdBy);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<User>> GetAllAsync(bool includeInactive = false)
        {
            // Only include deleted if specifically needed elsewhere
            return await _userRepository.GetAllAsync(includeInactive: includeInactive, includeDeleted: false);
        }

        /// <inheritdoc />
        public async Task UpdateAsync(User user, string modifiedBy = "System")
        {
            await _userRepository.UpdateAsync(user, modifiedBy);
        }

        /// <inheritdoc />
        public async Task DeleteAsync(int userId, string modifiedBy = "System")
        {
            await _userRepository.DeleteAsync(userId, modifiedBy);
        }

        /// <inheritdoc />
        public async Task SetActiveAsync(int userId, bool isActive, string modifiedBy = "System")
        {
            await _userRepository.SetActiveStatusAsync(userId, isActive, modifiedBy);
        }
    }
}
