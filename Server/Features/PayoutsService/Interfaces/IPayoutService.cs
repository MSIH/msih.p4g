/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using msih.p4g.Server.Features.PayoutService.Models;
using msih.p4g.Server.Features.PayoutService.Models.PayPal;
using PayoutEntity = msih.p4g.Server.Features.PayoutService.Models.Payout;
using AccountType = AccountType;
using AccountFormat = AccountFormat;

namespace msih.p4g.Server.Features.PayoutService.Interfaces
{
    /// <summary>
    /// Interface for PayPal payout service
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
        /// <param name="accountType">The type of account for the payout (default: PayPal)</param>
        /// <param name="accountFormat">The format of the account identifier (default: Email)</param>
        /// <returns>The created Payout record</returns>
        Task<PayoutEntity> CreatePayoutAsync(
            string fundraiserId,
            string paypalEmail,
            decimal amount,
            string currency = "USD",
            string? notes = null,
            AccountType accountType = AccountType.PayPal,
            AccountFormat accountFormat = AccountFormat.Email);


        /// <summary>
        /// Process multiple Payouts as a batch payout
        /// </summary>
        /// <param name="payoutIds">List of Payout IDs to process in a batch</param>
        /// <returns>List of updated Payout records</returns>
        Task<List<Payout>> ProcessBatchPayoutsAsync(List<string> payoutIds);

        /// <summary>
        /// Get a Payout by ID
        /// </summary>
        /// <param name="payoutId">The Payout ID</param>
        /// <returns>The Payout record</returns>
        Task<Payout> GetPayoutAsync(string payoutId);

        /// <summary>
        /// Get Payout history for a fundraiser
        /// </summary>
        /// <param name="fundraiserId">The fundraiser ID</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>List of Payout records</returns>
        Task<List<Payout>> GetFundraiserPayoutHistoryAsync(string fundraiserId, int page = 1, int pageSize = 20);

        /// <summary>
        /// Get all Payouts with a specific status
        /// </summary>
        /// <param name="status">The Payout status to filter by</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>List of Payout records</returns>
        Task<List<Payout>> GetPayoutsByStatusAsync(PayPalTransactionStatusEnum status, int page = 1, int pageSize = 20);

        /// <summary>
        /// Get the status of a PayPal batch payout
        /// </summary>
        /// <param name="batchId">The PayPal batch ID</param>
        /// <returns>Batch status information</returns>
        Task<PayPalBatchStatus> GetBatchPayoutStatusAsync(string batchId);

        /// <summary>
        /// Get all payouts for a fundraiser
        /// </summary>
        /// <param name="fundraiserId">The fundraiser ID</param>
        /// <returns>List of all Payout records for the fundraiser</returns>
        Task<List<Payout>> GetPayoutsByFundraiserIdAsync(string fundraiserId);
    }
}
