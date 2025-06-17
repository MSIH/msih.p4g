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
using msih.p4g.Server.Features.DonationService.Models;

namespace msih.p4g.Server.Common.Data
{
    /// <summary>
    /// Partial DbContext implementation for Donation entity
    /// </summary>
    public partial class ApplicationDbContext
    {
        /// <summary>
        /// Gets or sets the Donations DbSet
        /// </summary>
        public DbSet<Donation> Donations { get; set; }

        /// <summary>
        /// Configure the Donation entity
        /// </summary>
        partial void ConfigureDonationModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Donation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).IsRequired().HasPrecision(18, 2);
                entity.Property(e => e.DonationMessage).HasMaxLength(1000);
                entity.Property(e => e.ReferralCode).HasMaxLength(100);
                entity.Property(e => e.CampaignCode).HasMaxLength(100);
                // Add additional configuration as needed
            });
        }
    }
}


