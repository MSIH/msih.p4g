using System.Collections.Generic;
using System.Threading.Tasks;
using msih.p4g.Server.Features.FundraiserService.Model;

namespace msih.p4g.Server.Features.FundraiserService.Interfaces
{
    public interface IFundraiserService
    {
        Task<Fundraiser?> GetByIdAsync(int id);
        Task<Fundraiser?> GetByUserIdAsync(int userId);
        Task<IEnumerable<Fundraiser>> GetAllAsync();
        Task<Fundraiser> AddAsync(Fundraiser fundraiser);
        Task UpdateAsync(Fundraiser fundraiser);
        Task<bool> SetActiveAsync(int id, bool isActive, string modifiedBy = "System");
    }
}
