// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

using msih.p4g.Server.Features.DonorService.Interfaces;
using msih.p4g.Server.Features.DonorService.Model;
using msih.p4g.Shared.Models;

namespace msih.p4g.Server.Features.DonorService.Services
{
    /// <summary>
    /// Service implementation for Donor operations
    /// </summary>
    public class DonorService : IDonorService
    {
        private readonly IDonorRepository _repository;

        /// <summary>
        /// Initializes a new instance of the DonorService class
        /// </summary>
        /// <param name="repository">The donor repository</param>
        public DonorService(IDonorRepository repository)
        {
            _repository = repository;
        }

        /// <inheritdoc />
        public async Task<List<Donor>> GetAllAsync()
        {
            var donors = await _repository.GetAllAsync();
            return donors.ToList();
        }

        /// <inheritdoc />
        public async Task<Donor?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        /// <inheritdoc />
        public async Task<List<Donor>> SearchAsync(string searchTerm)
        {
            return await _repository.SearchAsync(searchTerm);
        }

        /// <inheritdoc />
        public async Task<Donor> AddAsync(Donor donor)
        {
            return await _repository.AddAsync(donor);
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(Donor donor)
        {
            await _repository.UpdateAsync(donor);
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> SetActiveAsync(int id, bool isActive, string modifiedBy = "DonorService")
        {
            return await _repository.SetActiveStatusAsync(id, isActive, modifiedBy);
        }

        /// <inheritdoc />
        public async Task<List<Donor>> GetAllWithUserDataAsync()
        {
            return await _repository.GetAllWithUserDataAsync();
        }

        /// <inheritdoc />
        public async Task<PagedResult<Donor>> GetPaginatedWithUserDataAsync(PaginationParameters paginationParameters)
        {
            return await _repository.GetPaginatedWithUserDataAsync(paginationParameters);
        }
    }
}
