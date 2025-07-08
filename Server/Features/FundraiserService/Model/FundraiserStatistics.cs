// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

namespace msih.p4g.Server.Features.FundraiserService.Model
{
    /// <summary>
    /// View model for fundraiser statistics
    /// </summary>
    public class FundraiserStatistics
    {
        /// <summary>
        /// Total number of donations received
        /// </summary>
        public int DonationCount { get; set; }

        /// <summary>
        /// Total amount raised through referrals
        /// </summary>
        public decimal TotalRaised { get; set; }

        /// <summary>
        /// Average donation amount
        /// </summary>
        public decimal AverageDonation { get; set; }

        /// <summary>
        /// List of donations with donor information
        /// </summary>
        public List<DonationInfo> Donations { get; set; } = new List<DonationInfo>();
    }

    /// <summary>
    /// Information about a donation for display in the UI
    /// </summary>
    public class DonationInfo
    {
        /// <summary>
        /// Donation ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Donor's name (First + Last)
        /// </summary>
        public string DonorName { get; set; }

        /// <summary>
        /// Donation amount
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Date of donation
        /// </summary>
        public DateTime DonationDate { get; set; }

        /// <summary>
        /// Message from the donor
        /// </summary>
        public string Message { get; set; }

        public int DonorId { get; set; } // Ensure this matches the expected type in the grouping logic
    }

    /// <summary>
    /// Information about a first-time donor
    /// </summary>
    public class FirstTimeDonorInfo
    {
        /// <summary>
        /// Donor ID
        /// </summary>
        public int DonorId { get; set; }

        /// <summary>
        /// Donor's name
        /// </summary>
        public string DonorName { get; set; }

        /// <summary>
        /// Date of their first donation
        /// </summary>
        public DateTime FirstDonationDate { get; set; }

        /// <summary>
        /// Amount of their first donation
        /// </summary>
        public decimal FirstDonationAmount { get; set; }
    }
}
