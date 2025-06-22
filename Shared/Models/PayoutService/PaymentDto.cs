/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using System;

namespace msih.p4g.Shared.Models.PayoutService
{
    /// <summary>
    /// Data transfer object for payment history information
    /// </summary>
    public class PaymentDto
    {
        /// <summary>
        /// The payment ID
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// The fundraiser ID this payment is associated with
        /// </summary>
        public string FundraiserId { get; set; }
        
        /// <summary>
        /// The amount of the payment
        /// </summary>
        public decimal Amount { get; set; }
        
        /// <summary>
        /// The currency of the payment
        /// </summary>
        public string Currency { get; set; }
        
        /// <summary>
        /// The status of the payment
        /// </summary>
        public string Status { get; set; }
        
        /// <summary>
        /// The PayPal batch ID if applicable
        /// </summary>
        public string PaypalBatchId { get; set; }
        
        /// <summary>
        /// Any notes associated with the payment
        /// </summary>
        public string Notes { get; set; }
        
        /// <summary>
        /// The date the payment was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
