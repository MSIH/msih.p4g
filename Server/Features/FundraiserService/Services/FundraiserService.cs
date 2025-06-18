/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using System.Collections.Generic;
using System.Threading.Tasks;
using msih.p4g.Server.Features.FundraiserService.Model;
using msih.p4g.Server.Features.FundraiserService.Interfaces;

namespace msih.p4g.Server.Features.FundraiserService.Services
{
    /// <summary>
    /// Service implementation for Fundraiser operations
    /// </summary>
    public class FundraiserService : IFundraiserService
    {
        private readonly IFundraiserRepository _repository;
        
        /// <summary>
        /// Initializes a new instance of the FundraiserService class
        /// </summary>
        /// <param name="repository">The fundraiser repository</param>
        public FundraiserService(IFundraiserRepository repository)
        {
            _repository = repository;
        }

        /// <inheritdoc />
        public async Task<Fundraiser?> GetByIdAsync(int id) => 
            await _repository.GetByIdAsync(id);
            
        /// <inheritdoc />
        public async Task<Fundraiser?> GetByUserIdAsync(int userId) => 
            await _repository.GetByUserIdAsync(userId);
            
        /// <inheritdoc />
        public async Task<IEnumerable<Fundraiser>> GetAllAsync() => 
            await _repository.GetAllAsync();
            
        /// <inheritdoc />
        public async Task<Fundraiser> AddAsync(Fundraiser fundraiser) => 
            await _repository.AddAsync(fundraiser);
            
        /// <inheritdoc />
        public async Task UpdateAsync(Fundraiser fundraiser) => 
            await _repository.UpdateAsync(fundraiser);

        /// <inheritdoc />
        public async Task<bool> SetActiveAsync(int id, bool isActive, string modifiedBy = "System") =>
            await _repository.SetActiveStatusAsync(id, isActive, modifiedBy);
    }
}
