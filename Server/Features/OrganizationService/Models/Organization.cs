/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using msih.p4g.Server.Common.Models;
using msih.p4g.Server.Features.CampaignService.Model;
using msih.p4g.Server.Features.DonationService.Models;
using System.ComponentModel.DataAnnotations;

namespace msih.p4g.Server.Features.OrganizationService.Models
{
    /// <summary>
    /// Represents a non-profit organization with relevant information and relationships
    /// </summary>
    public class Organization : BaseEntity
    {
        /// <summary>
        /// Legal name of the organization
        /// </summary>
        [MaxLength(200)]
        public string? LegalName { get; set; }

        /// <summary>
        /// Organization's tax ID (EIN for US organizations)
        /// </summary>
        [MaxLength(50)]
        public string? TaxId { get; set; }

        /// <summary>
        /// Organization's website URL
        /// </summary>
        [MaxLength(255)]
        public string? Website { get; set; }

        /// <summary>
        /// Primary email address for the organization
        /// </summary>
        [MaxLength(100)]
        [EmailAddress]
        public string? EmailAddress { get; set; }

        /// <summary>
        /// Primary phone number for the organization
        /// </summary>
        [MaxLength(20)]
        public string? Phone { get; set; }

        /// <summary>
        /// Organization's mission statement
        /// </summary>
        [MaxLength(1000)]
        public string? MissionStatement { get; set; }

        /// <summary>
        /// Organization's short description
        /// </summary>
        [MaxLength(500)]
        public string? ShortDescription { get; set; }

        /// <summary>
        /// The type of non-profit organization (e.g., 501(c)(3), 501(c)(4), etc.)
        /// </summary>
        [MaxLength(50)]
        public string? OrganizationType { get; set; }

        /// <summary>
        /// Logo URL for the organization
        /// </summary>
        [MaxLength(255)]
        public string? LogoUrl { get; set; }

        /// <summary>
        /// Street address of the organization
        /// </summary>
        [MaxLength(200)]
        public string? Street { get; set; }

        /// <summary>
        /// City of the organization
        /// </summary>
        [MaxLength(100)]
        public string? City { get; set; }

        /// <summary>
        /// State/province of the organization
        /// </summary>
        [MaxLength(100)]
        public string? State { get; set; }

        /// <summary>
        /// Postal/zip code of the organization
        /// </summary>
        [MaxLength(20)]
        public string? PostalCode { get; set; }

        /// <summary>
        /// Country of the organization
        /// </summary>
        [MaxLength(100)]
        public string? Country { get; set; }

        /// <summary>
        /// Navigation property for campaigns associated with this organization
        /// </summary>
        public virtual ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();

        /// <summary>
        /// Navigation property for donations made to this organization
        /// </summary>
        public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();
    }
}
