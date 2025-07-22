/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.EntityFrameworkCore;
using MSIH.Core.Services.Message.Models;

namespace MSIH.Core.Common.Data
{
    /// <summary>
    /// Partial DbContext implementation for message service related entities
    /// </summary>
    public partial class ApplicationDbContext
    {

        /// <summary>
        /// Configure the message entities
        /// </summary>
        partial void ConfigureMessageModel(ModelBuilder modelBuilder)
        {
            // Configure the Message entity
            modelBuilder.Entity<MSIH.Core.Services.Message.Models.Message>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MessageType).IsRequired().HasMaxLength(10);
                entity.Property(e => e.From).IsRequired().HasMaxLength(100);
                entity.Property(e => e.To).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Subject).HasMaxLength(200);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.ErrorMessage).HasMaxLength(500);

                // Common audit properties from BaseEntity
                entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
                entity.Property(e => e.ModifiedBy).HasMaxLength(100);
            });

            // Configure the MessageTemplate entity
            modelBuilder.Entity<MSIH.Core.Services.Message.Models.MessageTemplate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.MessageType).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
                entity.Property(e => e.DefaultSubject).HasMaxLength(200);
                entity.Property(e => e.TemplateContent).IsRequired();
                entity.Property(e => e.AvailablePlaceholders).HasMaxLength(1000);
                entity.Property(e => e.DefaultSender).HasMaxLength(100);

                // Common audit properties from BaseEntity
                entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
                entity.Property(e => e.ModifiedBy).HasMaxLength(100);
            });

            // Configure the MessageTemplateUsage entity
            modelBuilder.Entity<MSIH.Core.Services.Message.Models.MessageTemplateUsage>(entity =>
            {
                entity.HasKey(e => e.MessageId);
                
                entity.HasOne(e => e.Message)
                    .WithOne()
                    .HasForeignKey<MSIH.Core.Services.Message.Models.MessageTemplateUsage>(e => e.MessageId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Template)
                    .WithMany()
                    .HasForeignKey(e => e.TemplateId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.Property(e => e.PlaceholderValuesJson).HasColumnType("nvarchar(max)");
            });
        }
    }
}
