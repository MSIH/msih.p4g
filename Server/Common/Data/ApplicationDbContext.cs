/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.EntityFrameworkCore;
using msih.p4g.Server.Common.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace msih.p4g.Server.Common.Data
{
    /// <summary>
    /// Main application DbContext that combines all feature-specific contexts
    /// </summary>
    public partial class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        /// <summary>
        /// Override SaveChanges to handle auditable entities
        /// </summary>
        public override int SaveChanges()
        {
            ApplyAuditInfo();
            return base.SaveChanges();
        }
        
        /// <summary>
        /// Override SaveChangesAsync to handle auditable entities
        /// </summary>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditInfo();
            return base.SaveChangesAsync(cancellationToken);
        }
        
        /// <summary>
        /// Applies audit information to IAuditableEntity entities before saving
        /// </summary>
        private void ApplyAuditInfo()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is IAuditableEntity && 
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                if (entityEntry.State == EntityState.Added)
                {
                    // Set defaults for new entities
                    ((IAuditableEntity)entityEntry.Entity).CreatedOn = DateTime.UtcNow;
                    ((IAuditableEntity)entityEntry.Entity).IsActive = true;
                    ((IAuditableEntity)entityEntry.Entity).IsDeleted = false;
                }
                else
                {
                    // Don't modify CreatedOn and CreatedBy for existing entities
                    entityEntry.Property("CreatedOn").IsModified = false;
                    entityEntry.Property("CreatedBy").IsModified = false;
                    
                    // Set ModifiedOn automatically
                    ((IAuditableEntity)entityEntry.Entity).ModifiedOn = DateTime.UtcNow;
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Each feature's OnModelCreating is implemented in its own partial class
            ConfigureCampaignModel(modelBuilder);
            ConfigureDonorModel(modelBuilder);
            ConfigureDonationModel(modelBuilder);
            ConfigureUserModel(modelBuilder);
            ConfigureProfileModel(modelBuilder);
            ConfigureSmsModel(modelBuilder);
            ConfigurePaymentModel(modelBuilder);
            ConfigureSettingsModel(modelBuilder);
            ConfigureMessageModel(modelBuilder);
        }
        
        // These methods are implemented in each feature's partial class
        partial void ConfigureCampaignModel(ModelBuilder modelBuilder);
        partial void ConfigureDonorModel(ModelBuilder modelBuilder);
        partial void ConfigureDonationModel(ModelBuilder modelBuilder);
        partial void ConfigureUserModel(ModelBuilder modelBuilder);
        partial void ConfigureProfileModel(ModelBuilder modelBuilder);
        partial void ConfigureSmsModel(ModelBuilder modelBuilder);
        partial void ConfigurePaymentModel(ModelBuilder modelBuilder);
        partial void ConfigureSettingsModel(ModelBuilder modelBuilder);
        partial void ConfigureMessageModel(ModelBuilder modelBuilder);
    }
}
