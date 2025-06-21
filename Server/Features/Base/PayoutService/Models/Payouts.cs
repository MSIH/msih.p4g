/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using System;
using msih.p4g.Shared.Models.PaymentService;
using msih.p4g.Server.Common.Models;

namespace msih.p4g.Server.Features.Base.PaypalPayoutService.Models
{
    /// <summary>
    /// Represents a payment/payout to a fundraiser
    /// </summary>
    public class Payouts : BaseEntity
    {
        /// <summary>
        /// The ID of the fundraiser receiving the payment
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
        /// The current status of the payment
        /// </summary>
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        
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
        /// When the payment record was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// When the payment was processed with PayPal
        /// </summary>
        public DateTime? ProcessedAt { get; set; }
        
        /// <summary>
        /// Optional notes about the payment
        /// </summary>
        public string? Notes { get; set; }
        
        /// <summary>
        /// Error message if payment processing failed
        /// </summary>
        public string? ErrorMessage { get; set; }
        
        /// <summary>
        /// Indicates whether this payment is part of a batch
        /// </summary>
        public bool IsBatchPayment { get; set; } = false;
    }
}
