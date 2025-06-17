using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using msih.p4g.Server.Features.FundraiserService.Model;
using msih.p4g.Server.Common.Data;

namespace msih.p4g.Server.Features.FundraiserService.Repositories
{
    public class FundraiserRepository
    {
        private readonly ApplicationDbContext _context;
        public FundraiserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Fundraiser?> GetByIdAsync(int id)
        {
            return await _context.Set<Fundraiser>().FindAsync(id);
        }

        public async Task<IEnumerable<Fundraiser>> GetAllAsync()
        {
            return await _context.Set<Fundraiser>().ToListAsync();
        }

        public async Task<Fundraiser> AddAsync(Fundraiser fundraiser)
        {
            _context.Set<Fundraiser>().Add(fundraiser);
            await _context.SaveChangesAsync();
            return fundraiser;
        }

        public async Task UpdateAsync(Fundraiser fundraiser)
        {
            _context.Set<Fundraiser>().Update(fundraiser);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _context.Set<Fundraiser>().Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
