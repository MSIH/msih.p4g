using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.Base.UserService.Interfaces;
using msih.p4g.Server.Features.Base.UserService.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using msih.p4g.Server.Common.Data;

namespace msih.p4g.Server.Features.Base.UserService.Repositories
{
    public class UserRepository : GenericRepository<User, ApplicationDbContext>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
        }
        // All other CRUD methods are inherited from GenericRepository
    }
}
