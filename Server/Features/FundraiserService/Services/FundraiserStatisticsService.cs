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
using msih.p4g.Server.Features.FundraiserService.Interfaces;
using msih.p4g.Server.Features.FundraiserService.Model;

namespace msih.p4g.Server.Features.FundraiserService.Services
{
    /// <summary>
    /// Service for retrieving fundraiser statistics
    /// </summary>
    public class FundraiserStatisticsService : IFundraiserStatisticsService
    {
        private readonly IFundraiserStatisticsRepository _repository;

        /// <summary>
        /// Initializes a new instance of the FundraiserStatisticsService class
        /// </summary>
        /// <param name="repository">The fundraiser statistics repository</param>
        public FundraiserStatisticsService(IFundraiserStatisticsRepository repository)
        {
            _repository = repository;
        }

        /// <inheritdoc />
        public async Task<FundraiserStatistics> GetStatisticsAsync(int fundraiserId)
        {
            return await _repository.GetStatisticsAsync(fundraiserId);
        }

        /// <inheritdoc />
        public async Task<List<FirstTimeDonorInfo>> GetFirstTimeDonorsAsync(int fundraiserId)
        {
            return await _repository.GetFirstTimeDonorsAsync(fundraiserId);
        }
    }
}
