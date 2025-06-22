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
using msih.p4g.Server.Features.Base.PayoutService.Models;
using msih.p4g.Server.Features.Base.PayoutService.Models.PayPal;

namespace msih.p4g.Server.Features.Base.PayoutService.Utilities
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
                Status = payout.Status,
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
        public static PayPalBatchStatusDto ToDto(this Models.PayPalBatchStatus batchStatus)
        {
            if (batchStatus == null)
                return null;

            return new PayPalBatchStatusDto
            {
                BatchId = batchStatus.BatchId,
                Status = batchStatus.Status,
                BatchCreationTime = batchStatus.BatchCreationTime,
                BatchCompletionTime = batchStatus.BatchCompletionTime,
                TotalAmount = batchStatus.TotalAmount,
                Currency = batchStatus.Currency,
                SuccessCount = batchStatus.SuccessCount,
                ErrorCount = batchStatus.ErrorCount,
                Items = batchStatus.Items?.Select(i => new PayPalBatchItemStatusDto
                {
                    PayoutItemId = i.PayoutItemId,
                    TransactionId = i.TransactionId,
                    Status = i.Status,
                    Amount = i.Amount,
                    Currency = i.Currency,
                    ReceiverEmail = i.ReceiverEmail,
                    SenderItemId = i.SenderItemId
                }).ToList() ?? new List<PayPalBatchItemStatusDto>()
            };
        }
    }
}
