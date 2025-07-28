/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

using msih.p4g.Server.Common.Models;
using msih.p4g.Server.Features.DonorService.Model;
using msih.p4g.Server.Features.CampaignService.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace msih.p4g.Server.Features.RecurringDonationService.Models
{
    /// <summary>
    /// Represents a recurring donation subscription that processes automatically.
    /// </summary>
    public class RecurringDonation : BaseEntity
    {
        /// <summary>
        /// The donor who set up the recurring donation (FK).
        /// </summary>
        [Required]
        public int DonorId { get; set; }

        /// <summary>
        /// Navigation property for the Donor.
        /// </summary>
        [ForeignKey("DonorId")]
        public virtual Donor Donor { get; set; }

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
        /// Currency code (e.g., USD).
        /// </summary>
        [Required]
        [MaxLength(3)]
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// The status of the recurring donation.
        /// </summary>
        [Required]
        public RecurringDonationStatus Status { get; set; }

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
        /// The next date this recurring donation should be processed.
        /// </summary>
        [Required]
        public DateTime NextProcessDate { get; set; }

        /// <summary>
        /// The last date this recurring donation was successfully processed.
        /// </summary>
        public DateTime? LastProcessedDate { get; set; }

        /// <summary>
        /// Total number of successful donations processed.
        /// </summary>
        public int SuccessfulDonationsCount { get; set; } = 0;

        /// <summary>
        /// Total number of failed donation attempts.
        /// </summary>
        public int FailedAttemptsCount { get; set; } = 0;

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

        /// <summary>
        /// Navigation property for the associated campaign.
        /// </summary>
        [ForeignKey("CampaignId")]
        public virtual Campaign Campaign { get; set; }

        /// <summary>
        /// Error message from the last failed attempt.
        /// </summary>
        [MaxLength(2000)]
        public string? LastErrorMessage { get; set; }

        /// <summary>
        /// When the recurring donation was cancelled (if applicable).
        /// </summary>
        public DateTime? CancelledDate { get; set; }

        /// <summary>
        /// Who cancelled the recurring donation.
        /// </summary>
        [MaxLength(255)]
        public string? CancelledBy { get; set; }

        /// <summary>
        /// Reason for cancellation.
        /// </summary>
        [MaxLength(1000)]
        public string? CancellationReason { get; set; }
    }

    /// <summary>
    /// Frequency options for recurring donations.
    /// </summary>
    public enum RecurringFrequency
    {
        Monthly = 1,
        Annually = 12
    }

    /// <summary>
    /// Status options for recurring donations.
    /// </summary>
    public enum RecurringDonationStatus
    {
        Active = 1,
        Paused = 2,
        Cancelled = 3,
        Failed = 4,
        Expired = 5
    }
}