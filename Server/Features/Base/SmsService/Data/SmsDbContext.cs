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
using msih.p4g.Server.Features.Base.SmsService.Model;

namespace msih.p4g.Server.Common.Data
{
    /// <summary>
    /// Partial DbContext implementation for SMS service related entities
    /// </summary>
    public partial class ApplicationDbContext
    {
        /// <summary>
        /// DbSet for validated phone numbers
        /// </summary>
        public DbSet<ValidatedPhoneNumber> ValidatedPhoneNumbers { get; set; }

        /// <summary>
        /// Configure the SMS entities
        /// </summary>
        partial void ConfigureSmsModel(ModelBuilder modelBuilder)
        {
            // Configure the ValidatedPhoneNumber entity
            modelBuilder.Entity<ValidatedPhoneNumber>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.PhoneNumber).IsUnique();
                entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Carrier).HasMaxLength(100);
                entity.Property(e => e.CountryCode).HasMaxLength(5);

                // Common audit properties from BaseEntity
                entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
                entity.Property(e => e.ModifiedBy).HasMaxLength(100);
            });
        }
    }
}


