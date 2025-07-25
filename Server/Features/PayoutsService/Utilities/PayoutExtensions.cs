/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using msih.p4g.Server.Features.PayoutService.Models;
using msih.p4g.Server.Features.PayoutService.Models.PayPal;

namespace msih.p4g.Server.Features.PayoutService.Utilities
{
    /// <summary>
    /// Extension methods for mapping between Payout entities and DTOs
    /// </summary>
    public static class PayoutExtensions
    {
        /// <summary>
        /// Maps a Payout entity to a PayoutDto
        /// </summary>
        /// <param name="payout">The Payout entity to map</param>
        /// <returns>A PayoutDto object</returns>
        public static PayoutDto ToDto(this Payout payout)
        {
            if (payout == null)
                return null;

            return new PayoutDto
            {
                Id = payout.Id,
                FundraiserId = payout.FundraiserId,
                PayoutAccount = payout.PayoutAccount,
                PayoutAccountType = payout.PayoutAccountType,
                PayoutAccountFormat = payout.PayoutAccountFormat,
                Amount = payout.Amount,
                Currency = payout.Currency,
                // Status field removed as it doesn't exist in the Payout model
                PaypalBatchId = payout.PaypalBatchId,
                PaypalPayoutItemId = payout.PaypalPayoutItemId,
                PaypalTransactionId = payout.PaypalTransactionId,
                CreatedAt = payout.CreatedAt,
                ProcessedAt = payout.ProcessedAt,
                Notes = payout.Notes,
                ErrorMessage = payout.ErrorMessage,
                IsBatchPayout = payout.IsBatchPayout
            };
        }

        /// <summary>
        /// Maps a list of Payout entities to a list of PayoutDtos
        /// </summary>
        /// <param name="payouts">The list of Payout entities to map</param>
        /// <returns>A list of PayoutDto objects</returns>
        public static List<PayoutDto> ToDtoList(this IEnumerable<Payout> payouts)
        {
            return payouts?.Select(p => p.ToDto()).ToList() ?? new List<PayoutDto>();
        }

        /// <summary>
        /// Maps a PayPalBatchStatus entity to a PayPalBatchStatusDto
        /// </summary>
        /// <param name="batchStatus">The PayPalBatchStatus entity to map</param>
        /// <returns>A PayPalBatchStatusDto object</returns>
        public static PayPalBatchStatusDto ToDto(this PayPalBatchStatus batchStatus)
        {
            if (batchStatus == null)
                return null;

            var dto = new PayPalBatchStatusDto
            {
                BatchId = batchStatus.BatchHeader.PayoutBatchId,
                Status = batchStatus.BatchHeader.BatchStatus,
                // TimeCreated and TimeCompleted fields in PayPalBatchStatusHeader are string in the original model
                BatchCreationTime = DateTime.UtcNow, // Default to current time
                BatchCompletionTime = null,
                Currency = batchStatus.BatchHeader.Amount?.Currency ?? string.Empty,
                Items = new List<PayPalBatchItemStatusDto>(),
                SuccessCount = 0,
                ErrorCount = 0
            };

            // Try to parse the creation time if available
            if (batchStatus.BatchHeader.TimeProcessed != null &&
                DateTime.TryParse(batchStatus.BatchHeader.TimeProcessed, out DateTime creationTime))
            {
                dto.BatchCreationTime = creationTime;
            }

            // Try to parse the completion time if available
            if (batchStatus.BatchHeader.TimeCompleted != null &&
                DateTime.TryParse(batchStatus.BatchHeader.TimeCompleted, out DateTime completionTime))
            {
                dto.BatchCompletionTime = completionTime;
            }

            if (batchStatus.BatchHeader.Amount != null && decimal.TryParse(batchStatus.BatchHeader.Amount.Value, out decimal totalAmount))
            {
                dto.TotalAmount = totalAmount;
            }

            // Map items if available
            if (batchStatus.Items != null)
            {
                foreach (var item in batchStatus.Items)
                {
                    var itemDto = new PayPalBatchItemStatusDto
                    {
                        PayoutItemId = item.PayoutItemId,
                        TransactionId = item.TransactionId,
                        Status = item.TransactionStatus,
                        SenderItemId = item.SenderItemId
                    };

                    // Extract amount and currency from PayoutItem if available
                    if (item.PayoutItem?.Amount != null)
                    {
                        if (decimal.TryParse(item.PayoutItem.Amount.Value, out decimal amount))
                        {
                            itemDto.Amount = amount;
                        }
                        itemDto.Currency = item.PayoutItem.Amount.Currency;
                    }

                    // Extract receiver email
                    if (item.PayoutItem != null)
                    {
                        itemDto.ReceiverEmail = item.PayoutItem.Receiver;
                    }

                    dto.Items.Add(itemDto);

                    // Update counts
                    if (item.TransactionStatus == "SUCCESS")
                        dto.SuccessCount++;
                    else if (item.TransactionStatus == "FAILED" || item.TransactionStatus == "BLOCKED" || item.TransactionStatus == "RETURNED")
                        dto.ErrorCount++;
                }
            }

            return dto;
        }
    }
}
