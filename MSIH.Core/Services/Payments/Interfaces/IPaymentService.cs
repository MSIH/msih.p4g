/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using MSIH.Core.Services.Payments.Models;
using System.Threading.Tasks;

namespace MSIH.Core.Services.Payments.Interfaces
{
    /// <summary>
    /// Interface for payment service operations
    /// </summary>
    public interface IPaymentService
    {
        /// <summary>
        /// Gets the name of the payment provider
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// Initializes the payment service with necessary configuration
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Generates a client token for the payment SDK
        /// </summary>
        /// <param name="request">The client token request</param>
        /// <returns>A client token response</returns>
        Task<ClientTokenResponse> GenerateClientTokenAsync(Models.ClientTokenRequest request);

        /// <summary>
        /// Processes a payment
        /// </summary>
        /// <param name="request">The payment request</param>
        /// <returns>A payment response</returns>
        Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request);

        /// <summary>
        /// Processes a refund
        /// </summary>
        /// <param name="request">The refund request</param>
        /// <returns>A refund response</returns>
        Task<RefundResponse> ProcessRefundAsync(RefundRequest request);

        /// <summary>
        /// Voids a transaction
        /// </summary>
        /// <param name="transactionId">The transaction ID to void</param>
        /// <returns>True if the transaction was voided successfully, false otherwise</returns>
        Task<bool> VoidTransactionAsync(string transactionId);

        /// <summary>
        /// Gets the details of a transaction
        /// </summary>
        /// <param name="transactionId">The transaction ID</param>
        /// <returns>The payment transaction details</returns>
        Task<PaymentTransaction?> GetTransactionDetailsAsync(string transactionId);

        /// <summary>
        /// Updates the status of a transaction from the payment provider
        /// </summary>
        /// <param name="transactionId">The transaction ID</param>
        /// <returns>The updated payment transaction</returns>
        Task<PaymentTransaction?> UpdateTransactionStatusAsync(string transactionId);
    }
}
