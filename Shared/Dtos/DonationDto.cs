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

namespace msih.p4g.Shared.Dtos
{
    /// <summary>
    /// DTO for donation data transfer between client and server
    /// </summary>
    public class DonationDto
    {
        public int Id { get; set; }
        public decimal DonationAmount { get; set; }
        public bool PayTransactionFee { get; set; }
        public decimal PayTransactionFeeAmount { get; set; }
        public bool IsMonthly { get; set; }
        public bool IsAnnual { get; set; }
        public string? DonationMessage { get; set; }
        public string? ReferralCode { get; set; }
        public string? CampaignCode { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsActive { get; set; }



        /// <summary>
        /// Computed property for display purposes
        /// </summary>
        public string RecurrenceType => IsMonthly ? "Monthly" : IsAnnual ? "Annual" : "One-time";

        /// <summary>
        /// Total amount charged to the customer (donation + fees if fees are paid by customer)
        /// </summary>
        public decimal TotalAmount => DonationAmount;

        /// <summary>
        /// Net donation amount received by the organization (total amount minus fees if not paid by customer)
        /// </summary>
        public decimal NetDonationAmount => PayTransactionFee ? DonationAmount - PayTransactionFeeAmount : DonationAmount;
    }
}
