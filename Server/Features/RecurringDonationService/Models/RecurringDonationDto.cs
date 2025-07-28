/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

using System.ComponentModel.DataAnnotations;

namespace msih.p4g.Server.Features.RecurringDonationService.Models
{
    /// <summary>
    /// DTO for creating a new recurring donation.
    /// </summary>
    public class CreateRecurringDonationDto
    {
        /// <summary>
        /// The amount to be charged for each recurring donation.
        /// </summary>
        [Required]
        [Range(25.00, double.MaxValue, ErrorMessage = "Recurring donation amount must be at least $25.00")]
        public decimal Amount { get; set; }

        /// <summary>
        /// The frequency of the recurring donation.
        /// </summary>
        [Required]
        public RecurringFrequency Frequency { get; set; }

        /// <summary>
        /// When the recurring donation should start processing.
        /// </summary>
        [Required]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Optional end date for the recurring donation.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// The payment method token from the payment provider.
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string PaymentMethodToken { get; set; }

        /// <summary>
        /// If true, donor pays the transaction fee.
        /// </summary>
        public bool PayTransactionFee { get; set; }

        /// <summary>
        /// Amount of transaction fee paid by donor.
        /// </summary>
        public decimal PayTransactionFeeAmount { get; set; }

        /// <summary>
        /// Optional message from the donor.
        /// </summary>
        [MaxLength(1000)]
        public string? DonationMessage { get; set; }

        /// <summary>
        /// Optional referral code.
        /// </summary>
        [MaxLength(100)]
        public string? ReferralCode { get; set; }

        /// <summary>
        /// Optional campaign code.
        /// </summary>
        [MaxLength(100)]
        public string? CampaignCode { get; set; }

        /// <summary>
        /// Campaign ID if this recurring donation is associated with a specific campaign.
        /// </summary>
        public int? CampaignId { get; set; }
    }

    /// <summary>
    /// DTO for updating a recurring donation.
    /// </summary>
    public class UpdateRecurringDonationDto
    {
        /// <summary>
        /// The new amount for the recurring donation.
        /// </summary>
        [Range(25.00, double.MaxValue, ErrorMessage = "Recurring donation amount must be at least $25.00")]
        public decimal? Amount { get; set; }

        /// <summary>
        /// New payment method token.
        /// </summary>
        [MaxLength(500)]
        public string? PaymentMethodToken { get; set; }

        /// <summary>
        /// If true, donor pays the transaction fee.
        /// </summary>
        public bool? PayTransactionFee { get; set; }

        /// <summary>
        /// Amount of transaction fee paid by donor.
        /// </summary>
        public decimal? PayTransactionFeeAmount { get; set; }

        /// <summary>
        /// Optional message from the donor.
        /// </summary>
        [MaxLength(1000)]
        public string? DonationMessage { get; set; }

        /// <summary>
        /// Optional end date for the recurring donation.
        /// </summary>
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// DTO for recurring donation responses.
    /// </summary>
    public class RecurringDonationResponseDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public RecurringFrequency Frequency { get; set; }
        public string Currency { get; set; }
        public RecurringDonationStatus Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime NextProcessDate { get; set; }
        public DateTime? LastProcessedDate { get; set; }
        public int SuccessfulDonationsCount { get; set; }
        public int FailedAttemptsCount { get; set; }
        public bool PayTransactionFee { get; set; }
        public decimal PayTransactionFeeAmount { get; set; }
        public string? DonationMessage { get; set; }
        public string? ReferralCode { get; set; }
        public string? CampaignCode { get; set; }
        public int? CampaignId { get; set; }
        public string? CampaignName { get; set; }
        public string? LastErrorMessage { get; set; }
        public DateTime? CancelledDate { get; set; }
        public string? CancelledBy { get; set; }
        public string? CancellationReason { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }

    /// <summary>
    /// DTO for cancelling a recurring donation.
    /// </summary>
    public class CancelRecurringDonationDto
    {
        /// <summary>
        /// Reason for cancellation.
        /// </summary>
        [MaxLength(1000)]
        public string? Reason { get; set; }
    }
}