/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.Base.PaypalPayoutService.Interfaces;
using msih.p4g.Server.Features.Base.PaypalPayoutService.Models;
using msih.p4g.Shared.Models.PayoutService;

namespace msih.p4g.Server.Features.Base.PaypalPayoutService.Repositories
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
            ApplicationDbContext dbContext,
            ILogger<PayoutRepository> logger)
            : base(dbContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get Payouts by status
        /// </summary>
        public async Task<List<Payout>> GetPayoutsByStatusAsync(PayoutStatus status, int page = 1, int pageSize = 20)
        {
            try
            {
                return await DbSet
                    .Where(p => p.Status == status)
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
                return await DbSet
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
                return await DbSet
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
        public async Task<List<Payout>> GetPayoutsByIdsAsync(List<string> PayoutIds)
        {
            try
            {
                return await DbSet
                    .Where(p => PayoutIds.Contains(p.Id))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Payouts by IDs");
                throw;
            }
        }
    }
}
