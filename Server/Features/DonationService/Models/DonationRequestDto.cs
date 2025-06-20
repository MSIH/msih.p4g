// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

using msih.p4g.Server.Features.Base.ProfileService.Model;
using System.ComponentModel.DataAnnotations;

namespace msih.p4g.Server.Features.DonationService.Models
{
    /// <summary>
    /// DTO for processing a donation request from the client.
    /// </summary>
    public class DonationRequestDto
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Mobile { get; set; }

        [Required]
        public AddressModel Address { get; set; }

        [Required]
        public decimal DonationAmount { get; set; }

        public decimal TransactionFee { get; set; }

        public string? DonationMessage { get; set; }
        public bool PayTransactionFee { get; set; }
        public bool IsMonthly { get; set; }
        public bool IsAnnual { get; set; }

        public string? ReferralCode { get; set; }
        public string? CampaignCode { get; set; }

        [Required]
        public string PaymentToken { get; set; }
    }
}
