/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Collections.Generic;

namespace MSIH.Core.Services.Message.Models
{
    /// <summary>
    /// Represents the relationship between a message and the template used to create it
    /// </summary>
    public class MessageTemplateUsage
    {
        /// <summary>
        /// Gets or sets the message ID
        /// </summary>
        [Key]
        [ForeignKey("Message")]
        public int MessageId { get; set; }

        /// <summary>
        /// Gets or sets the template ID
        /// </summary>
        [ForeignKey("Template")]
        public int TemplateId { get; set; }

        /// <summary>
        /// Gets or sets the placeholder values used when creating the message
        /// </summary>
        public string PlaceholderValuesJson { get; set; } = "{}";

        /// <summary>
        /// Gets or sets the navigation property to the message
        /// </summary>
        public virtual Message Message { get; set; }

        /// <summary>
        /// Gets or sets the navigation property to the template
        /// </summary>
        public virtual MessageTemplate Template { get; set; }

        /// <summary>
        /// Gets the placeholder values as a dictionary
        /// </summary>
        [NotMapped]
        public Dictionary<string, string> PlaceholderValues
        {
            get
            {
                if (string.IsNullOrEmpty(PlaceholderValuesJson))
                {
                    return new Dictionary<string, string>();
                }
                return JsonSerializer.Deserialize<Dictionary<string, string>>(PlaceholderValuesJson);
            }
            set
            {
                PlaceholderValuesJson = JsonSerializer.Serialize(value);
            }
        }
    }
}
