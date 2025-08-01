// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

using msih.p4g.Server.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace msih.p4g.Server.Features.FundraiserService.Model
{
    /// <summary>
    /// Represents a fundraiser with PayPal info, email, and document.
    /// </summary>
    public class Fundraiser : BaseEntity
    {

        [MaxLength(200)]
        public string? PayoutAccount { get; set; }

        public AccountType? PayoutAccountType { get; set; }

        public AccountFormat? PayoutAccountFormat { get; set; }

        /// <summary>
        /// Path or identifier for the document (e.g., PDF, image).
        /// </summary>
        [MaxLength(500)]
        public string? W9Document { get; set; }

        /// <summary>
        /// Indicates whether this affiliate account is suspended.
        /// </summary>
        public bool IsSuspended { get; set; } = false;

        /// <summary>
        /// The reason for suspension if the account is suspended.
        /// </summary>
        [MaxLength(500)]
        public string? SuspensionReason { get; set; }

        /// <summary>
        /// The date when the account was suspended (if applicable).
        /// </summary>
        public DateTime? SuspendedDate { get; set; }

        public int UserId { get; set; }
        public virtual Base.UserService.Models.User User { get; set; }
    }

    public enum AccountType
    {
        PayPal,
        Venmo
    }

    public enum AccountFormat
    {
        Email,
        Mobile,
        Handle
    }
}
