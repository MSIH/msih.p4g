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
using msih.p4g.Server.Features.FundraiserService.Model;

namespace msih.p4g.Server.Common.Data
{
    /// <summary>
    /// Partial DbContext implementation for Fundraiser entity
    /// </summary>
    public partial class ApplicationDbContext
    {
        /// <summary>
        /// Gets or sets the Fundraisers DbSet
        /// </summary>
        public DbSet<Fundraiser> Fundraisers { get; set; }

        /// <summary>
        /// Configure the Fundraiser entity
        /// </summary>
        partial void ConfigureFundraiserModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Fundraiser>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PayPalAccount).HasMaxLength(200);
                entity.Property(e => e.W9Document).HasMaxLength(500);
            });
        }
    }
}
