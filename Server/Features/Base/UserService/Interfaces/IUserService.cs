using System.Collections.Generic;
using System.Threading.Tasks;
using msih.p4g.Server.Features.Base.UserService.Models;

namespace msih.p4g.Server.Features.Base.UserService.Interfaces
{
    /// <summary>
    /// Interface for managing user operations
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Retrieves a user by their email address
        /// </summary>
        /// <param name="email">The email address to search for</param>
        /// <returns>The found user or null if not found</returns>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>
        /// Creates a new user in the system
        /// </summary>
        /// <param name="user">The user to create</param>
        /// <param name="createdBy">Who created the user</param>
        /// <returns>The created user with Id assigned</returns>
        Task<User> AddAsync(User user, string createdBy = "System");

        /// <summary>
        /// Gets all active users in the system
        /// </summary>
        /// <param name="includeInactive">Whether to include inactive users</param>
        /// <returns>A collection of users</returns>
        Task<IEnumerable<User>> GetAllAsync(bool includeInactive = false);

        /// <summary>
        /// Updates an existing user
        /// </summary>
        /// <param name="user">The user with updated information</param>
        /// <param name="modifiedBy">Who modified the user</param>
        Task UpdateAsync(User user, string modifiedBy = "System");

        /// <summary>
        /// Deletes a user (soft delete)
        /// </summary>
        /// <param name="userId">The ID of the user to delete</param>
        /// <param name="modifiedBy">Who deleted the user</param>
        Task DeleteAsync(int userId, string modifiedBy = "System");

        /// <summary>
        /// Sets the active status of a user
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <param name="isActive">The new active status</param>
        /// <param name="modifiedBy">Who changed the status</param>
        Task SetActiveAsync(int userId, bool isActive, string modifiedBy = "System");
    }
}
