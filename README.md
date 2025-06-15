# MSIH Project for Good (P4G)

## Overview
This is a .NET 9 Blazor Server application following clean architecture principles.

## Licensing & Copyright

All source code in this repository is protected by copyright:
Copyright (c) 2025 MSIH LLC. All rights reserved.
This file is developed for Make Sure It Happens Inc.
Unauthorized copying, modification, distribution, or use is prohibited.
### License Headers

All source code files in this repository must include the appropriate license header. The repository includes two mechanisms to ensure this:

1. **For new files**: The `.editorconfig` file configures Visual Studio and VS Code to automatically add license headers to new files.

2. **For existing files**: Use the PowerShell script to add license headers:
# Show help
.\Add-LicenseHeaders.ps1 -Help

# Preview changes without modifying files
.\Add-LicenseHeaders.ps1 -WhatIf

# Add license headers to all files
.\Add-LicenseHeaders.ps1
## Project Structure

The application follows a feature-based organization:

- **Client**: Web client code
  - **Client.Common**: Common code, models, services, utilities
  - **Client.Feature.Base**: Base features common to all applications
  - **Client.Feature.<FeatureName>**: Application-specific features
    - Interfaces
    - Services
    - Models
    - Components
    - Layouts
    - Utilities

- **Server**: Server-side code
  - **Server.Common**: Common code, models, services, utilities
  - **Server.Feature.Base**: Base features common to all applications
  - **Server.Feature.<FeatureName>**: Application-specific features
    - Interfaces
    - Services
    - Models
    - Utilities

- **Shared**: Code shared between client and server
  - Models/DTOs
  - Utilities

## Development

### Prerequisites
- .NET 9 SDK
- Visual Studio 2022 or VS Code
- MySQL (for production) or SQLite (for local development)

### Local Development
1. Clone the repository
2. Configure your connection strings in `appsettings.Development.json`
3. Run the application with `dotnet run`

## Configuration
Configuration is handled through a combination of:
- Database settings (primary)
- `appsettings.json` (fallback)

The application uses the Options pattern for strongly-typed configuration.

## Database Migrations

The following migrations have been created for each DbContext:

- **UserDbContext**: `InitialUserDbContextMigration`  
  Path: `Server/Features/Base/UserService/Data/Migrations`
- **SmsDbContext**: `InitialSmsDbContextMigration`  
  Path: `Server/Features/Base/SmsService/Data/Migrations`
- **CampaignDbContext**: `InitialCampaignDbContextMigration`  
  Path: `Server/Features/CampaignService/Data/Migrations`
- **PaymentDbContext**: `InitialPaymentDbContextMigration`  
  Path: `Server/Features/Base/PaymentService/Data/Migrations`
- **SettingsDbContext**: `InitialSettingsDbContextMigration`  
  Path: `Server/Features/Base/SettingsService/Data/Migrations`
- **ProfileDbContext**: `InitialProfileDbContextMigration`  
  Path: `Server/Features/Base/ProfileService/Data/Migrations`
- **DonorDbContext**: `InitialDonorDbContextMigration`  
  Path: `Server/Features/DonorService/Data/Migrations`

To add a new migration for a specific context, use:
# Example for UserDbContext
dotnet ef migrations add Initial --context UserDbContext --output-dir Server/Features/Base/UserService/Data/Migrations

# Example for SmsDbContext
dotnet ef migrations add Initial --context SmsDbContext --output-dir Server/Features/Base/SmsService/Data/Migrations

# Example for CampaignDbContext
dotnet ef migrations add Initial --context CampaignDbContext --output-dir Server/Features/CampaignService/Data/Migrations

# Example for PaymentDbContext
dotnet ef migrations add Initial --context PaymentDbContext --output-dir Server/Features/Base/PaymentService/Data/Migrations

# Example for SettingsDbContext
dotnet ef migrations add Initial --context SettingsDbContext --output-dir Server/Features/Base/SettingsService/Data/Migrations

# Example for ProfileDbContext
dotnet ef migrations add Initial --context ProfileDbContext --output-dir Server/Features/Base/ProfileService/Data/Migrations

# Example for DonorDbContext
dotnet ef migrations add Initial --context DonorDbContext --output-dir Server/Features/DonorService/Data/Migrations
> **Note:**
> - Each DbContext's migrations should be output to its respective service/feature folder, not to a common/shared location. This keeps each context's migrations organized and maintainable.
> - Make sure to update the migration names and output directories as needed for your solution.
> - After recreating migrations, update your database with `dotnet ef database update --context <DbContextName>` for each context.

## Resetting All EF Core Migrations (PowerShell)

To completely reset all migrations for all models/DbContexts, run the following PowerShell commands from the root of your repository:
# Remove all migration files for all DbContexts
Remove-Item -Recurse -Force .\Migrations\*
Remove-Item -Recurse -Force .\Migrations\PaymentDb\*
Remove-Item -Recurse -Force .\Migrations\SmsDb\*
Remove-Item -Recurse -Force .\Server\Features\Base\UserService\Data\Migrations\*
Remove-Item -Recurse -Force .\Server\Features\Base\SmsService\Data\Migrations\*
Remove-Item -Recurse -Force .\Server\Features\CampaignService\Data\Migrations\*
Remove-Item -Recurse -Force .\Server\Features\Base\PaymentService\Data\Migrations\*
Remove-Item -Recurse -Force .\Server\Features\Base\SettingsService\Data\Migrations\*
Remove-Item -Recurse -Force .\Server\Features\Base\ProfileService\Data\Migrations\*
Remove-Item -Recurse -Force .\Server\Features\DonorService\Data\Migrations\*

# Recreate initial migrations for each DbContext (example for UserDbContext)
dotnet ef migrations add InitialUserDbContextMigration --context UserDbContext --output-dir Server/Features/Base/UserService/Data/Migrations
dotnet ef migrations add InitialSmsDbContextMigration --context SmsDbContext --output-dir Server/Features/Base/SmsService/Data/Migrations
dotnet ef migrations add InitialCampaignDbContextMigration --context CampaignDbContext --output-dir Server/Features/CampaignService/Data/Migrations
dotnet ef migrations add InitialPaymentDbContextMigration --context PaymentDbContext --output-dir Server/Features/Base/PaymentService/Data/Migrations
dotnet ef migrations add InitialSettingsDbContextMigration --context SettingsDbContext --output-dir Server/Features/Base/SettingsService/Data/Migrations
dotnet ef migrations add InitialProfileDbContextMigration --context ProfileDbContext --output-dir Server/Features/Base/ProfileService/Data/Migrations
dotnet ef migrations add InitialDonorDbContextMigration --context DonorDbContext --output-dir Server/Features/DonorService/Data/Migrations
> **Note:**
> - Make sure to update the migration names and output directories as needed for your solution.
> - After recreating migrations, update your database with `dotnet ef database update --context <DbContextName>` for each context.
# remove remote branches that have been deleted
git remote prune origin# remove local branches that have been deleted from remote
git branch -vv | Where-Object { $_ -match '\[origin/.*: gone\]' } | ForEach-Object { git branch -D $_.trim().split(" ")[0] }
