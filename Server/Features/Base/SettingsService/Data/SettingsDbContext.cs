/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.EntityFrameworkCore;
using msih.p4g.Server.Features.Base.SettingsService.Model;

namespace msih.p4g.Server.Common.Data
{
    /// <summary>
    /// Partial DbContext implementation for Setting entity
    /// </summary>
    public partial class ApplicationDbContext
    {
        /// <summary>
        /// Gets or sets the Settings DbSet
        /// </summary>
        public DbSet<Setting> Settings { get; set; }
        
        /// <summary>
        /// Configure the Setting entity
        /// </summary>
        partial void ConfigureSettingsModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Setting>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Value).HasMaxLength(1000);
                entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
                entity.Property(e => e.ModifiedBy).HasMaxLength(100);
            });
        }
    }
}

namespace msih.p4g.Server.Features.Base.SettingsService.Data
{
    /// <summary>
    /// DbContext for Setting entity only - retained for backward compatibility
    /// Will be removed in future versions
    /// </summary>
    [Obsolete("Use ApplicationDbContext instead. This class will be removed in a future version.")]
    public class SettingsDbContext : DbContext
    {
        public SettingsDbContext(DbContextOptions<SettingsDbContext> options) : base(options) { }
        
        public DbSet<Setting> Settings { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Setting>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Value).HasMaxLength(1000);
                entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
                entity.Property(e => e.ModifiedBy).HasMaxLength(100);
            });
        }
    }
}
