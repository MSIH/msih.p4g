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
using msih.p4g.Server.Features.Base.ProfileService.Model;

namespace msih.p4g.Server.Common.Data
{
    /// <summary>
    /// Partial DbContext implementation for Profile entity
    /// </summary>
    public partial class ApplicationDbContext
    {
        /// <summary>
        /// Gets or sets the Profiles DbSet
        /// </summary>
        public DbSet<Profile> Profiles { get; set; }

        /// <summary>
        /// Configure the Profile entity
        /// </summary>
        partial void ConfigureProfileModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Profile>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Age).IsRequired(false);
                entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired(false);
                entity.Property(e => e.LastName).HasMaxLength(100).IsRequired(false);
                entity.Ignore(e => e.FullName);
                entity.Property(e => e.MobileNumber).IsRequired(false);
                entity.Property(e => e.ConsentReceiveText);
                entity.Property(e => e.UnsubscribeMobile);
                entity.Property(e => e.ConsentReceiveEmail);
                entity.Property(e => e.UnsubscribeEmail);
                entity.Property(e => e.ConsentReceiveMail);
                entity.Property(e => e.UnsubscribeMail);

                // ReferralCode: unique and required
                entity.Property(e => e.ReferralCode).HasMaxLength(100).IsRequired();
                entity.HasIndex(e => e.ReferralCode).IsUnique();

                entity.OwnsOne(e => e.Address, address =>
                {
                    // Update properties to match the required attribute in AddressModel
                    address.Property(a => a.Street).HasMaxLength(100).IsRequired();
                    address.Property(a => a.City).HasMaxLength(100).IsRequired();
                    address.Property(a => a.State).HasMaxLength(100).IsRequired();
                    address.Property(a => a.PostalCode).HasMaxLength(20).IsRequired();
                    address.Property(a => a.Country).HasMaxLength(100).IsRequired(false);
                });
            });
        }
    }
}


