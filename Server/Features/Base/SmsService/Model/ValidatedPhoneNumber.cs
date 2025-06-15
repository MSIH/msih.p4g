/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using msih.p4g.Server.Common.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace msih.p4g.Server.Features.Base.SmsService.Model
{
    /// <summary>
    /// Represents a validated phone number with information about its type and validation status
    /// </summary>
    public class ValidatedPhoneNumber : BaseEntity
    {
        /// <summary>
        /// The phone number in E.164 format (e.g., +12125551234)
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// Indicates whether the phone number is a mobile number
        /// </summary>
        public bool IsMobile { get; set; }
        
        /// <summary>
        /// The carrier of the phone number, if available
        /// </summary>
        [MaxLength(100)]
        public string? Carrier { get; set; }
        
        /// <summary>
        /// The country code of the phone number
        /// </summary>
        [MaxLength(5)]
        public string? CountryCode { get; set; }
        
        /// <summary>
        /// The date and time when the phone number was validated
        /// </summary>
        public DateTime ValidatedOn { get; set; }
        
        /// <summary>
        /// Indicates whether the phone number is valid
        /// </summary>
        public bool IsValid { get; set; }
    }
}
