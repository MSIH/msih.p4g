/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

// This file has been deprecated as part of the architectural simplification.
// The server-side FundraiserStatisticsService is now directly injected into components.
// This file is kept for reference but should be deleted in a future cleanup.

/*
using System.Threading.Tasks;
using msih.p4g.Client.Features.FundraiserManagement.Interfaces;
using msih.p4g.Server.Features.FundraiserService.Model;

namespace msih.p4g
{
    /// <summary>
    /// Client service implementation for fundraiser statistics
    /// </summary>
    public class FundraiserStatisticsService : IFundraiserStatisticsService
    {
        private readonly msih.p4g.Server.Features.FundraiserService.Interfaces.IFundraiserStatisticsService _serverService;

        /// <summary>
        /// Initializes a new instance of the FundraiserStatisticsService class
        /// </summary>
        /// <param name="serverService">The server-side fundraiser statistics service</param>
        public FundraiserStatisticsService(msih.p4g.Server.Features.FundraiserService.Interfaces.IFundraiserStatisticsService serverService)
        {
            _serverService = serverService;
        }

        /// <inheritdoc />
        public async Task<FundraiserStatistics> GetStatisticsAsync(int fundraiserId)
        {
            return await _serverService.GetStatisticsAsync(fundraiserId);
        }
    }
}
*/
