using msih.p4g.Shared.Models.PaymentService;
using msih.p4g.Server.Features.Base.PaymentService.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace msih.p4g.Server.Features.Base.PaymentService.Interfaces
{
    /// <summary>
    /// Interface for PayPal payout service
    /// </summary>
    public interface IPayPalPayoutService
    {
        /// <summary>
        /// Create a new payment record
        /// </summary>
        /// <param name="fundraiserId">The ID of the fundraiser</param>
        /// <param name="paypalEmail">The PayPal email to send payment to</param>
        /// <param name="amount">The amount to pay</param>
        /// <param name="currency">The currency code (default: USD)</param>
        /// <param name="notes">Optional notes for the payment</param>
        /// <returns>The created payment record</returns>
        Task<PaymentDto> CreatePaymentAsync(string fundraiserId, string paypalEmail, decimal amount, string currency = "USD", string? notes = null);

        /// <summary>
        /// Process a pending payment through PayPal
        /// </summary>
        /// <param name="paymentId">The ID of the payment to process</param>
        /// <returns>The updated payment record</returns>
        Task<PaymentDto> ProcessPaymentAsync(string paymentId);

        /// <summary>
        /// Get a payment by ID
        /// </summary>
        /// <param name="paymentId">The payment ID</param>
        /// <returns>The payment record</returns>
        Task<PaymentDto> GetPaymentAsync(string paymentId);

        /// <summary>
        /// Get payment history for a fundraiser
        /// </summary>
        /// <param name="fundraiserId">The fundraiser ID</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>List of payment records</returns>
        Task<List<PaymentDto>> GetFundraiserPaymentHistoryAsync(string fundraiserId, int page = 1, int pageSize = 20);

        /// <summary>
        /// Get all payments with a specific status
        /// </summary>
        /// <param name="status">The payment status to filter by</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>List of payment records</returns>
        Task<List<PaymentDto>> GetPaymentsByStatusAsync(PaymentStatus status, int page = 1, int pageSize = 20);
    }
}
