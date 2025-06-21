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
using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Features.OrganizationService.Models;

namespace msih.p4g.Server.Features.OrganizationService.Data
{
    /// <summary>
    /// Data seeder for Organization data
    /// </summary>
    public class OrganizationDataSeeder
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OrganizationDataSeeder> _logger;

        public OrganizationDataSeeder(
            IServiceProvider serviceProvider,
            ILogger<OrganizationDataSeeder> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Seeds initial organization data if none exists
        /// </summary>
        public async Task SeedAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Check if any organizations exist
                if (!await dbContext.Organizations.AnyAsync())
                {
                    _logger.LogInformation("Seeding organization data...");

                    // Add default organization
                    var organization = new Organization
                    {
                        LegalName = "Make Sure It Happens Inc",
                        TaxId = "85-3536160",
                        OrganizationType = "501(c)(3)",
                        EmailAddress = "ithappens@msih.org",
                        Website = "https://www.msih.org",
                        ShortDescription = "Make Sure It Happens, Inc. provide charitable and educational assistance to the general public to help them make a positive impact on the world.",
                        MissionStatement = "Our mission is to empower students and communities through educational initiatives and research projects that address real-world problems.",
                        IsActive = true,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = "System"
                    };

                    dbContext.Organizations.Add(organization);
                    await dbContext.SaveChangesAsync();

                    _logger.LogInformation("Organization data seeded successfully.");
                }
                else
                {
                    _logger.LogInformation("Organization data already exists. Skipping seed.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding organization data.");
                throw;
            }
        }
    }
}
