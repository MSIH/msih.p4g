// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

using System.ComponentModel.DataAnnotations;

namespace msih.p4g.Shared.Models
{
    /// <summary>
    /// Represents a user profile with contact info and consents.
    /// </summary>
    public class Profile : BaseEntity
    {
        // Navigation to User
        public int UserId { get; set; }
        public virtual msih.p4g.Server.Features.Base.UserService.Models.User User { get; set; }

        // Profile fields
        public int? Age { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
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
    }
}
