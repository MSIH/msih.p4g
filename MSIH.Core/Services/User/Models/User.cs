// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

using MSIH.Core.Common.Models;
using System.ComponentModel.DataAnnotations;
using ProfileEntity = MSIH.Core.Services.Profile.Model.Profile;

namespace MSIH.Core.Services.User.Models
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

        public virtual ProfileEntity Profile { get; set; }
        
        // Navigation properties to other entities
        public virtual object Donor { get; set; }  // TODO: Replace with proper type when Donor entity is migrated
        public virtual object Fundraiser { get; set; }  // TODO: Replace with proper type when Fundraiser entity is migrated

        public void ChangeRole(UserRole newRole)
        {
            Role = newRole;
        }

        // Email verification properties
        public bool EmailConfirmed { get; set; } = false;
        public DateTime? EmailConfirmedAt { get; set; }
        public DateTime? LastEmailVerificationSentAt { get; set; }
        public string? EmailVerificationToken { get; set; }

    }
}
