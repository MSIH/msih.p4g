/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using msih.p4g.Server.Common.Models;
using msih.p4g.Server.Features.Base.SmsService.Model;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace msih.p4g.Server.Features.Base.SmsService.Data
{
    /// <summary>
    /// Database context for SMS service related entities
    /// </summary>
    public class SmsDbContext : DbContext
    {
        public SmsDbContext(DbContextOptions<SmsDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// DbSet for validated phone numbers
        /// </summary>
        public DbSet<ValidatedPhoneNumber> ValidatedPhoneNumbers { get; set; }
        
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
            
            // Configure the ValidatedPhoneNumber entity
            modelBuilder.Entity<ValidatedPhoneNumber>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.PhoneNumber).IsUnique();
                entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Carrier).HasMaxLength(100);
                entity.Property(e => e.CountryCode).HasMaxLength(5);
                
                // Common audit properties from BaseEntity
                entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
                entity.Property(e => e.ModifiedBy).HasMaxLength(100);
            });
        }
    }
}
