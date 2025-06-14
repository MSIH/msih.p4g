using msih.p4g.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace msih.p4g.Server.Features.Base.DonorService.Interfaces
{
    public interface IDonorService
    {
        Task<List<Donor>> GetAllAsync();
        Task<Donor?> GetByIdAsync(int id);
        Task<List<Donor>> SearchAsync(string searchTerm);
        Task<Donor> AddAsync(Donor donor);
        Task<bool> UpdateAsync(Donor donor);
        Task<bool> DeleteAsync(int id);
    }
}
