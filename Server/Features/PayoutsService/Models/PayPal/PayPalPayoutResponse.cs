/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace msih.p4g.Server.Features.PayoutService.Models.PayPal
{
    /// <summary>
    /// PayPal Payout API response model
    /// </summary>
    public class PayPalPayoutResponse
    {
        /// <summary>
        /// The PayPal batch ID
        /// </summary>
        [JsonPropertyName("batch_header")]
        public PayPalBatchHeader BatchHeader { get; set; } = new PayPalBatchHeader();

        /// <summary>
        /// Collection of links that callers can use to manage the payout
        /// </summary>
        [JsonPropertyName("links")]
        public List<PayPalLink>? Links { get; set; }
    }

    /// <summary>
    /// PayPal Batch Header in response
    /// </summary>
    public class PayPalBatchHeader
    {
        /// <summary>
        /// The PayPal-generated ID for the batch payout
        /// </summary>
        [JsonPropertyName("payout_batch_id")]
        public string PayoutBatchId { get; set; } = null!;

        /// <summary>
        /// The sender-specified ID for the batch payout
        /// </summary>
        [JsonPropertyName("sender_batch_header")]
        public PayPalSenderBatchHeaderResponse SenderBatchHeader { get; set; } = new PayPalSenderBatchHeaderResponse();

        /// <summary>
        /// The number of items in the batch payout
        /// </summary>
        [JsonPropertyName("batch_status")]
        public string BatchStatus { get; set; } = null!;
    }

    /// <summary>
    /// PayPal Sender Batch Header in response
    /// </summary>
    public class PayPalSenderBatchHeaderResponse
    {
        /// <summary>
        /// The sender-specified ID for the batch payout
        /// </summary>
        [JsonPropertyName("sender_batch_id")]
        public string SenderBatchId { get; set; } = null!;

        /// <summary>
        /// The email subject that PayPal uses for payout emails
        /// </summary>
        [JsonPropertyName("email_subject")]
        public string? EmailSubject { get; set; }
    }

    /// <summary>
    /// PayPal Link
    /// </summary>
    public class PayPalLink
    {
        /// <summary>
        /// The link relation
        /// </summary>
        [JsonPropertyName("rel")]
        public string Rel { get; set; } = null!;

        /// <summary>
        /// The link URL
        /// </summary>
        [JsonPropertyName("href")]
        public string Href { get; set; } = null!;

        /// <summary>
        /// The HTTP method for the link
        /// </summary>
        [JsonPropertyName("method")]
        public string Method { get; set; } = null!;
    }
}
