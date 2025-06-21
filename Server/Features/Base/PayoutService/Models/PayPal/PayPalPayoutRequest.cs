/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace msih.p4g.Server.Features.Base.PayoutService.Models.PayPal
{
    /// <summary>
    /// PayPal Payout API request model
    /// </summary>
    public class PayPalPayoutRequest
    {
        /// <summary>
        /// The subject that appears in the email the recipient receives
        /// </summary>
        [JsonPropertyName("sender_batch_header")]
        public PayPalSenderBatchHeader SenderBatchHeader { get; set; } = new PayPalSenderBatchHeader();

        /// <summary>
        /// List of payout items
        /// </summary>
        [JsonPropertyName("items")]
        public List<PayPalPayoutItem> Items { get; set; } = new List<PayPalPayoutItem>();
    }

    /// <summary>
    /// PayPal Sender Batch Header
    /// </summary>
    public class PayPalSenderBatchHeader
    {
        /// <summary>
        /// The sender-specified ID number. Required for non-synchronous/immediate batch payout
        /// </summary>
        [JsonPropertyName("sender_batch_id")]
        public string SenderBatchId { get; set; } = null!;

        /// <summary>
        /// The subject line of the email that PayPal sends when payment is made
        /// </summary>
        [JsonPropertyName("email_subject")]
        public string EmailSubject { get; set; } = null!;

        /// <summary>
        /// The message in the email that PayPal sends when payment is made
        /// </summary>
        [JsonPropertyName("email_message")]
        public string? EmailMessage { get; set; }
    }

    /// <summary>
    /// PayPal Payout Item
    /// </summary>
    public class PayPalPayoutItem
    {
        /// <summary>
        /// The recipient type. Set to EMAIL for payout to the recipient's PayPal account
        /// </summary>
        [JsonPropertyName("recipient_type")]
        public string RecipientType { get; set; } = "EMAIL";

        /// <summary>
        /// The amount of money to pay the recipient
        /// </summary>
        [JsonPropertyName("amount")]
        public PayPalAmount Amount { get; set; } = new PayPalAmount();

        /// <summary>
        /// A sender-specified note for notifications
        /// </summary>
        [JsonPropertyName("note")]
        public string? Note { get; set; }

        /// <summary>
        /// A sender-specified ID number. Tracks the payout in an accounting system
        /// </summary>
        [JsonPropertyName("sender_item_id")]
        public string SenderItemId { get; set; } = null!;

        /// <summary>
        /// The recipient's email address. The receiver must have a PayPal account with this email address
        /// </summary>
        [JsonPropertyName("receiver")]
        public string Receiver { get; set; } = null!;
    }

    /// <summary>
    /// PayPal Amount
    /// </summary>
    public class PayPalAmount
    {
        /// <summary>
        /// The three-character ISO-4217 currency code
        /// </summary>
        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// The amount to pay
        /// </summary>
        [JsonPropertyName("value")]
        public string Value { get; set; } = null!;
    }
}
