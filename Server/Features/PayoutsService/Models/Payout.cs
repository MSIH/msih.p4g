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
using msih.p4g.Server.Features.PayoutService.Models.PayPal;
using MSIH.Core.Common.Models;
using AccountFormat = msih.p4g.Server.Features.FundraiserService.Model.AccountFormat;
using AccountType = msih.p4g.Server.Features.FundraiserService.Model.AccountType;

namespace msih.p4g.Server.Features.PayoutService.Models
{
    /// <summary>
    /// Represents a Payout/payout to a fundraiser
    /// </summary>
    public class Payout : BaseEntity
    {
        /// <summary>
        /// The ID of the fundraiser receiving the Payout
        /// </summary>
        public string FundraiserId { get; set; } = null!;

        public string? PayoutAccount { get; set; }

        public AccountType? PayoutAccountType { get; set; }

        public AccountFormat? PayoutAccountFormat { get; set; }

        /// <summary>
        /// The amount to be paid
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The currency code (e.g., USD)
        /// </summary>
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// The current status of the Payout
        /// </summary>
        public PayPalBatchStatusEnum BatchStatus { get; set; } = PayPalBatchStatusEnum.PENDING;

        /// The current status of the Payout
        /// </summary>
        public PayPalTransactionStatusEnum TransactionStatus { get; set; } = PayPalTransactionStatusEnum.PENDING;

        public decimal? FeeAmount { get; set; }

        /// <summary>
        /// The PayPal batch ID for batch payouts
        /// </summary>
        public string? PaypalSenderId { get; set; } = null;

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
        /// When the Payout record was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When the Payout was processed with PayPal
        /// </summary>
        public DateTime? ProcessedAt { get; set; }

        /// <summary>
        /// Optional notes about the Payout
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Error message if Payout processing failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Indicates whether this Payout is part of a batch
        /// </summary>
        public bool IsBatchPayout { get; set; } = false;
    }
}
