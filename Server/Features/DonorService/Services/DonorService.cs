/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

using Microsoft.EntityFrameworkCore;
using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Features.DonorService.Interfaces;
using msih.p4g.Server.Features.DonorService.Model;

namespace msih.p4g.Server.Features.DonorService.Services
{
    public class DonorService : IDonorService
    {
        private readonly ApplicationDbContext _db;
        public DonorService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<Donor>> GetAllAsync()
        {
            return await _db.Donors.ToListAsync();
        }

        public async Task<Donor?> GetByIdAsync(int id)
        {
            return await _db.Donors.FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<List<Donor>> SearchAsync(string searchTerm)
        {
            // Only search by PaymentProcessorDonorId now
            return await _db.Donors
                .Where(d => d.PaymentProcessorDonorId != null && d.PaymentProcessorDonorId.Contains(searchTerm))
                .ToListAsync();
        }

        public async Task<Donor> AddAsync(Donor donor)
        {
            _db.Donors.Add(donor);
            await _db.SaveChangesAsync();
            return donor;
        }

        public async Task<bool> UpdateAsync(Donor donor)
        {
            var existing = await _db.Donors.FirstOrDefaultAsync(d => d.Id == donor.Id);
            if (existing == null) return false;
            _db.Entry(existing).CurrentValues.SetValues(donor);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var donor = await _db.Donors.FirstOrDefaultAsync(d => d.Id == id);
            if (donor == null) return false;
            _db.Donors.Remove(donor);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
