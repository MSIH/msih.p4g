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
                entity.Property(e => e.DonationAmount).IsRequired().HasPrecision(18, 2);
                entity.Property(e => e.DonationMessage).HasMaxLength(1000);
                entity.Property(e => e.ReferralCode).HasMaxLength(100);
                entity.Property(e => e.CampaignCode).HasMaxLength(100);
               
                // Configure relationships with two-way navigations
                entity.HasOne(d => d.Donor)
                      .WithMany(donor => donor.Donations)  // Two-way navigation
                      .HasForeignKey(d => d.DonorId)
                      .OnDelete(DeleteBehavior.Restrict);  // Don't cascade delete donors
                
                entity.HasOne(d => d.PaymentTransaction)
                      .WithMany()  // A payment transaction may be associated with multiple donations
                      .HasForeignKey(d => d.PaymentTransactionId)
                      .OnDelete(DeleteBehavior.Restrict);  // Don't cascade delete payment transactions
                
                entity.HasOne(d => d.Campaign)
                      .WithMany(campaign => campaign.Donations)  // Two-way navigation
                      .HasForeignKey(d => d.CampaignId)
                      .OnDelete(DeleteBehavior.Restrict);  // Don't cascade delete campaigns

                // Configure self-referencing relationship for recurring donations
                entity.HasOne(d => d.ParentRecurringDonation)
                      .WithMany(parent => parent.RecurringPayments)
                      .HasForeignKey(d => d.ParentRecurringDonationId)
                      .OnDelete(DeleteBehavior.Restrict);  // Don't cascade delete parent

            });
        }
    }
}


