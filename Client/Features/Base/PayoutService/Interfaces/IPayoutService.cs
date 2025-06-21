/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using System.Collections.Generic;
using System.Threading.Tasks;
using msih.p4g.Server.Features.Base.PayoutService.Models;
using msih.p4g.Server.Features.Base.PayoutService.Models.PayPal;
using msih.p4g.Shared.Models.PayoutService;

namespace msih.p4g.Client.Features.Base.PayoutService.Interfaces
{
    /// <summary>
    /// Interface for client-side PayPal payout service
    /// </summary>
    public interface IPayoutService
    {
        /// <summary>
        /// Create a new Payout record
        /// </summary>
        /// <param name="fundraiserId">The ID of the fundraiser</param>
        /// <param name="paypalEmail">The PayPal email to send Payout to</param>
        /// <param name="amount">The amount to pay</param>
        /// <param name="currency">The currency code (default: USD)</param>
        /// <param name="notes">Optional notes for the Payout</param>
        /// <returns>The created Payout DTO</returns>
        Task<PayoutDto> CreatePayoutAsync(string fundraiserId, string paypalEmail, decimal amount, string currency = "USD", string? notes = null);

        /// <summary>
        /// Process a pending Payout through PayPal
        /// </summary>
        /// <param name="payoutId">The ID of the Payout to process</param>
        /// <returns>The updated Payout DTO</returns>
        Task<PayoutDto> ProcessPayoutAsync(string payoutId);

        /// <summary>
        /// Process multiple Payouts as a batch payout
        /// </summary>
        /// <param name="payoutIds">List of Payout IDs to process in a batch</param>
        /// <returns>List of updated Payout DTOs</returns>
        Task<List<PayoutDto>> ProcessBatchPayoutsAsync(List<string> payoutIds);

        /// <summary>
        /// Get a Payout by ID
        /// </summary>
        /// <param name="payoutId">The Payout ID</param>
        /// <returns>The Payout DTO</returns>
        Task<PayoutDto> GetPayoutAsync(string payoutId);

        /// <summary>
        /// Get Payout history for a fundraiser
        /// </summary>
        /// <param name="fundraiserId">The fundraiser ID</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>List of Payout DTOs</returns>
        Task<List<PayoutDto>> GetFundraiserPayoutHistoryAsync(string fundraiserId, int page = 1, int pageSize = 20);

        /// <summary>
        /// Get all Payouts with a specific status
        /// </summary>
        /// <param name="status">The Payout status to filter by</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>List of Payout DTOs</returns>
        Task<List<PayoutDto>> GetPayoutsByStatusAsync(PayoutStatus status, int page = 1, int pageSize = 20);
        
        /// <summary>
        /// Get the status of a PayPal batch payout
        /// </summary>
        /// <param name="batchId">The PayPal batch ID</param>
        /// <returns>Batch status information DTO</returns>
        Task<PayPalBatchStatusDto> GetBatchPayoutStatusAsync(string batchId);
    }
}
