/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using msih.p4g.Server.Common.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace msih.p4g.Server.Features.Base.MessageService.Models
{
    /// <summary>
    /// Template for messages that can be used for emails and SMS
    /// </summary>
    public class MessageTemplate : BaseEntity
    {
        /// <summary>
        /// Gets or sets the name of the template
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the template
        /// </summary>
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the message type (Email or SMS)
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string MessageType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the template category (ThankYou, DonationSolicitation, AnnualStatement, etc.)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the default subject line (for emails)
        /// </summary>
        [MaxLength(200)]
        public string DefaultSubject { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the template content with placeholders for dynamic content
        /// </summary>
        [Required]
        public string TemplateContent { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the content is HTML (for emails)
        /// </summary>
        public bool IsHtml { get; set; } = false;

        /// <summary>
        /// Gets or sets the list of available placeholders that can be used in this template
        /// </summary>
        [MaxLength(1000)]
        public string AvailablePlaceholders { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether this is the default template for its category
        /// </summary>
        public bool IsDefault { get; set; } = false;
        
        /// <summary>
        /// Gets or sets the default sender (email or phone number)
        /// </summary>
        [MaxLength(100)]
        public string DefaultSender { get; set; } = string.Empty;
    }
}
