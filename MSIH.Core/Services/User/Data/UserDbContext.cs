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
using MSIH.Core.Services.User.Models;

namespace MSIH.Core.Common.Data
{
    /// <summary>
    /// Partial DbContext implementation for User entity
    /// </summary>
    public partial class ApplicationDbContext
    {

        /// <summary>
        /// Configure the User entity
        /// </summary>
        partial void ConfigureUserModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MSIH.Core.Services.User.Models.User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.Role).IsRequired();
                entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
                entity.Property(e => e.ModifiedBy).HasMaxLength(100);
            });
        }
    }
}


