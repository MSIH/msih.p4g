/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using System.Collections.Generic;
using System.Threading.Tasks;
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.Base.PayoutService.Models;
using msih.p4g.Shared.Models.PayoutService;

namespace msih.p4g.Server.Features.Base.PayoutService.Interfaces
{
    /// <summary>
    /// Repository interface for PayPal Payouts
    /// </summary>
    public interface IPayoutRepository : IGenericRepository<Payout>
    {
        /// <summary>
        /// Get Payouts by status
        /// </summary>
        /// <param name="status">The Payout status to filter by</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>List of Payouts</returns>
        Task<List<Payout>> GetPayoutsByStatusAsync(PayoutStatus status, int page = 1, int pageSize = 20);
        
        /// <summary>
        /// Get Payouts for a specific fundraiser
        /// </summary>
        /// <param name="fundraiserId">The fundraiser ID</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>List of Payouts</returns>
        Task<List<Payout>> GetPayoutsByFundraiserIdAsync(string fundraiserId, int page = 1, int pageSize = 20);
        
        /// <summary>
        /// Get Payouts by batch ID
        /// </summary>
        /// <param name="batchId">The PayPal batch ID</param>
        /// <returns>List of Payouts in the batch</returns>
        Task<List<Payout>> GetPayoutsByBatchIdAsync(string batchId);
        
        /// <summary>
        /// Get multiple Payouts by their IDs
        /// </summary>
        /// <param name="PayoutIds">List of Payout IDs</param>
        /// <returns>List of Payouts</returns>
        Task<List<Payout>> GetPayoutsByIdsAsync(List<string> PayoutIds);
    }
}
