/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using MSIH.Core.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace MSIH.Core.Services.Setting.Models
{
    /// <summary>
    /// Represents a key-value application setting (for Email, SMS, etc.)
    /// </summary>
    public class Setting : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Key { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Value { get; set; }
    }
}