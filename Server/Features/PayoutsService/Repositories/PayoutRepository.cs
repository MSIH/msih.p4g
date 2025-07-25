/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.EntityFrameworkCore;
using MSIH.Core.Common.Data;
using MSIH.Core.Common.Data.Repositories;
using msih.p4g.Server.Features.PayoutService.Interfaces;
using msih.p4g.Server.Features.PayoutService.Models;
using msih.p4g.Server.Features.PayoutService.Models.PayPal;

namespace msih.p4g.Server.Features.PayoutService.Repositories
{
    /// <summary>
    /// Repository implementation for PayPal Payouts
    /// </summary>
    public class PayoutRepository : GenericRepository<Payout>, IPayoutRepository
    {
        private readonly ILogger<PayoutRepository> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        public PayoutRepository(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            ILogger<PayoutRepository> logger)
            : base(contextFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get Payouts by status
        /// </summary>
        public async Task<List<Payout>> GetPayoutsByStatusAsync(PayPalTransactionStatusEnum status, int page = 1, int pageSize = 20)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Set<Payout>()
                    .Where(p => p.TransactionStatus == status)
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Payouts by status {Status}", status);
                throw;
            }
        }

        /// <summary>
        /// Get Payouts for a specific fundraiser
        /// </summary>
        public async Task<List<Payout>> GetPayoutsByFundraiserIdAsync(string fundraiserId, int page = 1, int pageSize = 20)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Set<Payout>()
                    .Where(p => p.FundraiserId == fundraiserId)
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Payouts for fundraiser {FundraiserId}", fundraiserId);
                throw;
            }
        }

        /// <summary>
        /// Get all Payouts for a specific fundraiser
        /// </summary>
        public async Task<List<Payout>> GetPayoutsByFundraiserIdAsync(string fundraiserId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Set<Payout>()
                    .Where(p => p.FundraiserId == fundraiserId)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all Payouts for fundraiser {FundraiserId}", fundraiserId);
                throw;
            }
        }

        /// <summary>
        /// Get Payouts by batch ID
        /// </summary>
        public async Task<List<Payout>> GetPayoutsByBatchIdAsync(string batchId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Set<Payout>()
                    .Where(p => p.PaypalBatchId == batchId)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Payouts for batch {BatchId}", batchId);
                throw;
            }
        }

        /// <summary>
        /// Get multiple Payouts by their IDs
        /// </summary>
        public async Task<List<Payout>> GetPayoutsByIdsAsync(List<string> payoutIds)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                // Convert string IDs to integers
                var ids = payoutIds
                    .Select(id => int.TryParse(id, out int result) ? result : -1)
                    .Where(id => id != -1)
                    .ToList();

                return await context.Set<Payout>()
                    .Where(p => ids.Contains(p.Id))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Payouts by IDs");
                throw;
            }
        }

        /// <summary>
        /// Updates multiple Payout entities at once
        /// </summary>
        public async Task UpdateRangeAsync(List<Payout> payouts)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                context.UpdateRange(payouts);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating multiple Payouts");
                throw;
            }
        }
    }
}
