using msih.p4g.Shared.Models;
using msih.p4g.Server.Features.Campaign.Data;
using msih.p4g.Server.Features.Base.DonorService.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace msih.p4g.Server.Features.Base.DonorService.Services
{
    public class DonorService : IDonorService
    {
        private readonly CampaignDbContext _db;
        public DonorService(CampaignDbContext db)
        {
            _db = db;
        }

        public async Task<List<Donor>> GetAllAsync()
        {
            return await _db.Donors.Where(d => !d.IsDeleted).ToListAsync();
        }

        public async Task<Donor?> GetByIdAsync(int id)
        {
            return await _db.Donors.FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
        }

        public async Task<List<Donor>> SearchAsync(string searchTerm)
        {
            return await _db.Donors
                .Where(d => !d.IsDeleted && (
                    d.FirstName.Contains(searchTerm) ||
                    d.LastName.Contains(searchTerm) ||
                    d.EmailAddress.Contains(searchTerm) ||
                    d.MobileNumber.Contains(searchTerm)
                ))
                .ToListAsync();
        }

        public async Task<Donor> AddAsync(Donor donor)
        {
            if (string.IsNullOrWhiteSpace(donor.DonorId))
            {
                donor.DonorId = Guid.NewGuid().ToString();
            }
            _db.Donors.Add(donor);
            await _db.SaveChangesAsync();
            return donor;
        }

        public async Task<bool> UpdateAsync(Donor donor)
        {
            var existing = await _db.Donors.FirstOrDefaultAsync(d => d.Id == donor.Id && !d.IsDeleted);
            if (existing == null) return false;
            _db.Entry(existing).CurrentValues.SetValues(donor);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var donor = await _db.Donors.FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
            if (donor == null) return false;
            donor.IsDeleted = true;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
