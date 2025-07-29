# SettingsService

## Overview
The SettingsService provides a comprehensive configuration management system that handles application settings with a hierarchical lookup strategy. It manages key-value pairs stored in the database while providing fallback to appsettings.json and environment variables. The service includes data integrity features, caching capabilities, and initialization tools to ensure consistent configuration management across the application.

## Architecture

### Components
- **ISettingsService**: Main service interface for settings operations
- **SettingsService**: Core implementation with hierarchical value resolution
- **ISettingRepository**: Specialized repository interface for setting entities
- **SettingRepository**: Repository with caching and specialized query optimization
- **SettingsInitializer**: Service for initializing database settings from configuration
- **Setting**: Entity representing key-value configuration pairs

### Dependencies
- Entity Framework Core for data persistence
- ApplicationDbContext for database operations
- IConfiguration for appsettings.json access
- ICacheStrategy for performance optimization
- Microsoft.Extensions.Logging for structured logging

## Key Features
- Hierarchical value lookup (Database → appsettings.json → Environment Variables)
- Automatic database initialization from configuration files
- Data integrity and duplicate cleanup functionality
- Performance optimization through caching with custom cache key generation
- Setting versioning and audit trail through BaseEntity
- Configuration section batch initialization
- Setting validation and cleanup operations

## Database Schema

### Setting Entity
- **Id** (int): Primary key, auto-generated
- **Key** (string): Setting key/name (max 100 chars, required, indexed)
- **Value** (string?): Setting value (max 1000 chars, nullable)
- **IsActive** (bool): Soft delete flag from BaseEntity
- **CreatedOn** (DateTime): Creation timestamp from BaseEntity
- **CreatedBy** (string): Creator identifier from BaseEntity
- **ModifiedOn** (DateTime?): Last modification timestamp from BaseEntity
- **ModifiedBy** (string?): Last modifier identifier from BaseEntity

### Database Constraints
- Required Key field with maximum length validation
- Optimized for key-based lookups with custom caching
- Soft delete support for data integrity

## Usage

### Basic Setting Operations
```csharp
@inject ISettingsService SettingsService

// Get a setting value (checks DB, then appsettings, then environment)
var smtpServer = await SettingsService.GetValueAsync("SendGrid:SmtpServer");

// Set or update a setting value in the database
await SettingsService.SetValueAsync("SendGrid:ApiKey", "new-api-key", "AdminUser");

// Get all settings as a dictionary
var allSettings = await SettingsService.GetAllAsync();
foreach (var setting in allSettings)
{
    Console.WriteLine($"{setting.Key}: {setting.Value}");
}
```

### Hierarchical Value Resolution
The service follows a specific lookup order:
1. **Database**: First checks if the setting exists in the Settings table
2. **appsettings.json**: If not in DB, checks configuration files
3. **Environment Variables**: Final fallback to system environment variables
4. **Auto-Creation**: If found in appsettings or environment, creates DB entry

```csharp
// This will:
// 1. Check Settings table for "Email:SmtpHost"
// 2. If not found, check appsettings.json for "Email:SmtpHost"
// 3. If not found, check environment variable "Email:SmtpHost"
// 4. Create DB entry with resolved value (or null if not found)
var smtpHost = await SettingsService.GetValueAsync("Email:SmtpHost");
```

### Settings Cleanup Operations
```csharp
// Clean up duplicate settings and invalid entries
var cleanupResult = await SettingsService.CleanupSettingsAsync();

if (cleanupResult.Success)
{
    Console.WriteLine($"Duplicates found: {cleanupResult.DuplicatesFound}");
    Console.WriteLine($"Duplicates removed: {cleanupResult.DuplicatesRemoved}");
    Console.WriteLine($"Invalid entries removed: {cleanupResult.InvalidEntriesRemoved}");
}
else
{
    Console.WriteLine($"Cleanup failed: {cleanupResult.ErrorMessage}");
}
```

### Settings Initialization
```csharp
@inject SettingsInitializer SettingsInitializer

// Initialize database settings from appsettings.json
await SettingsInitializer.InitializeSettingsAsync();
```

## Integration

### Configuration Integration
The service integrates with ASP.NET Core's configuration system:
```json
// appsettings.json
{
  "SendGrid": {
    "ApiKey": "your-sendgrid-key",
    "SmtpServer": "smtp.sendgrid.net"
  },
  "Braintree": {
    "Environment": "Sandbox",
    "MerchantId": "your-merchant-id"
  },
  "BaseUrl": "https://localhost:7001"
}
```

### Service Registration
```csharp
// In Program.cs or Startup.cs
builder.Services.AddSettingsServices();

// Optional: Run initialization on startup
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<SettingsInitializer>();
    await initializer.InitializeSettingsAsync();
}
```

### Caching Integration
The repository implements intelligent caching:
- Custom cache key generation for setting lookups
- Special handling for key-based queries
- Cache invalidation on updates
- Optimized for frequent setting access patterns

### Other Service Integration
- **EmailService**: Gets SMTP configuration from settings
- **PaymentService**: Retrieves payment gateway credentials
- **MessageService**: Accesses messaging service configuration
- **SmsService**: Gets SMS provider settings

## Advanced Features

### Section Initialization
The SettingsInitializer can initialize entire configuration sections:
```csharp
// Initializes all keys under "SendGrid:" prefix
await InitializeSectionAsync("SendGrid");

// Results in database entries like:
// "SendGrid:ApiKey" → "your-api-key"
// "SendGrid:SmtpServer" → "smtp.sendgrid.net"
```

### Duplicate Detection and Cleanup
The service includes comprehensive data integrity features:
- Identifies duplicate keys in the database
- Keeps the most recent entry when duplicates exist
- Removes invalid entries (empty/null keys)
- Provides detailed cleanup reporting

### Performance Optimizations
- **Caching**: Intelligent caching with custom key generation
- **Query Optimization**: Specialized queries for key-based lookups
- **Expression Parsing**: Advanced predicate analysis for cache keys
- **Singleton Repository**: Registered as singleton for optimal performance

## Files

```
Server/Features/Base/SettingsService/
├── Data/
│   └── SettingsDbContext.cs
├── Extensions/
│   └── SettingsServiceExtensions.cs
├── Interfaces/
│   ├── ISettingRepository.cs
│   └── ISettingsService.cs
├── Model/
│   └── Setting.cs
├── Repositories/
│   └── SettingRepository.cs
├── Services/
│   ├── SettingsInitializer.cs
│   └── SettingsService.cs
└── CLAUDE.md
```