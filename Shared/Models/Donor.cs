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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace msih.p4g.Shared.Models
{
    /// <summary>
    /// Represents a donor and their preferences/contact info.
    /// </summary>
    public class Donor : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string? DonorId { get; set; }
        public string? UserId { get; set; }
        public string? PaymentProcessorDonorId { get; set; }

        // Contact info
        [Phone]
        public string? MobileNumber { get; set; }
        public bool ConsentReceiveText { get; set; } = false;
        public bool UnsubscribeMobile { get; set; } = false;

        [Phone]
        public string? PhoneNumber { get; set; }
        public bool ConsentReceivePhone { get; set; } = false;
        public bool UnsubscribePhone { get; set; } = false;

        [EmailAddress]
        public string? EmailAddress { get; set; }
        public bool ConsentReceiveEmail { get; set; } = false;
        public bool UnsubscribeEmail { get; set; } = false;

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";

        // Communication preferences
        public bool ConsentReceiveMail { get; set; } = false;
        public bool UnsubscribeMail { get; set; } = false;

        // Address
        public AddressModel? Address { get; set; }
    }
}
