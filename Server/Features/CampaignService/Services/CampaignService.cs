/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using msih.p4g.Server.Features.CampaignService.Interfaces;
using msih.p4g.Server.Features.CampaignService.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace msih.p4g.Server.Features.CampaignService.Services
{
    /// <summary>
    /// Service implementation for Campaign operations
    /// </summary>
    public class CampaignService : ICampaignService
    {
        private readonly ICampaignRepository _repository;

        /// <summary>
        /// Initializes a new instance of the CampaignService class
        /// </summary>
        /// <param name="repository">The campaign repository</param>
        public CampaignService(ICampaignRepository repository)
        {
            _repository = repository;
        }

        /// <inheritdoc />
        public async Task<Campaign?> GetDefaultCampaignAsync()
        {
            return await _repository.GetDefaultCampaignAsync();
        }

        /// <inheritdoc />
        public async Task<bool> SetDefaultCampaignAsync(int campaignId)
        {
            // Verify the campaign exists before setting it as default
            var campaign = await _repository.GetByIdAsync(campaignId);
            if (campaign == null || !campaign.IsActive)
            {
                return false;
            }

            await _repository.SetDefaultCampaignAsync(campaignId);
            return true;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Campaign>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        /// <inheritdoc />
        public async Task<Campaign?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        /// <inheritdoc />
        public async Task<Campaign> AddAsync(Campaign campaign)
        {
            return await _repository.AddAsync(campaign);
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(Campaign campaign)
        {
            await _repository.UpdateAsync(campaign);
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> SetActiveAsync(int id, bool isActive, string modifiedBy = "System")
        {
            return await _repository.SetActiveStatusAsync(id, isActive, modifiedBy);
        }
    }
}
