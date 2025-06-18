// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

using msih.p4g.Server.Features.DonorService.Model;

namespace msih.p4g.Server.Features.DonorService.Interfaces
{
    public interface IDonorService
    {
        Task<List<Donor>> GetAllAsync();
        Task<Donor?> GetByIdAsync(int id);
        Task<List<Donor>> SearchAsync(string searchTerm);
        Task<Donor> AddAsync(Donor donor);
        Task<bool> UpdateAsync(Donor donor);
        Task<bool> SetActiveAsync(int id, bool isActive, string modifiedBy = "System");
    }
}
