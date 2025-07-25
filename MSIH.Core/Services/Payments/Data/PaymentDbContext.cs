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
using MSIH.Core.Services.Payments.Models;

namespace MSIH.Core.Common.Data
{
    /// <summary>
    /// Partial DbContext implementation for payment service related entities
    /// </summary>
    public partial class ApplicationDbContext
    {
        /// <summary>
        /// DbSet for payment transactions
        /// </summary>
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }

        /// <summary>
        /// Configure the Payment entities
        /// </summary>
        partial void ConfigurePaymentModel(ModelBuilder modelBuilder)
        {
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
