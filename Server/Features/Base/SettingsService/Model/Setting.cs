/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using msih.p4g.Server.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace msih.p4g.Server.Features.Base.SettingsService.Model
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
