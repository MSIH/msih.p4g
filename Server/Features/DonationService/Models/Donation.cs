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
using System.ComponentModel.DataAnnotations;

namespace msih.p4g.Server.Features.DonationService.Models
{
    /// <summary>
    /// Represents a donation made by a donor.
    /// </summary>
    public class Donation : BaseEntity
    {
        /// <summary>
        /// The amount donated.
        /// </summary>
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        /// <summary>
        /// The donor who made the donation (FK).
        /// </summary>
        [Required]
        public int DonorId { get; set; }

        /// <summary>
        /// The payment transaction associated with this donation (FK).
        /// </summary>
        public int? PaymentTransactionId { get; set; }

        /// <summary>
        /// If true, donor pays the transaction fee.
        /// </summary>
        public bool PayTransactionFee { get; set; }

        /// <summary>
        /// If true, this is a monthly recurring donation.
        /// </summary>
        public bool IsMonthly { get; set; }

        /// <summary>
        /// If true, this is an annual recurring donation.
        /// </summary>
        public bool IsAnnual { get; set; }

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
        /// Optional referral code.
        /// </summary>
        [MaxLength(100)]
        public string? CampaignCode { get; set; }
    }
}
