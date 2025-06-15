using msih.p4g.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace msih.p4g.Server.Features.Base.ProfileService.Interfaces
{
    public interface IProfileService
    {
        Task<IEnumerable<Profile>> GetAllAsync(bool includeInactive = false);
        Task<Profile> GetByIdAsync(int id);
        Task<Profile> AddAsync(Profile profile, string createdBy = "System");
        Task<Profile> UpdateAsync(Profile profile, string modifiedBy = "System");
        Task<bool> DeleteAsync(int id, string modifiedBy = "System");
        Task<bool> SetActiveAsync(int id, bool isActive, string modifiedBy = "System");
    }
}
