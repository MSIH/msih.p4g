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
using Microsoft.EntityFrameworkCore;
using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.Base.PaymentService.Interfaces;
using msih.p4g.Server.Features.Base.PaymentService.Models;

namespace msih.p4g.Server.Features.Base.PaymentService.Repositories
{
    /// <summary>
    /// Repository for managing payment transactions
    /// </summary>
    public class PaymentTransactionRepository : GenericRepository<PaymentTransaction>, IPaymentTransactionRepository
    {
        public PaymentTransactionRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : base(contextFactory)
        {
        }

        /// <summary>
        /// Gets a payment transaction by its provider-specific transaction ID
        /// </summary>
        /// <param name="transactionId">The transaction ID from the payment provider</param>
        /// <returns>The payment transaction, or null if not found</returns>
        public async Task<PaymentTransaction> GetByTransactionIdAsync(string transactionId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<PaymentTransaction>()
                .Where(t => t.TransactionId == transactionId && t.IsActive)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets payment transactions by their status
        /// </summary>
        /// <param name="status">The payment status to filter by</param>
        /// <returns>A collection of payment transactions with the specified status</returns>
        public async Task<IEnumerable<PaymentTransaction>> GetByStatusAsync(PaymentStatus status)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<PaymentTransaction>()
                .Where(t => t.Status == status && t.IsActive)
                .OrderByDescending(t => t.ProcessedOn)
                .ToListAsync();
        }

        /// <summary>
        /// Gets payment transactions for a specific order
        /// </summary>
        /// <param name="orderReference">The order reference to filter by</param>
        /// <returns>A collection of payment transactions for the specified order</returns>
        public async Task<IEnumerable<PaymentTransaction>> GetByOrderReferenceAsync(string orderReference)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<PaymentTransaction>()
                .Where(t => t.OrderReference == orderReference && t.IsActive)
                .OrderByDescending(t => t.ProcessedOn)
                .ToListAsync();
        }

        /// <summary>
        /// Gets payment transactions by customer email
        /// </summary>
        /// <param name="email">The customer email to filter by</param>
        /// <returns>A collection of payment transactions for the specified customer</returns>
        public async Task<IEnumerable<PaymentTransaction>> GetByCustomerEmailAsync(string email)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<PaymentTransaction>()
                .Where(t => t.CustomerEmail == email && t.IsActive)
                .OrderByDescending(t => t.ProcessedOn)
                .ToListAsync();
        }

        /// <summary>
        /// Gets payment transactions within a date range
        /// </summary>
        /// <param name="startDate">The start date (inclusive)</param>
        /// <param name="endDate">The end date (inclusive)</param>
        /// <returns>A collection of payment transactions within the specified date range</returns>
        public async Task<IEnumerable<PaymentTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<PaymentTransaction>()
                .Where(t => t.ProcessedOn >= startDate && t.ProcessedOn <= endDate && t.IsActive)
                .OrderByDescending(t => t.ProcessedOn)
                .ToListAsync();
        }

        /// <summary>
        /// Updates the status of a payment transaction
        /// </summary>
        /// <param name="id">The ID of the payment transaction</param>
        /// <param name="status">The new status</param>
        /// <param name="errorMessage">Optional error message if status is Failed</param>
        /// <param name="modifiedBy">The user who updated the status</param>
        /// <returns>True if the transaction was updated, false otherwise</returns>
        public async Task<bool> UpdateStatusAsync(int id, PaymentStatus status, string? errorMessage = null, string modifiedBy = "PaymentService")
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var transaction = await context.Set<PaymentTransaction>().FindAsync(id);
            if (transaction == null || !transaction.IsActive)
            {
                return false;
            }

            transaction.Status = status;

            if (status == PaymentStatus.Failed && !string.IsNullOrEmpty(errorMessage))
            {
                transaction.ErrorMessage = errorMessage;
            }

            transaction.ModifiedOn = DateTime.UtcNow;
            transaction.ModifiedBy = modifiedBy;

            await context.SaveChangesAsync();

            return true;
        }
    }
}
