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
using msih.p4g.Server.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace msih.p4g.Server.Features.Base.PaymentService.Models
{
    /// <summary>
    /// Represents a payment transaction in the system
    /// </summary>
    public class PaymentTransaction : BaseEntity
    {
        /// <summary>
        /// Unique transaction identifier from the payment provider
        /// </summary>
        [Required]
        [MaxLength(100)]
        public required string TransactionId { get; set; }

        /// <summary>
        /// The payment provider used for this transaction
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Provider { get; set; }

        /// <summary>
        /// Amount of the transaction
        /// </summary>
        [Required]
        public decimal Amount { get; set; }

        /// <summary>
        /// Currency code (e.g., USD, EUR)
        /// </summary>
        [Required]
        [MaxLength(3)]
        public string Currency { get; set; }

        /// <summary>
        /// Current status of the transaction
        /// </summary>
        [Required]
        public PaymentStatus Status { get; set; }

        /// <summary>
        /// Optional description of the transaction
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// The date and time when the payment was processed
        /// </summary>
        public DateTime ProcessedOn { get; set; }

        /// <summary>
        /// Email of the customer who made the payment
        /// </summary>
        [MaxLength(255)]
        public string? CustomerEmail { get; set; }

        /// <summary>
        /// Any additional data related to this payment
        /// </summary>
        [MaxLength(4000)]
        public string? AdditionalData { get; set; }

        /// <summary>
        /// Error message if payment failed
        /// </summary>
        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Reference to the order or item being paid for
        /// </summary>
        [MaxLength(100)]
        public string? OrderReference { get; set; }
    }

    /// <summary>
    /// Represents the status of a payment transaction
    /// </summary>
    public enum PaymentStatus
    {
        /// <summary>
        /// Payment is pending or in progress
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Payment has been authorized but not settled
        /// </summary>
        Authorized = 1,

        /// <summary>
        /// Payment has been successfully completed
        /// </summary>
        Completed = 2,

        /// <summary>
        /// Payment has failed
        /// </summary>
        Failed = 3,

        /// <summary>
        /// Payment has been refunded
        /// </summary>
        Refunded = 4,

        /// <summary>
        /// Payment has been partially refunded
        /// </summary>
        PartiallyRefunded = 5,

        /// <summary>
        /// Payment has been voided or canceled
        /// </summary>
        Voided = 6,

        /// <summary>
        /// Payment is being settled
        /// </summary>
        Settling = 7,

        /// <summary>
        /// Payment has been settled
        /// </summary>
        Settled = 8
    }
}
