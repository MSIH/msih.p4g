/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using System;
using System.Collections.Generic;

namespace msih.p4g.Shared.Models.PayoutService
{
    /// <summary>
    /// DTO for PayPal batch payout status information
    /// </summary>
    public class PayPalBatchStatusDto
    {
        /// <summary>
        /// The batch ID
        /// </summary>
        public string BatchId { get; set; } = null!;
        
        /// <summary>
        /// The batch status (e.g., SUCCESS, PENDING, PROCESSING, etc.)
        /// </summary>
        public string Status { get; set; } = null!;
        
        /// <summary>
        /// Time when the batch was created
        /// </summary>
        public DateTime BatchCreationTime { get; set; }
        
        /// <summary>
        /// Time when the batch was last updated
        /// </summary>
        public DateTime? BatchCompletionTime { get; set; }
        
        /// <summary>
        /// Total amount of the batch payout
        /// </summary>
        public decimal TotalAmount { get; set; }
        
        /// <summary>
        /// Currency of the batch payout
        /// </summary>
        public string Currency { get; set; } = null!;
        
        /// <summary>
        /// Number of successful items in the batch
        /// </summary>
        public int SuccessCount { get; set; }
        
        /// <summary>
        /// Number of failed items in the batch
        /// </summary>
        public int ErrorCount { get; set; }
        
        /// <summary>
        /// Detailed status information for each item in the batch
        /// </summary>
        public List<PayPalBatchItemStatusDto> Items { get; set; } = new List<PayPalBatchItemStatusDto>();
    }
    
    /// <summary>
    /// DTO for status information of an individual item in a PayPal batch payout
    /// </summary>
    public class PayPalBatchItemStatusDto
    {
        /// <summary>
        /// The payout item ID
        /// </summary>
        public string PayoutItemId { get; set; } = null!;
        
        /// <summary>
        /// The transaction ID
        /// </summary>
        public string? TransactionId { get; set; }
        
        /// <summary>
        /// The item status (e.g., SUCCESS, FAILED, etc.)
        /// </summary>
        public string Status { get; set; } = null!;
        
        /// <summary>
        /// The payout amount
        /// </summary>
        public decimal Amount { get; set; }
        
        /// <summary>
        /// The currency code
        /// </summary>
        public string Currency { get; set; } = null!;
        
        /// <summary>
        /// The recipient's PayPal email
        /// </summary>
        public string ReceiverEmail { get; set; } = null!;
        
        /// <summary>
        /// Item reference ID (typically the payment ID in our system)
        /// </summary>
        public string SenderItemId { get; set; } = null!;
    }
}
