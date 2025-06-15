using System.Collections.Generic;
using System.Threading.Tasks;
using msih.p4g.Server.Features.Base.UserService.Models;
using msih.p4g.Server.Features.Base.UserService.Interfaces;

namespace msih.p4g.Server.Features.Base.UserService.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _userRepository.GetByEmailAsync(email);
        }

        public async Task<IEnumerable<User>> GetAllAsync(bool includeInactive = false)
        {
            // Only include deleted if specifically needed elsewhere
            return await _userRepository.GetAllAsync(includeInactive: includeInactive, includeDeleted: false);
        }

        public async Task UpdateAsync(User user, string modifiedBy = "System")
        {
            await _userRepository.UpdateAsync(user, modifiedBy);
        }

        public async Task DeleteAsync(int userId, string modifiedBy = "System")
        {
            await _userRepository.DeleteAsync(userId, modifiedBy);
        }

        public async Task SetActiveAsync(int userId, bool isActive, string modifiedBy = "System")
        {
            await _userRepository.SetActiveStatusAsync(userId, isActive, modifiedBy);
        }
    }
}
