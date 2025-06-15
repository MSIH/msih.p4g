using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.Base.UserService.Data;
using msih.p4g.Server.Features.Base.UserService.Interfaces;
using msih.p4g.Server.Features.Base.UserService.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace msih.p4g.Server.Features.Base.UserService.Repositories
{
    public class UserRepository : GenericRepository<User, UserDbContext>, IUserRepository
    {
        public UserRepository(UserDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
        }
        // All other CRUD methods are inherited from GenericRepository
    }
}
