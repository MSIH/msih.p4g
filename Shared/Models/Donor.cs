/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using System.ComponentModel.DataAnnotations.Schema;

namespace msih.p4g.Shared.Models
{
    /// <summary>
    /// Represents a donor entity linked to a user.
    /// </summary>
    public class Donor : BaseEntity
    {
        public string? DonorId { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual msih.p4g.Server.Features.Base.UserService.Models.User User { get; set; }
        public string? PaymentProcessorDonorId { get; set; }
        // Only donor-specific fields remain. Profile/contact fields are now in Profile.
    }
}
