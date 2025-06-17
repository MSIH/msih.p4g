/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.EntityFrameworkCore;
using msih.p4g.Server.Features.DonorService.Model;

namespace msih.p4g.Server.Common.Data
{
    /// <summary>
    /// Partial DbContext implementation for Donor entity
    /// </summary>
    public partial class ApplicationDbContext
    {
        /// <summary>
        /// Gets or sets the Donors DbSet
        /// </summary>
        public DbSet<Donor> Donors { get; set; }
        
        /// <summary>
        /// Configure the Donor entity
        /// </summary>
        partial void ConfigureDonorModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Donor>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.DonorId).IsUnique();
                entity.Property(e => e.DonorId).IsRequired(false).ValueGeneratedOnAdd();
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.PaymentProcessorDonorId).IsRequired(false);
            });
        }
    }
}

namespace msih.p4g.Server.Features.DonorService.Data
{
    /// <summary>
    /// DbContext for Donor entity only - retained for backward compatibility
    /// Will be removed in future versions
    /// </summary>
    [Obsolete("Use ApplicationDbContext instead. This class will be removed in a future version.")]
    public class DonorDbContext : DbContext
    {
        public DonorDbContext(DbContextOptions<DonorDbContext> options) : base(options) { }
        
        public DbSet<Donor> Donors { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Donor>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.DonorId).IsUnique();
                entity.Property(e => e.DonorId).IsRequired(false).ValueGeneratedOnAdd();
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.PaymentProcessorDonorId).IsRequired(false);
            });
        }
    }
}
