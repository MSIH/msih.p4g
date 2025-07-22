/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using MSIH.Core.Common.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace MSIH.Core.Services.Message.Models
{
    /// <summary>
    /// Base message entity for storing both email and SMS messages
    /// </summary>
    public class Message : BaseEntity
    {
        /// <summary>
        /// Gets or sets the message type (Email or SMS)
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string MessageType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the sender of the message
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string From { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the recipient of the message
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string To { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the subject of the message (for emails)
        /// </summary>
        [MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the content of the message
        /// </summary>
        [Required]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the content is HTML (for emails)
        /// </summary>
        public bool IsHtml { get; set; } = false;

        /// <summary>
        /// Gets or sets when the message was sent
        /// </summary>
        public DateTime? SentOn { get; set; }

        /// <summary>
        /// Gets or sets whether the message was successfully sent
        /// </summary>
        public bool IsSent { get; set; } = false;

        /// <summary>
        /// Gets or sets any error message that occurred while sending
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the number of retry attempts
        /// </summary>
        public int RetryCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets when the message should be sent (for scheduled messages)
        /// </summary>
        public DateTime? ScheduledFor { get; set; }
    }
}
