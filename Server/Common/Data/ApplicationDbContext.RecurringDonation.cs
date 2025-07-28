/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

using Microsoft.EntityFrameworkCore;
using msih.p4g.Server.Features.RecurringDonationService.Models;

namespace msih.p4g.Server.Common.Data
{
    /// <summary>
    /// Partial DbContext implementation for RecurringDonation entity
    /// </summary>
    public partial class ApplicationDbContext
    {
        /// <summary>
        /// Gets or sets the RecurringDonations DbSet
        /// </summary>
        public DbSet<RecurringDonation> RecurringDonations { get; set; }

        /// <summary>
        /// Configure the RecurringDonation entity
        /// </summary>
        partial void ConfigureRecurringDonationModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RecurringDonation>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Amount)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.PayTransactionFeeAmount)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Currency)
                    .HasMaxLength(3)
                    .IsRequired();

                entity.Property(e => e.PaymentMethodToken)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(e => e.DonationMessage)
                    .HasMaxLength(1000);

                entity.Property(e => e.ReferralCode)
                    .HasMaxLength(100);

                entity.Property(e => e.CampaignCode)
                    .HasMaxLength(100);

                entity.Property(e => e.LastErrorMessage)
                    .HasMaxLength(2000);

                entity.Property(e => e.CancelledBy)
                    .HasMaxLength(255);

                entity.Property(e => e.CancellationReason)
                    .HasMaxLength(1000);

                // Configure enum properties
                entity.Property(e => e.Frequency)
                    .HasConversion<int>();

                entity.Property(e => e.Status)
                    .HasConversion<int>();

                // Configure foreign key relationships
                entity.HasOne(e => e.Donor)
                    .WithMany()
                    .HasForeignKey(e => e.DonorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Campaign)
                    .WithMany()
                    .HasForeignKey(e => e.CampaignId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Configure indexes for better query performance
                entity.HasIndex(e => e.DonorId)
                    .HasDatabaseName("IX_RecurringDonations_DonorId");

                entity.HasIndex(e => e.Status)
                    .HasDatabaseName("IX_RecurringDonations_Status");

                entity.HasIndex(e => e.NextProcessDate)
                    .HasDatabaseName("IX_RecurringDonations_NextProcessDate");

                entity.HasIndex(e => new { e.Status, e.NextProcessDate })
                    .HasDatabaseName("IX_RecurringDonations_Status_NextProcessDate");

                entity.HasIndex(e => e.ReferralCode)
                    .HasDatabaseName("IX_RecurringDonations_ReferralCode");

                entity.HasIndex(e => e.CampaignId)
                    .HasDatabaseName("IX_RecurringDonations_CampaignId");
            });
        }
    }
}