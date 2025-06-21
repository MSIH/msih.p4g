/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.EntityFrameworkCore;
using msih.p4g.Server.Features.OrganizationService.Models;

namespace msih.p4g.Server.Common.Data
{
    /// <summary>
    /// Partial DbContext implementation for Organization entity
    /// </summary>
    public partial class ApplicationDbContext
    {
        /// <summary>
        /// Gets or sets the Organizations DbSet
        /// </summary>
        public DbSet<Organization> Organizations { get; set; }

        /// <summary>
        /// Configure the Organization entity
        /// </summary>
        partial void ConfigureOrganizationModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Organization>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LegalName).IsRequired(false).HasMaxLength(200);
                entity.Property(e => e.TaxId).IsRequired(false).HasMaxLength(50);
                entity.HasIndex(e => e.TaxId).IsUnique();
                entity.Property(e => e.Website).IsRequired(false).HasMaxLength(255);
                entity.Property(e => e.EmailAddress).IsRequired(false).HasMaxLength(100);
                entity.Property(e => e.Phone).IsRequired(false).HasMaxLength(20);
                entity.Property(e => e.MissionStatement).IsRequired(false).HasMaxLength(1000);
                entity.Property(e => e.ShortDescription).IsRequired(false).HasMaxLength(500);
                entity.Property(e => e.OrganizationType).IsRequired(false).HasMaxLength(50);
                entity.Property(e => e.LogoUrl).IsRequired(false).HasMaxLength(255);
                entity.Property(e => e.Street).IsRequired(false).HasMaxLength(200);
                entity.Property(e => e.City).IsRequired(false).HasMaxLength(100);
                entity.Property(e => e.State).IsRequired(false).HasMaxLength(100);
                entity.Property(e => e.PostalCode).IsRequired(false).HasMaxLength(20);
                entity.Property(e => e.Country).IsRequired(false).HasMaxLength(100);
                
                // Relationship with Campaigns
                entity.HasMany(e => e.Campaigns)
                      .WithOne()
                      .HasForeignKey("OrganizationId")
                      .OnDelete(DeleteBehavior.Restrict);
                
                // Relationship with Donations
                entity.HasMany(e => e.Donations)
                      .WithOne()
                      .HasForeignKey("OrganizationId")
                      .OnDelete(DeleteBehavior.Restrict);
                
                // Common audit properties from BaseEntity
                entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
                entity.Property(e => e.ModifiedBy).HasMaxLength(100);
            });
        }
    }
}
