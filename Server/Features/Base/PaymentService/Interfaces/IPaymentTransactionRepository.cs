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
using msih.p4g.Server.Features.Base.PaymentService.Models;

namespace msih.p4g.Server.Features.Base.PaymentService.Interfaces
{
    /// <summary>
    /// Repository interface for payment transactions
    /// </summary>
    public interface IPaymentTransactionRepository : IGenericRepository<PaymentTransaction>
    {
        /// <summary>
        /// Gets a payment transaction by its provider-specific transaction ID
        /// </summary>
        /// <param name="transactionId">The transaction ID from the payment provider</param>
        /// <returns>The payment transaction, or null if not found</returns>
        Task<PaymentTransaction> GetByTransactionIdAsync(string transactionId);

        /// <summary>
        /// Gets payment transactions by their status
        /// </summary>
        /// <param name="status">The payment status to filter by</param>
        /// <returns>A collection of payment transactions with the specified status</returns>
        Task<IEnumerable<PaymentTransaction>> GetByStatusAsync(PaymentStatus status);

        /// <summary>
        /// Gets payment transactions for a specific order
        /// </summary>
        /// <param name="orderReference">The order reference to filter by</param>
        /// <returns>A collection of payment transactions for the specified order</returns>
        Task<IEnumerable<PaymentTransaction>> GetByOrderReferenceAsync(string orderReference);

        /// <summary>
        /// Gets payment transactions by customer email
        /// </summary>
        /// <param name="email">The customer email to filter by</param>
        /// <returns>A collection of payment transactions for the specified customer</returns>
        Task<IEnumerable<PaymentTransaction>> GetByCustomerEmailAsync(string email);

        /// <summary>
        /// Gets payment transactions within a date range
        /// </summary>
        /// <param name="startDate">The start date (inclusive)</param>
        /// <param name="endDate">The end date (inclusive)</param>
        /// <returns>A collection of payment transactions within the specified date range</returns>
        Task<IEnumerable<PaymentTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Updates the status of a payment transaction
        /// </summary>
        /// <param name="id">The ID of the payment transaction</param>
        /// <param name="status">The new status</param>
        /// <param name="errorMessage">Optional error message if status is Failed</param>
        /// <param name="modifiedBy">The user who updated the status</param>
        /// <returns>True if the transaction was updated, false otherwise</returns>
        Task<bool> UpdateStatusAsync(int id, PaymentStatus status, string? errorMessage = null, string modifiedBy = "PaymentService");
    }
}
