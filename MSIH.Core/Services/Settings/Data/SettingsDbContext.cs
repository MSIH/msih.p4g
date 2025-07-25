/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.EntityFrameworkCore;
using MSIH.Core.Services.Settings.Models;

namespace MSIH.Core.Services.Settings.Data
{
    /// <summary>
    /// DbContext for Setting entity (temporary standalone context)
    /// TODO: This will be integrated into the main ApplicationDbContext when Common is moved
    /// </summary>
    public class SettingsDbContext : DbContext
    {
        /// <summary>
        /// Gets or sets the Settings DbSet
        /// </summary>
        public DbSet<Models.Setting> Settings { get; set; }

        public SettingsDbContext(DbContextOptions<SettingsDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureSettingsModel(modelBuilder);
            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Configure the Setting entity
        /// </summary>
        private void ConfigureSettingsModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.Setting>(entity =>
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
