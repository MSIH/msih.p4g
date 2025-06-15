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
using msih.p4g.Server.Features.Base.ProfileService.Model;

namespace msih.p4g.Server.Features.Base.ProfileService.Data
{
    /// <summary>
    /// DbContext for Profile entity only
    /// </summary>
    public class ProfileDbContext : DbContext
    {
        public ProfileDbContext(DbContextOptions<ProfileDbContext> options) : base(options) { }
        public DbSet<Profile> Profiles { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Profile>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Age).IsRequired(false);
                entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired(false);
                entity.Property(e => e.LastName).HasMaxLength(100).IsRequired(false);
                entity.Ignore(e => e.FullName);
                entity.Property(e => e.MobileNumber).IsRequired(false);
                entity.Property(e => e.ConsentReceiveText);
                entity.Property(e => e.UnsubscribeMobile);
                entity.Property(e => e.ConsentReceiveEmail);
                entity.Property(e => e.UnsubscribeEmail);
                entity.Property(e => e.ConsentReceiveMail);
                entity.Property(e => e.UnsubscribeMail);
                entity.OwnsOne(e => e.Address, address =>
                {
                    address.Property(a => a.Street).HasMaxLength(100).IsRequired(false);
                    address.Property(a => a.City).HasMaxLength(100).IsRequired(false);
                    address.Property(a => a.State).HasMaxLength(100).IsRequired(false);
                    address.Property(a => a.PostalCode).HasMaxLength(20).IsRequired(false);
                    address.Property(a => a.Country).HasMaxLength(100).IsRequired(false);
                });
            });
        }
    }
}
