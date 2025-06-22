/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using System.ComponentModel.DataAnnotations;

namespace msih.p4g.Shared.W9FormService.Dtos
{
    /// <summary>
    /// Data Transfer Object for W9 tax form
    /// </summary>
    public class W9FormDto
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Name of entity/individual as shown on tax return
        /// </summary>
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string Name { get; set; }

        /// <summary>
        /// Business name/disregarded entity name, if different from above
        /// </summary>
        [MaxLength(200, ErrorMessage = "Business name cannot exceed 200 characters")]
        public string? BusinessName { get; set; }

        /// <summary>
        /// Federal tax classification
        /// </summary>
        [Required(ErrorMessage = "Federal tax classification is required")]
        [MaxLength(50, ErrorMessage = "Federal tax classification cannot exceed 50 characters")]
        public string FederalTaxClassification { get; set; }

        /// <summary>
        /// Whether the entity is an LLC and what tax classification it has chosen (C, S, or P)
        /// </summary>
        [MaxLength(1, ErrorMessage = "LLC tax classification should be C, S, or P")]
        public string? LLCTaxClassification { get; set; }

        /// <summary>
        /// Other special instructions for tax classification
        /// </summary>
        [MaxLength(500, ErrorMessage = "Other instructions cannot exceed 500 characters")]
        public string? OtherClassificationInstructions { get; set; }

        /// <summary>
        /// Contains the 3a checkbox option information
        /// </summary>
        [MaxLength(50, ErrorMessage = "Partnership/Trust info cannot exceed 50 characters")]
        public string? PartnershipTrustInfo { get; set; }

        /// <summary>
        /// Exemption codes (if any)
        /// </summary>
        [MaxLength(20, ErrorMessage = "Exempt payee code cannot exceed 20 characters")]
        public string? ExemptPayeeCode { get; set; }

        /// <summary>
        /// FATCA exemption code (if any)
        /// </summary>
        [MaxLength(20, ErrorMessage = "FATCA exemption code cannot exceed 20 characters")]
        public string? FATCAExemptionCode { get; set; }

        /// <summary>
        /// Street address
        /// </summary>
        [Required(ErrorMessage = "Address is required")]
        [MaxLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string Address { get; set; }

        /// <summary>
        /// City, state, and ZIP code
        /// </summary>
        [Required(ErrorMessage = "City, state, and ZIP is required")]
        [MaxLength(200, ErrorMessage = "City, state, and ZIP cannot exceed 200 characters")]
        public string CityStateZip { get; set; }

        /// <summary>
        /// Optional list of account numbers
        /// </summary>
        [MaxLength(200, ErrorMessage = "Account numbers cannot exceed 200 characters")]
        public string? AccountNumbers { get; set; }

        /// <summary>
        /// Social Security Number (masked for display)
        /// </summary>
        [MaxLength(11, ErrorMessage = "Social Security Number format should be XXX-XX-XXXX")]
        [RegularExpression(@"^\d{3}-\d{2}-\d{4}$|^$", ErrorMessage = "SSN must be in format XXX-XX-XXXX")]
        public string? SocialSecurityNumber { get; set; }

        /// <summary>
        /// Employer Identification Number (masked for display)
        /// </summary>
        [MaxLength(10, ErrorMessage = "Employer Identification Number format should be XX-XXXXXXX")]
        [RegularExpression(@"^\d{2}-\d{7}$|^$", ErrorMessage = "EIN must be in format XX-XXXXXXX")]
        public string? EmployerIdentificationNumber { get; set; }

        /// <summary>
        /// Date when form was signed
        /// </summary>
        public DateTime SignedDate { get; set; } = DateTime.UtcNow;

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
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        /// <summary>
        /// Form status (Draft, Submitted, Approved, Rejected)
        /// </summary>
        [Required(ErrorMessage = "Status is required")]
        [MaxLength(50)]
        public string Status { get; set; } = "Draft";
        
        // Audit fields
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
    }
}
