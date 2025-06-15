using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.Base.UserService.Models;
using System.Threading.Tasks;

namespace msih.p4g.Server.Features.Base.UserService.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        // Add any additional custom methods for User here
    }
}
