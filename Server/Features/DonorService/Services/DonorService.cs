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
            return await _db.Donors.Where(d => !d.IsDeleted).ToListAsync();
        }

        public async Task<Donor?> GetByIdAsync(int id)
        {
            return await _db.Donors.FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
        }

        public async Task<List<Donor>> SearchAsync(string searchTerm)
        {
            // Only search by DonorId or PaymentProcessorDonorId now
            return await _db.Donors
                .Where(d => !d.IsDeleted && (
                    d.DonorId.Contains(searchTerm) ||
                    (d.PaymentProcessorDonorId != null && d.PaymentProcessorDonorId.Contains(searchTerm))
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
