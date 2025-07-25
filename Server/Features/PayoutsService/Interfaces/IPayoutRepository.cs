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
using MSIH.Core.Common.Data.Repositories;
using msih.p4g.Server.Features.PayoutService.Models;
using msih.p4g.Server.Features.PayoutService.Models.PayPal;
using PayoutEntity = msih.p4g.Server.Features.PayoutService.Models.Payout;

namespace msih.p4g.Server.Features.PayoutService.Interfaces
{
    /// <summary>
    /// Repository interface for PayPal Payouts
    /// </summary>
    public interface IPayoutRepository : IGenericRepository<PayoutEntity>
    {
        /// <summary>
        /// Get Payouts by status
        /// </summary>
        /// <param name="status">The Payout status to filter by</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>List of Payouts</returns>
        Task<List<PayoutEntity>> GetPayoutsByStatusAsync(PayPalTransactionStatusEnum status, int page = 1, int pageSize = 20);

        /// <summary>
        /// Get Payouts for a specific fundraiser
        /// </summary>
        /// <param name="fundraiserId">The fundraiser ID</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>List of Payouts</returns>
        Task<List<PayoutEntity>> GetPayoutsByFundraiserIdAsync(string fundraiserId, int page = 1, int pageSize = 20);

        /// <summary>
        /// Get all Payouts for a specific fundraiser
        /// </summary>
        /// <param name="fundraiserId">The fundraiser ID</param>
        /// <returns>List of all Payouts for the fundraiser</returns>
        Task<List<PayoutEntity>> GetPayoutsByFundraiserIdAsync(string fundraiserId);

        /// <summary>
        /// Get Payouts by batch ID
        /// </summary>
        /// <param name="batchId">The PayPal batch ID</param>
        /// <returns>List of Payouts in the batch</returns>
        Task<List<PayoutEntity>> GetPayoutsByBatchIdAsync(string batchId);

        /// <summary>
        /// Get multiple Payouts by their IDs
        /// </summary>
        /// <param name="payoutIds">List of Payout IDs</param>
        /// <returns>List of Payouts</returns>
        Task<List<PayoutEntity>> GetPayoutsByIdsAsync(List<string> payoutIds);

        /// <summary>
        /// Updates multiple Payout entities at once
        /// </summary>
        /// <param name="payouts">The Payout entities to update</param>
        /// <returns>A Task representing the asynchronous operation</returns>
        Task UpdateRangeAsync(List<PayoutEntity> payouts);
    }
}
