// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

using msih.p4g.Server.Common.Models;
using msih.p4g.Server.Features.Base.ProfileService.Model;
using msih.p4g.Server.Features.DonorService.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [NotMapped]
        public virtual Profile Profile { get; set; }
        [NotMapped]
        public virtual Donor Donor { get; set; }

        public void ChangeRole(UserRole newRole)
        {
            Role = newRole;
        }
    }
}
