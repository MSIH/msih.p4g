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
using msih.p4g.Server.Features.DonorService.Model;

namespace msih.p4g.Server.Features.DonorService.Data
{
    /// <summary>
    /// DbContext for Donor entity only
    /// </summary>
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
