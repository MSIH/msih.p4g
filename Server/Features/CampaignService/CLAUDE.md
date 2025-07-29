# CampaignService

## Overview
The CampaignService manages fundraising campaigns within the MSIH Platform for Good. It provides comprehensive campaign lifecycle management including creation, updates, status management, and default campaign designation. This service serves as the foundation for organizing donations and fundraising efforts, enabling organizations to run multiple concurrent campaigns while maintaining a primary default campaign for general donations.

## Architecture

### Components
- **ICampaignService**: Service interface for campaign operations
- **CampaignService**: Core service implementation handling business logic
- **ICampaignRepository**: Repository interface for campaign data access
- **CampaignRepository**: Repository implementation with caching and specialized queries
- **Campaign**: Main entity representing fundraising campaigns

### Dependencies
- Entity Framework Core for data persistence
- ApplicationDbContext for database operations
- GenericRepository for base CRUD operations
- ICacheStrategy for performance optimization
- DonationService integration for campaign-donation relationships

## Key Features
- Complete campaign CRUD operations with audit trails
- Default campaign management with single-default enforcement
- Active/inactive status management for campaign lifecycle
- Performance-optimized caching for default campaign lookups
- Integration with donation tracking and reporting
- Campaign validation and constraint enforcement

## Database Schema

### Campaign Entity
- **Id** (int): Primary key, auto-generated
- **Title** (string): Campaign title (required, max 100 chars)
- **Description** (string): Campaign description (max 1000 chars)
- **IsDefault** (bool): Indicates if this is the default campaign
- **Donations** (navigation): Collection of donations associated with campaign
- **IsActive** (bool): Soft delete flag from BaseEntity
- **CreatedOn** (DateTime): Creation timestamp from BaseEntity
- **CreatedBy** (string): Creator identifier from BaseEntity
- **ModifiedOn** (DateTime?): Last modification timestamp from BaseEntity
- **ModifiedBy** (string?): Last modifier identifier from BaseEntity

### Business Rules
- Only one campaign can be marked as default at any time
- Default campaign must be active
- Campaign titles are required and limited to 100 characters
- Descriptions are optional with 1000 character limit

## Usage

### Basic Campaign Operations
```csharp
@inject ICampaignService CampaignService

// Create a new campaign
var campaign = new Campaign
{
    Title = "Emergency Relief Fund",
    Description = "Supporting families affected by natural disasters with immediate relief including food, shelter, and medical assistance.",
    IsDefault = false
};

var createdCampaign = await CampaignService.AddAsync(campaign);

// Get campaign by ID
var campaign = await CampaignService.GetByIdAsync(123);

// Update campaign
campaign.Description = "Updated description with more details about our relief efforts.";
var updated = await CampaignService.UpdateAsync(campaign);

// Get all active campaigns
var activeCampaigns = await CampaignService.GetAllAsync(includeInactive: false);
```

### Default Campaign Management
```csharp
// Get the current default campaign
var defaultCampaign = await CampaignService.GetDefaultCampaignAsync();

if (defaultCampaign != null)
{
    Console.WriteLine($"Default campaign: {defaultCampaign.Title}");
}

// Set a new default campaign
var success = await CampaignService.SetDefaultCampaignAsync(456);

if (success)
{
    Console.WriteLine("Default campaign updated successfully");
}
else
{
    Console.WriteLine("Failed to set default campaign - campaign not found or inactive");
}
```

### Campaign Status Management
```csharp
// Deactivate a campaign (soft delete)
var deactivated = await CampaignService.SetActiveAsync(123, false, "AdminUser");

// Reactivate a campaign
var reactivated = await CampaignService.SetActiveAsync(123, true, "AdminUser");

// Get all campaigns including inactive ones
var allCampaigns = await CampaignService.GetAllAsync(includeInactive: true);

foreach (var campaign in allCampaigns)
{
    Console.WriteLine($"{campaign.Title} - Active: {campaign.IsActive}, Default: {campaign.IsDefault}");
}
```

## Integration

### DonationService Integration
- **Campaign-Donation Relationship**: Donations are linked to campaigns via foreign key
- **Donation Tracking**: Campaign entity includes navigation property to associated donations
- **Revenue Reporting**: Enables campaign-specific donation reporting and analytics

### Default Campaign Logic
The service enforces business rules around default campaign management:
```csharp
public async Task<bool> SetDefaultCampaignAsync(int campaignId)
{
    // Verify campaign exists and is active
    var campaign = await _repository.GetByIdAsync(campaignId);
    if (campaign == null || !campaign.IsActive)
    {
        return false; // Cannot set inactive campaign as default
    }

    // Repository handles setting only this campaign as default
    await _repository.SetDefaultCampaignAsync(campaignId);
    return true;
}
```

### Caching Strategy
The repository implements intelligent caching for frequently accessed data:
```csharp
public async Task<Campaign?> GetDefaultCampaignAsync()
{
    var cacheKey = $"{typeof(Campaign).Name}_Default";
    
    // Try cache first
    var cachedResult = await _cacheStrategy.GetAsync<Campaign>(cacheKey);
    if (cachedResult != null)
    {
        return cachedResult;
    }
    
    // Query database and cache result
    var defaultCampaign = await context.Campaigns
        .FirstOrDefaultAsync(c => c.IsDefault && c.IsActive);
        
    await _cacheStrategy.SetAsync(cacheKey, defaultCampaign);
    return defaultCampaign;
}
```

## Advanced Features

### Campaign Data Seeding
The service includes data seeding capabilities for initial campaign setup:
```csharp
// CampaignDataSeeder can be used to populate initial campaigns
// Useful for development environments and fresh installations
public class CampaignDataSeeder
{
    public async Task SeedCampaignsAsync(ApplicationDbContext context)
    {
        if (!context.Campaigns.Any())
        {
            var defaultCampaign = new Campaign
            {
                Title = "General Fund",
                Description = "Support our general operations and ongoing initiatives.",
                IsDefault = true,
                IsActive = true
            };
            
            context.Campaigns.Add(defaultCampaign);
            await context.SaveChangesAsync();
        }
    }
}
```

### Cache Invalidation Strategy
When campaigns are updated, the repository implements comprehensive cache invalidation:
```csharp
public async Task SetDefaultCampaignAsync(int campaignId)
{
    // Update all campaigns - only one can be default
    var campaigns = await context.Campaigns.ToListAsync();
    foreach (var campaign in campaigns)
    {
        campaign.IsDefault = campaign.Id == campaignId;
    }
    await context.SaveChangesAsync();

    // Invalidate affected cache entries
    await _cacheStrategy.RemoveAsync($"{typeof(Campaign).Name}_Default");
    await InvalidateCollectionCacheAsync(); // All campaigns cache
    
    // Invalidate individual campaign caches
    foreach (var campaign in campaigns)
    {
        await InvalidateEntityCacheAsync(campaign.Id);
    }
}
```

## Business Logic and Validation

### Single Default Campaign Enforcement
The system ensures only one campaign can be marked as default:
- When setting a new default campaign, all other campaigns have IsDefault set to false
- Repository-level transaction ensures data consistency
- Cache invalidation maintains performance while ensuring accuracy

### Campaign Lifecycle Management
- **Creation**: New campaigns are created as non-default and active
- **Activation**: Campaigns can be activated/deactivated for lifecycle management
- **Default Assignment**: Only active campaigns can be set as default
- **Deletion**: Soft delete pattern preserves historical data and relationships

## Performance Optimizations

### Caching Implementation
- **Default Campaign**: Heavily cached as it's frequently accessed for donation processing
- **Collection Queries**: GetAll operations cached with active/inactive variants
- **Entity Caching**: Individual campaigns cached by ID for fast lookup
- **Smart Invalidation**: Targeted cache invalidation minimizes performance impact

### Database Optimization
- Indexes on IsDefault and IsActive for fast default campaign lookup
- Foreign key relationships properly indexed for donation queries
- Audit fields indexed for historical reporting and tracking

## Usage Scenarios

### Multi-Campaign Organization
```csharp
// Setup for organization running multiple campaigns
var campaigns = new[]
{
    new Campaign { Title = "Emergency Relief", Description = "Disaster response fund" },
    new Campaign { Title = "Education Initiative", Description = "Supporting local schools" },
    new Campaign { Title = "General Operations", Description = "Day-to-day operations", IsDefault = true }
};

foreach (var campaign in campaigns)
{
    await CampaignService.AddAsync(campaign);
}

// Donors without specific campaign selection go to default
var defaultCampaign = await CampaignService.GetDefaultCampaignAsync();
```

### Campaign Migration
```csharp
// Migrating default campaign to new initiative
var oldDefault = await CampaignService.GetDefaultCampaignAsync();
var newCampaignId = 456;

// Set new default (automatically unsets old default)
await CampaignService.SetDefaultCampaignAsync(newCampaignId);

// Optionally deactivate old campaign
await CampaignService.SetActiveAsync(oldDefault.Id, false, "Migration");
```

## Reporting and Analytics Integration

### Campaign Performance Tracking
```csharp
// Get campaign with donation data for reporting
var campaign = await CampaignService.GetByIdAsync(123);
var totalDonations = campaign.Donations.Sum(d => d.Amount);
var donorCount = campaign.Donations.Select(d => d.DonorId).Distinct().Count();

Console.WriteLine($"Campaign: {campaign.Title}");
Console.WriteLine($"Total Raised: ${totalDonations:N2}");
Console.WriteLine($"Unique Donors: {donorCount}");
```

## Files

```
Server/Features/CampaignService/
├── Data/
│   ├── CampaignDataSeeder.cs
│   └── CampaignDbContext.cs
├── Interfaces/
│   ├── ICampaignRepository.cs
│   └── ICampaignService.cs
├── Model/
│   └── Campaign.cs
├── Repositories/
│   └── CampaignRepository.cs
├── Services/
│   └── CampaignService.cs
└── CLAUDE.md
```