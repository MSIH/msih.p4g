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
using MSIH.Core.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace MSIH.Core.Services.W9Forms.Models
{
    /// <summary>
    /// Represents a W9 tax form for a user or organization
    /// </summary>
    public class W9Form : BaseEntity
    {
        /// <summary>
        /// Name of entity/individual as shown on tax return
        /// deprecated: use FirstName and LastName instead
        /// </summary>
        public string Name { get; set; }

        [Required]
        [MaxLength(200)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(200)]
        public string LastName { get; set; }

        /// <summary>
        /// Business name/disregarded entity name, if different from above
        /// </summary>
        [MaxLength(200)]
        public string? BusinessName { get; set; }

        /// <summary>
        /// Federal tax classification
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string FederalTaxClassification { get; set; }

        /// <summary>
        /// Whether the entity is an LLC and what tax classification it has chosen (C, S, or P)
        /// </summary>
        [MaxLength(1)]
        public string? LLCTaxClassification { get; set; }

        /// <summary>
        /// Other special instructions for tax classification
        /// </summary>
        [MaxLength(500)]
        public string? OtherClassificationInstructions { get; set; }

        /// <summary>
        /// Contains the 3a checkbox option information (Partnership or Trust/estate)
        /// </summary>
        [MaxLength(50)]
        public string? PartnershipTrustInfo { get; set; }

        /// <summary>
        /// Exemption codes (if any)
        /// </summary>
        [MaxLength(20)]
        public string? ExemptPayeeCode { get; set; }

        /// <summary>
        /// FATCA exemption code (if any)
        /// </summary>
        [MaxLength(20)]
        public string? FATCAExemptionCode { get; set; }

        /// <summary>
        /// Street address
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Address { get; set; }

        /// <summary>
        /// City, state, and ZIP code
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string CityStateZip { get; set; }

        /// <summary>
        /// Optional list of account numbers
        /// </summary>
        [MaxLength(200)]
        public string? AccountNumbers { get; set; }

        /// <summary>
        /// Social Security Number - stored encrypted
        /// </summary>
        [MaxLength(500)]
        public string? SocialSecurityNumber { get; set; }

        /// <summary>
        /// Employer Identification Number - stored encrypted
        /// </summary>
        [MaxLength(500)]
        public string? EmployerIdentificationNumber { get; set; }

        /// <summary>
        /// Date when the form was signed
        /// </summary>
        public DateTime SignedDate { get; set; }

        /// <summary>
        /// Digital signature verification token or process ID
        /// </summary>
        [MaxLength(500)]
        public string? SignatureVerification { get; set; }

        /// <summary>
        /// Associated fundraiser ID
        /// </summary>
        public int? FundraiserId { get; set; }

        /// <summary>
        /// Associated user ID for this W9
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Form status (Draft, Submitted, Approved, Rejected)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Draft";
    }
}
