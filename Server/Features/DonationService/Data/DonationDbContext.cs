/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.EntityFrameworkCore;
using msih.p4g.Server.Features.DonationService.Models;

namespace msih.p4g.Server.Features.DonationService.Data
{
    /// <summary>
    /// DbContext for Donation entity
    /// </summary>
    public class DonationDbContext : DbContext
    {
        public DonationDbContext(DbContextOptions<DonationDbContext> options) : base(options) { }

        public DbSet<Donation> Donations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Donation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).IsRequired().HasPrecision(18, 2);
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
            });
        }
    }
}
