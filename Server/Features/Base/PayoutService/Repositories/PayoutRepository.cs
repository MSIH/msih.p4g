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
using msih.p4g.Server.Features.Base.PayoutService.Interfaces;
using msih.p4g.Server.Features.Base.PayoutService.Models;
using msih.p4g.Server.Features.Base.PayoutService.Models.PayPal;

namespace msih.p4g.Server.Features.Base.PayoutService.Repositories
{
    /// <summary>
    /// Repository implementation for PayPal Payouts
    /// </summary>
    public class PayoutRepository : GenericRepository<Payout, ApplicationDbContext>, IPayoutRepository
    {
        private readonly ILogger<PayoutRepository> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        public PayoutRepository(
            ApplicationDbContext dbContext,
            ILogger<PayoutRepository> logger)
            : base(dbContext)
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
                return await _dbSet
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
                return await _dbSet
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
        /// Get Payouts by batch ID
        /// </summary>
        public async Task<List<Payout>> GetPayoutsByBatchIdAsync(string batchId)
        {
            try
            {
                return await _dbSet
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
                // Convert string IDs to integers
                var ids = payoutIds
                    .Select(id => int.TryParse(id, out int result) ? result : -1)
                    .Where(id => id != -1)
                    .ToList();

                return await _dbSet
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
                _context.UpdateRange(payouts);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating multiple Payouts");
                throw;
            }
        }
    }
}
