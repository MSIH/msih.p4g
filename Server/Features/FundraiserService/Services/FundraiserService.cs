using System.Collections.Generic;
using System.Threading.Tasks;
using msih.p4g.Server.Features.FundraiserService.Model;
using msih.p4g.Server.Features.FundraiserService.Interfaces;
using msih.p4g.Server.Features.FundraiserService.Repositories;

namespace msih.p4g.Server.Features.FundraiserService.Services
{
    public class FundraiserService : IFundraiserService
    {
        private readonly FundraiserRepository _repository;
        public FundraiserService(FundraiserRepository repository)
        {
            _repository = repository;
        }

        public async Task<Fundraiser?> GetByIdAsync(int id) => await _repository.GetByIdAsync(id);
        public async Task<IEnumerable<Fundraiser>> GetAllAsync() => await _repository.GetAllAsync();
        public async Task<Fundraiser> AddAsync(Fundraiser fundraiser) => await _repository.AddAsync(fundraiser);
        public async Task UpdateAsync(Fundraiser fundraiser) => await _repository.UpdateAsync(fundraiser);
        public async Task DeleteAsync(int id) => await _repository.DeleteAsync(id);
    }
}
