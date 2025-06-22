// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

using msih.p4g.Server.Common.Models;
using msih.p4g.Server.Features.Base.ProfileService.Model;
using msih.p4g.Server.Features.DonorService.Model;
using msih.p4g.Server.Features.FundraiserService.Model;
using System.ComponentModel.DataAnnotations;

namespace msih.p4g.Server.Features.Base.UserService.Models
{
    public enum UserRole
    {
        Donor,
        Fundraiser,
        Admin
    }

    public class User : BaseEntity
    {
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public UserRole Role { get; set; }

        public virtual Profile Profile { get; set; }

        public virtual Donor Donor { get; set; }

        public virtual Fundraiser Fundraiser { get; set; }

        public void ChangeRole(UserRole newRole)
        {
            Role = newRole;
        }

        // Email verification properties
        public bool EmailConfirmed { get; set; } = false;
        public DateTime? EmailConfirmedAt { get; set; }
        public DateTime? LastEmailVerificationSentAt { get; set; }

    }
}
