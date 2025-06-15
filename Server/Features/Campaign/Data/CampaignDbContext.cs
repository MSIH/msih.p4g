/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.EntityFrameworkCore;
using msih.p4g.Shared.Models;

namespace msih.p4g.Server.Features.Campaign.Data;

public class CampaignDbContext : DbContext
{
    public CampaignDbContext(DbContextOptions<CampaignDbContext> options) : base(options)
    {
    }

    public DbSet<msih.p4g.Shared.Models.Campaign> Campaigns { get; set; } = null!;
    public DbSet<msih.p4g.Shared.Models.Donor> Donors { get; set; } = null!;
    public DbSet<msih.p4g.Shared.Models.Profile> Profiles { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<msih.p4g.Shared.Models.Campaign>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.IsDeleted).IsRequired();
            entity.Property(e => e.CreatedOn).IsRequired();
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.ModifiedBy).HasMaxLength(100);
        });

        modelBuilder.Entity<msih.p4g.Shared.Models.Donor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DonorId).IsUnique();
            entity.Property(e => e.DonorId)
                .IsRequired(false)
                .ValueGeneratedOnAdd();
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.PaymentProcessorDonorId).IsRequired(false);
            entity.HasOne<msih.p4g.Server.Features.Base.UserService.Models.User>()
                .WithOne(u => u.Donor)
                .HasForeignKey<msih.p4g.Shared.Models.Donor>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<msih.p4g.Shared.Models.Profile>(entity =>
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
            entity.HasOne<msih.p4g.Server.Features.Base.UserService.Models.User>()
                .WithOne(u => u.Profile)
                .HasForeignKey<msih.p4g.Shared.Models.Profile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
