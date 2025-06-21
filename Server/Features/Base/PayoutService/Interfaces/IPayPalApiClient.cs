/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using System.Threading.Tasks;
using msih.p4g.Server.Features.Base.PayoutService.Models.PayPal;

namespace msih.p4g.Server.Features.Base.PayoutService.Interfaces
{
    /// <summary>
    /// Interface for direct PayPal API interactions
    /// </summary>
    public interface IPayPalApiClient
    {
        /// <summary>
        /// Get an access token from PayPal OAuth
        /// </summary>
        /// <returns>PayPal access token response</returns>
        Task<PayPalTokenResponse> GetAccessTokenAsync();

        /// <summary>
        /// Create a payout (single or batch) through PayPal API
        /// </summary>
        /// <param name="request">The payout request details</param>
        /// <returns>The PayPal payout response</returns>
        Task<PayPalPayoutResponse> CreatePayoutAsync(PayPalPayoutRequest request);

        /// <summary>
        /// Get the status of a batch payout
        /// </summary>
        /// <param name="batchId">The PayPal-generated batch ID</param>
        /// <returns>The batch status information</returns>
        Task<PayPalBatchStatus> GetBatchPayoutStatusAsync(string batchId);

        /// <summary>
        /// Get the details of a payout item
        /// </summary>
        /// <param name="payoutItemId">The PayPal-generated payout item ID</param>
        /// <returns>The payout item status information</returns>
        Task<PayPalBatchStatusItem> GetPayoutItemDetailsAsync(string payoutItemId);
    }
}
