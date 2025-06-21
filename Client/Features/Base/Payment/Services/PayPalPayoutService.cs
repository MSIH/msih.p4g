/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using msih.p4g.Shared.Models.PaymentService;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System;
using msih.p4g.Client.Features.Base.Payment.Interfaces;

namespace msih.p4g.Client.Features.Base.Payment.Services
{
    /// <summary>
    /// Client service for PayPal payout operations
    /// </summary>
    public class PayPalPayoutService : IPayPalPayoutService
    {
        private readonly ILogger<PayPalPayoutService> _logger;
        private readonly Server.Features.Base.PaymentService.Interfaces.IPayPalPayoutService _serverPayoutService;

        public PayPalPayoutService(
            Server.Features.Base.PaymentService.Interfaces.IPayPalPayoutService serverPayoutService, 
            ILogger<PayPalPayoutService> logger)
        {
            _serverPayoutService = serverPayoutService ?? throw new ArgumentNullException(nameof(serverPayoutService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get payment history for a fundraiser
        /// </summary>
        /// <param name="fundraiserId">Fundraiser ID</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>List of payment records</returns>
        public async Task<List<PaymentDto>> GetFundraiserPaymentHistoryAsync(string fundraiserId, int page = 1, int pageSize = 20)
        {
            try
            {
                _logger.LogInformation("Getting payment history for fundraiser {FundraiserId}", fundraiserId);
                
                return await _serverPayoutService.GetFundraiserPaymentHistoryAsync(fundraiserId, page, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment history for fundraiser {FundraiserId}", fundraiserId);
                throw;
            }
        }

        /// <summary>
        /// Get a payment by ID
        /// </summary>
        /// <param name="paymentId">Payment ID</param>
        /// <returns>Payment record</returns>
        public async Task<PaymentDto> GetPaymentAsync(string paymentId)
        {
            try
            {
                _logger.LogInformation("Getting payment {PaymentId}", paymentId);
                
                return await _serverPayoutService.GetPaymentAsync(paymentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment {PaymentId}", paymentId);
                throw;
            }
        }

        /// <summary>
        /// Create a new payment record
        /// </summary>
        /// <param name="fundraiserId">The ID of the fundraiser</param>
        /// <param name="paypalEmail">The PayPal email to send payment to</param>
        /// <param name="amount">The amount to pay</param>
        /// <param name="currency">The currency code (default: USD)</param>
        /// <param name="notes">Optional notes for the payment</param>
        /// <returns>The created payment record</returns>
        public async Task<PaymentDto> CreatePaymentAsync(string fundraiserId, string paypalEmail, decimal amount, string currency = "USD", string? notes = null)
        {
            try
            {
                _logger.LogInformation("Creating payment for fundraiser {FundraiserId}", fundraiserId);
                
                return await _serverPayoutService.CreatePaymentAsync(fundraiserId, paypalEmail, amount, currency, notes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment for fundraiser {FundraiserId}", fundraiserId);
                throw;
            }
        }

        /// <summary>
        /// Process a pending payment through PayPal
        /// </summary>
        /// <param name="paymentId">The ID of the payment to process</param>
        /// <returns>The updated payment record</returns>
        public async Task<PaymentDto> ProcessPaymentAsync(string paymentId)
        {
            try
            {
                _logger.LogInformation("Processing payment {PaymentId}", paymentId);
                
                return await _serverPayoutService.ProcessPaymentAsync(paymentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment {PaymentId}", paymentId);
                throw;
            }
        }
    }
}
