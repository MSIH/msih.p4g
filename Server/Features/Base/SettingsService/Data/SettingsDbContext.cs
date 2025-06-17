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
using Microsoft.EntityFrameworkCore;
using msih.p4g.Server.Features.Base.SettingsService.Model;

namespace msih.p4g.Server.Common.Data
{
    /// <summary>
    /// Partial DbContext implementation for Setting entity
    /// </summary>
    public partial class ApplicationDbContext
    {
        /// <summary>
        /// Gets or sets the Settings DbSet
        /// </summary>
        public DbSet<Setting> Settings { get; set; }

        /// <summary>
        /// Configure the Setting entity
        /// </summary>
        partial void ConfigureSettingsModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Setting>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Value).HasMaxLength(1000);
                entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
                entity.Property(e => e.ModifiedBy).HasMaxLength(100);
            });
        }
    }
}


