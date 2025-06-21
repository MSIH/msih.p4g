/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using System;
using msih.p4g.Shared.Models;

namespace msih.p4g.Shared.Models.PayoutService
{
    /// <summary>
    /// DTO for Payout information
    /// </summary>
    public class PayoutDto
    {
        /// <summary>
        /// The unique identifier for the payout
        /// </summary>
        public string Id { get; set; } = null!;
        
        /// <summary>
        /// The ID of the fundraiser receiving the payout
        /// </summary>
        public string FundraiserId { get; set; } = null!;
        
        /// <summary>
        /// The PayPal email address of the recipient
        /// </summary>
        public string PaypalEmail { get; set; } = null!;
        
        /// <summary>
        /// The amount to be paid
        /// </summary>
        public decimal Amount { get; set; }
        
        /// <summary>
        /// The currency code (e.g., USD)
        /// </summary>
        public string Currency { get; set; } = "USD";
        
        /// <summary>
        /// The current status of the payout
        /// </summary>
        public PayoutStatus Status { get; set; }
        
        /// <summary>
        /// The PayPal batch ID for batch payouts
        /// </summary>
        public string? PaypalBatchId { get; set; }
        
        /// <summary>
        /// The PayPal payout item ID for individual payouts
        /// </summary>
        public string? PaypalPayoutItemId { get; set; }
        
        /// <summary>
        /// Transaction ID returned by PayPal
        /// </summary>
        public string? PaypalTransactionId { get; set; }
        
        /// <summary>
        /// When the payout record was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// When the payout was processed with PayPal
        /// </summary>
        public DateTime? ProcessedAt { get; set; }
        
        /// <summary>
        /// Optional notes about the payout
        /// </summary>
        public string? Notes { get; set; }
    }
}
