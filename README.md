# MSIH Project for Good (P4G)

## Overview
This is a .NET 9 Blazor Server application following clean architecture principles.

## Licensing & Copyright

All source code in this repository is protected by copyright:

```
Copyright (c) 2025 MSIH LLC. All rights reserved.
This file is developed for Make Sure It Happens Inc.
Unauthorized copying, modification, distribution, or use is prohibited.
```

### License Headers

All source code files in this repository must include the appropriate license header. The repository includes two mechanisms to ensure this:

1. **For new files**: The `.editorconfig` file configures Visual Studio and VS Code to automatically add license headers to new files.

2. **For existing files**: Use the PowerShell script to add license headers:

```powershell
# Show help
.\Add-LicenseHeaders.ps1 -Help

# Preview changes without modifying files
.\Add-LicenseHeaders.ps1 -WhatIf

# Add license headers to all files
.\Add-LicenseHeaders.ps1
```

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
