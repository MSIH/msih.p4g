using System.Collections.Generic;
using System.Threading.Tasks;

namespace msih.p4g.Server.Features.DonorService.Interfaces
{
    public interface IDonorService
    {
        Task<List<msih.p4g.Shared.Models.Donor>> GetAllAsync();
        Task<msih.p4g.Shared.Models.Donor?> GetByIdAsync(int id);
        Task<List<msih.p4g.Shared.Models.Donor>> SearchAsync(string searchTerm);
        Task<msih.p4g.Shared.Models.Donor> AddAsync(msih.p4g.Shared.Models.Donor donor);
        Task<bool> UpdateAsync(msih.p4g.Shared.Models.Donor donor);
        Task<bool> DeleteAsync(int id);
    }
}
