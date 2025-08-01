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
using msih.p4g.Server.Features.DonationService.Models;
using System.ComponentModel.DataAnnotations;

namespace msih.p4g.Server.Features.DonorService.Model
{
    /// <summary>
    /// Represents a donor entity linked to a user.
    /// </summary>
    public class Donor : BaseEntity
    {
        public int UserId { get; set; }
        // Prevent EF Core from managing the User table in this context
        public virtual Base.UserService.Models.User User { get; set; }
        public string? PaymentProcessorDonorId { get; set; }

        [MaxLength(100)]
        public string? ReferralCode { get; set; }

        /// <summary>
        /// Collection of donations made by this donor.
        /// </summary>
        public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();

    }
}
