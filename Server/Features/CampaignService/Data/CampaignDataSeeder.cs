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
using msih.p4g.Server.Features.CampaignService.Model;


namespace msih.p4g.Server.Features.CampaignService.Data
{
    /// <summary>
    /// Data seeder for Campaign data
    /// </summary>
    public class CampaignDataSeeder
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CampaignDataSeeder> _logger;

        public CampaignDataSeeder(
            IServiceProvider serviceProvider,
            ILogger<CampaignDataSeeder> logger)
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

                // Define campaigns to seed based on provided text
                var campaignsToSeed = new List<Campaign>
                {
                     new Campaign
                    {
            Title = "General Fund",
            Description = "Provide charitable and educational assistance to the general public to help them make a positive impact on the world.",
            IsDefault = true,
            IsActive = true,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "System"
        },
                     new Campaign
        {
            Title = "Water Testing",
            Description = "Our Water Testing program provides student scientists across America with free water testing kits and laboratory analysis (valued at $50 each) to help them investigate local water quality issues and develop solutions for their communities. Our goal is to distribute 5,000 testing kits nationwide—100 kits to student scientists in each of the 50 states—empowering the next generation to make a positive impact through hands-on science and community service.",
            IsActive = true,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "System"
        },
        new Campaign
        {
            Title = "Student Research Projects",
            Description = "Provide charitable and educational assistance to high school and college students for academic research projects.",
            IsActive = true,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "System"
        },
        new Campaign
        {
            Title = "Music in Public",
            Description = "Music in Public is a charitable and educational organization dedicated to supporting acoustic musicians who perform in public spaces, with a mission to facilitate and enhance the experience of live music in communities by providing assistance to student and local musicians who share their talents in parks, plazas, streets, and other public venues. Through our program, we aim to create 5,000 acoustic music performances across multiple states, fostering cultural enrichment and bringing the joy of live music directly to the public while providing meaningful support to emerging and established musicians. Donations to Music in Public directly support our musicians through performance compensation and fund the development of our digital platform, which connects performers with audiences and coordinates performance opportunities, helping us cultivate vibrant musical communities while ensuring that the universal language of music remains accessible to all.",
            IsActive = true,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "System"
        },
        new Campaign
        {
            Title = "Nurturing Capitalism",
            Description = "Provide charitable and educational assistance to empower low-income people to start a business.",
            IsActive = true,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "System"
        },
        new Campaign
        {
            Title = "Food Worker",
            Description = "Provide information to food workers about salary, benefits, and tip policy for business in every state.",
            IsActive = true,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "System"
        },
        new Campaign
        {
            Title = "Election Survey",
            Description = "Provide citizens of the United States of America a way to share their views and see how others' opinions on current topics and policies.",
            IsActive = true,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "System"
        }
    };
                // Get existing campaign titles
                var existingTitles = await dbContext.Campaigns
                    .Select(c => c.Title)
                    .ToListAsync();

                // Filter out campaigns that already exist by title
                var newCampaigns = campaignsToSeed
                    .Where(campaign => !existingTitles.Contains(campaign.Title))
                    .ToList();

                if (newCampaigns.Any())
                {
                    _logger.LogInformation($"Seeding {newCampaigns.Count} new campaign(s)...");

                    dbContext.Campaigns.AddRange(newCampaigns);
                    await dbContext.SaveChangesAsync();

                    _logger.LogInformation($"Successfully seeded {newCampaigns.Count} campaign(s): {string.Join(", ", newCampaigns.Select(c => c.Title))}");
                }
                else
                {
                    _logger.LogInformation("All campaigns already exist. Skipping seed.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding campaign data.");
                throw;
            }
        }
    }
}
