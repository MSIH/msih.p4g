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
    }
}
