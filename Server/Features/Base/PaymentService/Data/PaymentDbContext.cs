/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using msih.p4g.Server.Common.Models;
using msih.p4g.Server.Features.Base.PaymentService.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace msih.p4g.Server.Features.Base.PaymentService.Data
{
    /// <summary>
    /// Database context for payment service related entities
    /// </summary>
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// DbSet for payment transactions
        /// </summary>
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        
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
            
            // Configure the PaymentTransaction entity
            modelBuilder.Entity<PaymentTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TransactionId);
                entity.Property(e => e.TransactionId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Provider).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Amount).IsRequired().HasPrecision(18, 2);
                entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.CustomerEmail).HasMaxLength(255);
                entity.Property(e => e.AdditionalData).HasMaxLength(4000);
                entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
                entity.Property(e => e.OrderReference).HasMaxLength(100);
                
                // Common audit properties from BaseEntity
                entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
                entity.Property(e => e.ModifiedBy).HasMaxLength(100);
            });
        }
    }
}