/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.EntityFrameworkCore;
using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Features.Base.W9FormService.Models;

namespace msih.p4g.Server.Common.Data
{
    /// <summary>
    /// Extension of ApplicationDbContext for W9Form-related configuration
    /// </summary>
    public partial class ApplicationDbContext
    {
        public DbSet<W9Form> W9Forms { get; set; }

        partial void ConfigureW9FormModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<W9Form>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.BusinessName).HasMaxLength(200);
                entity.Property(e => e.FederalTaxClassification).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LLCTaxClassification).HasMaxLength(1);
                entity.Property(e => e.OtherClassificationInstructions).HasMaxLength(500);
                entity.Property(e => e.PartnershipTrustInfo).HasMaxLength(50);
                entity.Property(e => e.ExemptPayeeCode).HasMaxLength(20);
                entity.Property(e => e.FATCAExemptionCode).HasMaxLength(20);
                entity.Property(e => e.Address).IsRequired().HasMaxLength(200);
                entity.Property(e => e.CityStateZip).IsRequired().HasMaxLength(200);
                entity.Property(e => e.AccountNumbers).HasMaxLength(200);
                
                // Sensitive data fields
                entity.Property(e => e.SocialSecurityNumber).HasMaxLength(500);
                entity.Property(e => e.EmployerIdentificationNumber).HasMaxLength(500);
                
                entity.Property(e => e.SignatureVerification).HasMaxLength(500);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                
                // Relationships
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.FundraiserId);
            });
        }
    }
}
