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
using msih.p4g.Server.Common.Models;
using msih.p4g.Server.Features.Base.PaymentService.Models;
using msih.p4g.Server.Features.CampaignService.Model;
using msih.p4g.Server.Features.DonorService.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace msih.p4g.Server.Features.DonationService.Models
{
    /// <summary>
    /// Represents a donation made by a donor.
    /// </summary>
    public class Donation : BaseEntity
    {
        /// <summary>
        /// The total amount charged to the donor (Amount + TransactionFeeAmount if PayTransactionFee is true).
        /// </summary>
        [Required]
        [Range(25.00, double.MaxValue, ErrorMessage = "Donation amount must be at least $25.00")]
        public decimal DonationAmount { get; set; }

        /// <summary>
        /// If true, donor pays the transaction fee.
        public bool PayTransactionFee { get; set; }

        /// <summary>
        /// Amount of transaction fee paid by Donor that disaplyed on screen.
        public decimal PayTransactionFeeAmount { get; set; }

        /// <summary>
        /// The donor who made the donation (FK).
        /// </summary>
        [Required]
        public int DonorId { get; set; }

        /// <summary>
        /// Navigation property for the Donor who made the donation.
        /// </summary>
        [ForeignKey("DonorId")]
        public virtual Donor Donor { get; set; }

        /// <summary>
        /// The payment transaction associated with this donation (FK).
        /// </summary>
        public int? PaymentTransactionId { get; set; }

        /// <summary>
        /// Navigation property for the associated payment transaction.
        /// </summary>
        [ForeignKey("PaymentTransactionId")]
        public virtual PaymentTransaction PaymentTransaction { get; set; }


        /// <summary>
        /// If true, this is a monthly recurring donation.
        /// </summary>
        public bool IsMonthly { get; set; }

        /// <summary>
        /// If true, this is an annual recurring donation.
        /// </summary>
        public bool IsAnnual { get; set; }

        /// <summary>
        /// For recurring donations: when the next payment should be processed.
        /// </summary>
        public DateTime? NextProcessDate { get; set; }

        /// <summary>
        /// For recurring donations: stored payment method token for automatic processing.
        /// </summary>
        [MaxLength(500)]
        public string? RecurringPaymentToken { get; set; }

        /// <summary>
        /// For recurring donation payments: ID of the parent recurring donation setup.
        /// Null for the original setup donation, populated for each automatic payment.
        /// </summary>
        public int? ParentRecurringDonationId { get; set; }

        /// <summary>
        /// Navigation property for the parent recurring donation (setup record).
        /// </summary>
        [ForeignKey("ParentRecurringDonationId")]
        public virtual Donation ParentRecurringDonation { get; set; }

        /// <summary>
        /// Navigation property for child recurring donation payments.
        /// </summary>
        public virtual ICollection<Donation> RecurringPayments { get; set; } = new List<Donation>();

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
        /// Campaign ID if this donation is associated with a specific campaign.
        /// </summary>
        public int? CampaignId { get; set; }

        /// <summary>
        /// Navigation property for the associated campaign.
        /// </summary>
        [ForeignKey("CampaignId")]
        public virtual Campaign Campaign { get; set; }
    }
}
