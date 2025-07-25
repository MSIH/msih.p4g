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
using System.ComponentModel.DataAnnotations;

namespace MSIH.Core.Services.Payments.Models
{
    /// <summary>
    /// Model representing a payment request
    /// </summary>
    public class PaymentRequest
    {
        /// <summary>
        /// The amount to charge
        /// </summary>
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        /// <summary>
        /// The currency code (e.g., USD, EUR)
        /// </summary>
        [Required]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be a 3-letter code")]
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// Description of what the payment is for
        /// </summary>
        [MaxLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// Reference to the order or item being paid for
        /// </summary>
        [MaxLength(100)]
        public string OrderReference { get; set; }

        /// <summary>
        /// Customer's email address
        /// </summary>
        [EmailAddress]
        [MaxLength(255)]
        public string CustomerEmail { get; set; }

        /// <summary>
        /// Payment method nonce provided by Braintree client SDK
        /// </summary>
        [Required]
        public string PaymentMethodNonce { get; set; }

        /// <summary>
        /// Device data collected by Braintree client SDK
        /// </summary>
        public string DeviceData { get; set; }
    }

    /// <summary>
    /// Model representing a payment response
    /// </summary>
    public class PaymentResponse
    {
        /// <summary>
        /// Whether the payment was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Transaction ID from the payment provider
        /// </summary>
        public string TransactionId { get; set; }

        /// <summary>
        /// Status of the payment
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Error message, if any
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Amount that was charged
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Currency code (e.g., USD, EUR)
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Last four digits of the credit card
        /// </summary>
        public string CardLastFour { get; set; }

        /// <summary>
        /// Card type (Visa, Mastercard, etc.)
        /// </summary>
        public string CardType { get; set; }
    }

    /// <summary>
    /// Model representing a refund request
    /// </summary>
    public class RefundRequest
    {
        /// <summary>
        /// The original transaction ID to refund
        /// </summary>
        [Required]
        public string TransactionId { get; set; }

        /// <summary>
        /// The amount to refund. If null, the full amount will be refunded
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Reason for the refund
        /// </summary>
        [MaxLength(500)]
        public string Reason { get; set; }
    }

    /// <summary>
    /// Model representing a refund response
    /// </summary>
    public class RefundResponse
    {
        /// <summary>
        /// Whether the refund was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Transaction ID of the refund
        /// </summary>
        public string RefundTransactionId { get; set; }

        /// <summary>
        /// The original transaction ID that was refunded
        /// </summary>
        public string OriginalTransactionId { get; set; }

        /// <summary>
        /// Status of the refund
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Error message, if any
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Amount that was refunded
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Currency code (e.g., USD, EUR)
        /// </summary>
        public string Currency { get; set; }
    }

    /// <summary>
    /// Model representing client token generation request
    /// </summary>
    public class ClientTokenRequest
    {
        /// <summary>
        /// Optional customer ID for Braintree
        /// </summary>
        public string CustomerId { get; set; }
    }

    /// <summary>
    /// Model representing client token response
    /// </summary>
    public class ClientTokenResponse
    {
        /// <summary>
        /// Whether the token generation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The client token string
        /// </summary>
        public string ClientToken { get; set; }

        /// <summary>
        /// Error message, if any
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
