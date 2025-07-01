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


## Azure 

### Apps
- gd4-org-abhyagf2dygvffa3.westcentralus-01.azurewebsites.net
- dev-gd4-org-f6avh3bzcfcbe8gr.westcentralus-01.azurewebsites.net
- test-gd4-org-hfecfhemeqgfepbp.westcentralus-01.azurewebsites.net

#### Environment Variables
- `ASPNETCORE_ENVIRONMENT`: Set to `Development`, `Staging`, or `Production` as needed.
- `ConnectionStrings:DefaultConnection`: Set to your database connection string.
- UseSqlServer = true

### Databases
- nonprod-gd4.database.windows.net / msihadmin
- Server=tcp:nonprod-gd4.database.windows.net,1433;Initial Catalog=test-gd4;Persist Security Info=False;User ID=msihadmin;Password={your_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
- Server=tcp:nonprod-gd4.database.windows.net,1433;Initial Catalog=nonprod-pg4;Persist Security Info=False;User ID=msihadmin;Password={your_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
- 
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

## Resetting All EF Core Migrations (PowerShell)

To completely reset all migrations for all models/DbContexts, run the following PowerShell commands from the root of your repository:

# Remove all migration files for all DbContexts
Remove-Item -Recurse -Force .\Server/Common/Data/Migrations/*

# Recreate initial migrations for each DbContext (example for UserDbContext)

dotnet ef migrations add Initial --context ApplicationDbContext --output-dir Server/Common/Data/Migrations


> **Note:**
> - Make sure to update the migration names and output directories as needed for your solution.
> - After recreating migrations, update your database with `dotnet ef database update --context <DbContextName>` for each context.
  - 
# remove remote branches that have been deleted
git remote prune origin 

# remove local branches that have been deleted from remote

git branch -vv | Where-Object { $_ -match '\[origin/.*: gone\]' } | ForEach-Object { git branch -D $_.trim().split(" ")[0] }
