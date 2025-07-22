/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.EntityFrameworkCore;
using MSIH.Core.Services.Setting.Models;
using MSIH.Core.Services.Sms.Models;
using MSIH.Core.Services.User.Models;
using MSIH.Core.Services.Profile.Model;
using MSIH.Core.Services.Message.Models;

namespace MSIH.Core.Common.Data
{
    /// <summary>
    /// ApplicationDbContext for MSIH.Core project
    /// Contains all entity configurations for Core services
    /// </summary>
    public partial class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSets for Core entities
        public DbSet<MSIH.Core.Services.Setting.Models.Setting> Settings { get; set; }
        public DbSet<ValidatedPhoneNumber> ValidatedPhoneNumbers { get; set; }
        public DbSet<MSIH.Core.Services.User.Models.User> Users { get; set; }
        public DbSet<MSIH.Core.Services.Profile.Model.Profile> Profiles { get; set; }
        public DbSet<MSIH.Core.Services.Message.Models.Message> Messages { get; set; }
        public DbSet<MessageTemplate> MessageTemplates { get; set; }
        public DbSet<MessageTemplateUsage> MessageTemplateUsages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureSettingsModel(modelBuilder);
            ConfigureValidatedPhoneNumberModel(modelBuilder);
            ConfigureUserModel(modelBuilder);
            ConfigureProfileModel(modelBuilder);
            ConfigureMessageModel(modelBuilder);
            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Partial method for configuring User entity - implemented in UserDbContext.cs
        /// </summary>
        partial void ConfigureUserModel(ModelBuilder modelBuilder);

        /// <summary>
        /// Partial method for configuring Profile entity - implemented in ProfileDbContext.cs
        /// </summary>
        partial void ConfigureProfileModel(ModelBuilder modelBuilder);

        /// <summary>
        /// Partial method for configuring Message entities - implemented in MessageDbContext.cs
        /// </summary>
        partial void ConfigureMessageModel(ModelBuilder modelBuilder);

        /// <summary>
        /// Configure the Setting entity
        /// </summary>
        private void ConfigureSettingsModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MSIH.Core.Services.Setting.Models.Setting>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Value).HasMaxLength(1000);
                entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
                entity.Property(e => e.ModifiedBy).HasMaxLength(100);
            });
        }

        /// <summary>
        /// Configure the ValidatedPhoneNumber entity
        /// </summary>
        private void ConfigureValidatedPhoneNumberModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ValidatedPhoneNumber>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Carrier).HasMaxLength(100);
                entity.Property(e => e.CountryCode).HasMaxLength(5);
                entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
                entity.Property(e => e.ModifiedBy).HasMaxLength(100);
                
                // Add unique index on PhoneNumber
                entity.HasIndex(e => e.PhoneNumber).IsUnique();
            });
        }
    }
}