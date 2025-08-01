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
using msih.p4g.Shared.Models.PayoutService;

namespace msih.p4g.Server.Features.Base.PayoutService.Models
{
    /// <summary>
    /// DTO for Payout information
    /// </summary>
    public class PayoutDto
    {
        /// <summary>
        /// The unique identifier for the payout
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The ID of the fundraiser receiving the payout
        /// </summary>
        public string FundraiserId { get; set; } = null!;

        /// <summary>
        /// The payout account (e.g., PayPal email address)
        /// </summary>
        public string? PayoutAccount { get; set; }

        /// <summary>
        /// The type of payout account (e.g., PayPal)
        /// </summary>
        public msih.p4g.Server.Features.FundraiserService.Model.AccountType? PayoutAccountType { get; set; }

        /// <summary>
        /// The format of the payout account (e.g., Email)
        /// </summary>
        public msih.p4g.Server.Features.FundraiserService.Model.AccountFormat? PayoutAccountFormat { get; set; }

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

        /// <summary>
        /// Error message if payout processing failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Indicates whether this payout is part of a batch
        /// </summary>
        public bool IsBatchPayout { get; set; }
    }
}
