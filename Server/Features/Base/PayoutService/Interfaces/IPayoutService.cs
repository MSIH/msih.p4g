using msih.p4g.Shared.Models.PayoutService;
using msih.p4g.Server.Features.Base.PaypalPayoutService.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace msih.p4g.Server.Features.Base.PayoutService.Interfaces
{
    /// <summary>
    /// Interface for PayPal payout service
    /// </summary>
    public interface IPayPalPayoutService
    {
        /// <summary>
        /// Create a new Payout record
        /// </summary>
        /// <param name="fundraiserId">The ID of the fundraiser</param>
        /// <param name="paypalEmail">The PayPal email to send Payout to</param>
        /// <param name="amount">The amount to pay</param>
        /// <param name="currency">The currency code (default: USD)</param>
        /// <param name="notes">Optional notes for the Payout</param>
        /// <returns>The created Payout record</returns>
        Task<Payout> CreatePayoutAsync(string fundraiserId, string paypalEmail, decimal amount, string currency = "USD", string? notes = null);

        /// <summary>
        /// Process a pending Payout through PayPal
        /// </summary>
        /// <param name="PayoutId">The ID of the Payout to process</param>
        /// <returns>The updated Payout record</returns>
        Task<Payout> ProcessPayoutAsync(string PayoutId);

        /// <summary>
        /// Process multiple Payouts as a batch payout
        /// </summary>
        /// <param name="PayoutIds">List of Payout IDs to process in a batch</param>
        /// <returns>List of updated Payout records</returns>
        Task<List<Payout>> ProcessBatchPayoutsAsync(List<string> PayoutIds);

        /// <summary>
        /// Get a Payout by ID
        /// </summary>
        /// <param name="PayoutId">The Payout ID</param>
        /// <returns>The Payout record</returns>
        Task<Payout> GetPayoutAsync(string PayoutId);

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
        Task<List<Payout>> GetPayoutsByStatusAsync(PayoutStatus status, int page = 1, int pageSize = 20);
        
        /// <summary>
        /// Get the status of a PayPal batch payout
        /// </summary>
        /// <param name="batchId">The PayPal batch ID</param>
        /// <returns>Batch status information</returns>
        Task<PayPalBatchStatus> GetBatchPayoutStatusAsync(string batchId);
    }
}
