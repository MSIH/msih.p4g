// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

using Microsoft.EntityFrameworkCore;
using msih.p4g.Server.Common.Models;
using Server.Common.Utilities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace msih.p4g.Server.Features.Base.ProfileService.Model
{
    /// <summary>
    /// Represents a user profile with contact info and consents.
    /// </summary>
    [Index(nameof(ReferralCode), IsUnique = true)] // Move the Index attribute to the class level
    public class Profile : BaseEntity
    {
        // Navigation to User
        public int UserId { get; set; }
        [ForeignKey("UserId")]

        public virtual UserService.Models.User User { get; set; }

        // Profile fields
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        public  string FirstName { get; set; }

        public  string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public AddressModel? Address { get; set; }
        [Phone]
        public string? MobileNumber { get; set; }
        public bool ConsentReceiveText { get; set; } = false;
        public bool UnsubscribeMobile { get; set; } = false;
        public bool ConsentReceiveEmail { get; set; } = false;
        public bool UnsubscribeEmail { get; set; } = false;
        public bool ConsentReceiveMail { get; set; } = false;
        public bool UnsubscribeMail { get; set; } = false;

        /// <summary>
        /// Unique referral code for the profile.
        /// </summary>
        [MaxLength(100)]
        [Required]
        public string ReferralCode { get; set; } = string.Empty;

        /// <summary>
        /// Generates a unique referral code for this profile based on the UserId.
        /// Call this method once when creating the profile.
        /// </summary>
        public void GenerateReferralCode()
        {
            ReferralCode = RandomStringGenerator.Generate(UserId, 6, RandomStringGenerator.CharSet.LowercaseAndUppercase);
        }
    }
}
