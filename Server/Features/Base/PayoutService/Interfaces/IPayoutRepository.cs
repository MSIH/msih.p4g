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
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.Base.PayoutService.Models;
using msih.p4g.Server.Features.Base.PayoutService.Models.PayPal;

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
        Task<List<Payout>> GetPayoutsByStatusAsync(PayPalTransactionStatusEnum status, int page = 1, int pageSize = 20);

        /// <summary>
        /// Get Payouts for a specific fundraiser
        /// </summary>
        /// <param name="fundraiserId">The fundraiser ID</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>List of Payouts</returns>
        Task<List<Payout>> GetPayoutsByFundraiserIdAsync(string fundraiserId, int page = 1, int pageSize = 20);

        /// <summary>
        /// Get all Payouts for a specific fundraiser
        /// </summary>
        /// <param name="fundraiserId">The fundraiser ID</param>
        /// <returns>List of all Payouts for the fundraiser</returns>
        Task<List<Payout>> GetPayoutsByFundraiserIdAsync(string fundraiserId);

        /// <summary>
        /// Get Payouts by batch ID
        /// </summary>
        /// <param name="batchId">The PayPal batch ID</param>
        /// <returns>List of Payouts in the batch</returns>
        Task<List<Payout>> GetPayoutsByBatchIdAsync(string batchId);

        /// <summary>
        /// Get multiple Payouts by their IDs
        /// </summary>
        /// <param name="payoutIds">List of Payout IDs</param>
        /// <returns>List of Payouts</returns>
        Task<List<Payout>> GetPayoutsByIdsAsync(List<string> payoutIds);

        /// <summary>
        /// Updates multiple Payout entities at once
        /// </summary>
        /// <param name="payouts">The Payout entities to update</param>
        /// <returns>A Task representing the asynchronous operation</returns>
        Task UpdateRangeAsync(List<Payout> payouts);
    }
}
