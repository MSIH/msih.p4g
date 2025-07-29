# AffiliateMonitoringService

## Overview
The AffiliateMonitoringService is a base infrastructure service that monitors affiliate accounts and automatically handles suspension logic based on donor account creation patterns. This service helps prevent abuse of the affiliate system by identifying and suspending affiliates who create multiple unqualified donor accounts.

## Architecture

### Components
- **Service**: `AffiliateMonitoringService` - Core business logic for monitoring and suspension
- **Interface**: `IAffiliateMonitoringService` - Service contract
- **Extensions**: `AffiliateMonitoringServiceExtensions` - DI registration

### Dependencies
- `ApplicationDbContext` - Database access
- `IProfileService` - User profile operations
- `IFundraiserService` - Fundraiser management
- `IUserService` - User operations
- `IMessageService` - Email notifications
- `ISettingsService` - Configuration settings

## Key Features

- **Automatic Monitoring**: Checks affiliate accounts after donor creation
- **Suspension Logic**: Suspends affiliates based on configurable criteria
- **Email Notifications**: Sends suspension notifications to affected affiliates
- **Unqualified Account Tracking**: Counts donors who haven't made donations
- **Pattern Detection**: Identifies suspicious account creation patterns

## Database Schema

This service works with existing entities and doesn't define its own models:
- Uses `Fundraiser` entity for affiliate data
- Uses `Profile` entity for contact information
- Uses `Donor` entity for tracking account creation and donations

## Usage

### Service Registration
```csharp
builder.Services.AddAffiliateMonitoringService();
```

### Checking Affiliate After Donor Creation
```csharp
public async Task<bool> ProcessNewDonor(string referralCode)
{
    var wasSuspended = await _affiliateMonitoringService
        .CheckAffiliateAfterDonorCreationAsync(referralCode);
    
    if (wasSuspended)
    {
        _logger.LogInformation("Affiliate {ReferralCode} was suspended", referralCode);
    }
    
    return wasSuspended;
}
```

### Manual Suspension
```csharp
public async Task<bool> SuspendAffiliate(Fundraiser fundraiser, string reason)
{
    var success = await _affiliateMonitoringService
        .SuspendAffiliateAsync(fundraiser, reason);
    
    if (success)
    {
        await _affiliateMonitoringService
            .SendSuspensionNotificationAsync(fundraiser.Profile, fundraiser, reason);
    }
    
    return success;
}
```

## Integration

### With DonorService
- Called automatically when new donors are created
- Passes referral code for affiliate monitoring

### With FundraiserService
- Updates fundraiser suspension status
- Manages suspension metadata (date, reason)

### With MessageService
- Sends HTML email notifications
- Includes suspension details and contact information

### With UserService
- Retrieves user data with profile and fundraiser information
- Validates referral codes

## Suspension Criteria

### Primary Rules
1. **First Two Accounts**: Suspend if first 2 donor accounts are unqualified
2. **Ten+ Unqualified**: Suspend if more than 9 unqualified accounts exist

### Unqualified Account Definition
- Donor account with no donations made
- Account is active (not deleted/disabled)

## Files

```
Server/Features/Base/AffiliateMonitoringService/
├── Extensions/
│   └── AffiliateMonitoringServiceExtensions.cs
├── Interfaces/
│   └── IAffiliateMonitoringService.cs
├── Services/
│   └── AffiliateMonitoringService.cs
└── CLAUDE.md
```