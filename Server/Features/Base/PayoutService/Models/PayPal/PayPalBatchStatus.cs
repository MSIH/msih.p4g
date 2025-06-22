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
using System.Text.Json.Serialization;

namespace msih.p4g.Server.Features.Base.PayoutService.Models.PayPal
{
    /// <summary>
    /// PayPal Batch Status response from API
    /// </summary>
    public class PayPalBatchStatus
    {
        /// <summary>
        /// The PayPal-generated ID for the batch payout
        /// </summary>
        [JsonPropertyName("batch_header")]
        public PayPalBatchStatusHeader BatchHeader { get; set; } = new PayPalBatchStatusHeader();

        /// <summary>
        /// Details about the payout items in the batch
        /// </summary>
        [JsonPropertyName("items")]
        public List<PayPalBatchStatusItem>? Items { get; set; }

        /// <summary>
        /// Collection of links that callers can use to manage the payout
        /// </summary>
        [JsonPropertyName("links")]
        public List<PayPalLink>? Links { get; set; }


    }
    public enum PayPalBatchStatusEnum
    {
        DENIED,
        PENDING,
        PROCESSING,
        SUCCESS,
        CANCELED,
        ERROR

    }

    public enum PayPalTransactionStatusEnum
    {
        //            SUCCESS    Funds have been credited to the recipient’s account.
        //FAILED  This payout request has failed, so funds were not deducted from the sender’s account.
        //PENDING Payout request has been submitted and is being processed. Recipient will get the funds once the request is processed successfully, else the funds will be returned to you.
        //UNCLAIMED   The recipient for this payout does not have a PayPal account. A link to sign up for a PayPal account was sent to the recipient. However, if the recipient does not claim this payout within 30 days, the funds are returned to your account.
        //RETURNED    Funds have been returned to your account. This can be because the recipient has not claimed this payout, or you have cancelled the payout.
        //ONHOLD  This payout request is being reviewed and is on hold.
        //BLOCKED This payout request has been blocked.
        //REFUNDED    Funds have been refunded back to your account. This is because the recipient(PayPal Business verified account) has issued a refund for the payout initiated by you.
        //REVERSED This payout request was reversed.This status is specific to web uploads.
        SUCCESS,
        FAILED,
        PENDING,
        UNCLAIMED,
        RETURNED,
        ONHOLD,
        BLOCKED,
        REFUNDED,
        REVERSED
    }

    /// <summary>
    /// Batch Status Header from PayPal
    /// </summary>
    public class PayPalBatchStatusHeader
    {
        /// <summary>
        /// The PayPal-generated ID for the batch payout
        /// </summary>
        [JsonPropertyName("payout_batch_id")]
        public string PayoutBatchId { get; set; } = null!;

        /// <summary>
        /// The batch status
        /// </summary>
        [JsonPropertyName("batch_status")]
        public string BatchStatus { get; set; } = null!;

        /// <summary>
        /// Time the batch payout was processed
        /// </summary>
        [JsonPropertyName("time_processed")]
        public string? TimeProcessed { get; set; }

        /// <summary>
        /// Time the batch payout was completed
        /// </summary>
        [JsonPropertyName("time_completed")]
        public string? TimeCompleted { get; set; }

        /// <summary>
        /// The sender-specified batch header
        /// </summary>
        [JsonPropertyName("sender_batch_header")]
        public PayPalSenderBatchHeaderResponse SenderBatchHeader { get; set; } = new PayPalSenderBatchHeaderResponse();

        /// <summary>
        /// The payout item count
        /// </summary>
        [JsonPropertyName("payout_item_count")]
        public int? PayoutItemCount { get; set; }

        /// <summary>
        /// The amount of time in seconds for which to try a payout
        /// </summary>
        [JsonPropertyName("amount")]
        public PayPalAmount? Amount { get; set; }
    }

    /// <summary>
    /// Batch Status Item from PayPal
    /// </summary>
    public class PayPalBatchStatusItem
    {
        /// <summary>
        /// The PayPal-generated ID for the payout item
        /// </summary>
        [JsonPropertyName("payout_item_id")]
        public string PayoutItemId { get; set; } = null!;

        /// <summary>
        /// The transaction status
        /// </summary>
        [JsonPropertyName("transaction_status")]
        public string TransactionStatus { get; set; } = null!;

        /// <summary>
        /// The payout item status
        /// </summary>
        [JsonPropertyName("payout_item_fee")]
        public PayPalAmount? PayoutItemFee { get; set; }

        /// <summary>
        /// The payout item
        /// </summary>
        [JsonPropertyName("payout_item")]
        public PayPalPayoutItem? PayoutItem { get; set; }

        /// <summary>
        /// The payout batch ID
        /// </summary>
        [JsonPropertyName("payout_batch_id")]
        public string PayoutBatchId { get; set; } = null!;

        /// <summary>
        /// The error information if the transaction failed
        /// </summary>
        [JsonPropertyName("errors")]
        public PayPalError? Errors { get; set; }

        /// <summary>
        /// The transaction ID
        /// </summary>
        [JsonPropertyName("transaction_id")]
        public string? TransactionId { get; set; }

        /// <summary>
        /// Item reference ID (typically the payment ID in our system)
        /// </summary>
        [JsonPropertyName("sender_item_id")]
        public string SenderItemId { get; set; } = null!;
    }

    /// <summary>
    /// PayPal Error
    /// </summary>
    public class PayPalError
    {
        /// <summary>
        /// The error name
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// The error message
        /// </summary>
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        /// <summary>
        /// The error details
        /// </summary>
        [JsonPropertyName("details")]
        public List<PayPalErrorDetail>? Details { get; set; }
    }

    /// <summary>
    /// PayPal Error Detail
    /// </summary>
    public class PayPalErrorDetail
    {
        /// <summary>
        /// The field that caused the error
        /// </summary>
        [JsonPropertyName("field")]
        public string? Field { get; set; }

        /// <summary>
        /// The issue
        /// </summary>
        [JsonPropertyName("issue")]
        public string? Issue { get; set; }
    }
}


