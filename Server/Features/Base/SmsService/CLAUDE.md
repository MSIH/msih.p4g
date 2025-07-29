# SmsService

## Overview
The SmsService provides comprehensive SMS messaging and phone number validation capabilities through Twilio integration. It manages message delivery, phone number validation with carrier intelligence, and maintains a database of validated phone numbers for performance optimization. The service supports both basic SMS sending and advanced phone number analysis including mobile detection and carrier identification.

## Architecture

### Components
- **ISmsService**: Main service interface for SMS operations and phone validation
- **TwilioSmsService**: Twilio-based implementation for SMS and validation
- **PhoneValidationService**: Blazor-friendly service wrapper for validation operations
- **IValidatedPhoneNumberRepository**: Repository interface for phone number caching
- **ValidatedPhoneNumberRepository**: Repository implementation with add-or-update logic
- **ValidatedPhoneNumber**: Entity representing validated phone number data

### Dependencies
- Twilio SDK for SMS sending and phone lookup services
- Entity Framework Core for validated phone number persistence
- SettingsService for Twilio configuration management
- ApplicationDbContext for database operations
- ValidationUtilities for phone number format validation

## Key Features
- SMS message sending via Twilio with comprehensive error handling
- Phone number format validation using E.164 standard
- Advanced phone number validation with Twilio Lookup API
- Carrier intelligence and mobile detection capabilities
- Phone number validation result caching for performance
- Automatic database migration with hosted service
- Configuration management through SettingsService integration
- Support for both free and paid Twilio validation services

## Database Schema

### ValidatedPhoneNumber Entity
- **Id** (int): Primary key, auto-generated
- **PhoneNumber** (string): Phone number in E.164 format (max 20 chars, required)
- **IsMobile** (bool): Indicates if the number is a mobile phone
- **Carrier** (string?): Carrier name if available (max 100 chars)
- **CountryCode** (string?): Country code of the phone number (max 5 chars)
- **ValidatedOn** (DateTime): Timestamp when validation was performed
- **IsValid** (bool): Whether the phone number is valid
- **IsActive** (bool): Soft delete flag from BaseEntity
- **CreatedOn** (DateTime): Creation timestamp from BaseEntity
- **CreatedBy** (string): Creator identifier from BaseEntity
- **ModifiedOn** (DateTime?): Last modification timestamp from BaseEntity
- **ModifiedBy** (string?): Last modifier identifier from BaseEntity

### Performance Optimizations
- Indexed phone number lookups for fast retrieval
- Cached validation results to reduce API calls
- Add-or-update pattern for efficient data management

## Usage

### Sending SMS Messages
```csharp
@inject ISmsService SmsService

// Send a simple SMS message
await SmsService.SendSmsAsync("+12125551234", "Hello, this is a test message!");

// With error handling
try 
{
    await SmsService.SendSmsAsync(recipientPhone, messageContent);
    // Message sent successfully
}
catch (ArgumentException ex)
{
    // Invalid phone number format
    Console.WriteLine($"Invalid phone number: {ex.Message}");
}
catch (Exception ex)
{
    // Twilio API error or other issues
    Console.WriteLine($"Failed to send SMS: {ex.Message}");
}
```

### Phone Number Validation
```csharp
// Basic format validation (local check)
bool isValidFormat = SmsService.IsValidPhoneNumber("+12125551234");

// Advanced validation with Twilio Lookup (cached)
var validationResult = await SmsService.ValidatePhoneNumberAsync(
    phoneNumber: "+12125551234",
    useCache: true,
    usePaidService: false
);

Console.WriteLine($"Valid: {validationResult.IsValid}");
Console.WriteLine($"Mobile: {validationResult.IsMobile}");
Console.WriteLine($"Carrier: {validationResult.Carrier}");
Console.WriteLine($"Country: {validationResult.CountryCode}");
```

### Using PhoneValidationService in Blazor
```csharp
@inject PhoneValidationService PhoneValidationService

// Validate a phone number
var result = await PhoneValidationService.ValidatePhoneNumberAsync(phoneNumber);

// Get all validated phone numbers
var allValidated = await PhoneValidationService.GetAllAsync(includeInactive: false);
```

### Bulk Phone Number Analysis
```csharp
var phoneNumbers = new[] { "+12125551234", "+14155552345", "+13105553456" };

foreach (var phone in phoneNumbers)
{
    var validation = await SmsService.ValidatePhoneNumberAsync(phone, useCache: true);
    Console.WriteLine($"{phone}: Valid={validation.IsValid}, Mobile={validation.IsMobile}");
}
```

## Integration

### Twilio Configuration
The service requires Twilio configuration through SettingsService:
```json
{
  "Twilio": {
    "AccountSid": "your-twilio-account-sid",
    "AuthToken": "your-twilio-auth-token",
    "FromNumber": "+12125550123"
  }
}
```

### Service Registration
```csharp
// In Program.cs or Startup.cs
builder.Services.AddSmsServices(builder.Configuration, builder.Environment);
```

### SettingsService Integration
- Configuration values loaded through hierarchical lookup
- Database settings override appsettings.json values
- Environment variables as final fallback
- Automatic settings initialization and caching

### Profile and Communication Services Integration
- Used by ProfileService for mobile number validation
- Integration with MessageService for multi-channel communication
- Supports communication consent flags from Profile entity
- Used in donation and campaign notifications

## Advanced Features

### Validation Caching Strategy
```csharp
// Use cached results (default behavior)
var cachedResult = await SmsService.ValidatePhoneNumberAsync(phone, useCache: true);

// Force fresh validation (bypasses cache)
var freshResult = await SmsService.ValidatePhoneNumberAsync(phone, useCache: false);

// Use paid Twilio services for enhanced validation
var enhancedResult = await SmsService.ValidatePhoneNumberAsync(phone, usePaidService: true);
```

### Carrier Intelligence
The service provides detailed carrier information:
```csharp
var validation = await SmsService.ValidatePhoneNumberAsync("+12125551234");

if (validation.IsMobile)
{
    Console.WriteLine($"Mobile carrier: {validation.Carrier}");
    // Can send SMS with confidence
}
else
{
    Console.WriteLine("Landline detected - SMS may not be delivered");
}
```

### Error Handling and Logging
- Comprehensive error logging for troubleshooting
- Twilio API exception handling with graceful degradation
- Invalid phone number tracking for analytics
- Performance monitoring and validation metrics

### Database Migration
- Automatic migration application through hosted service
- Zero-downtime deployment support
- Schema evolution management
- Development and production migration strategies

## Controllers and API Integration

### PhoneValidationController
Provides HTTP endpoints for phone validation:
- Validation endpoint for external integrations
- Bulk validation support
- RESTful API design for frontend consumption

### SmsController
Provides HTTP endpoints for SMS operations:
- Message sending endpoint
- Delivery status tracking
- Rate limiting and quota management

## Files

```
Server/Features/Base/SmsService/
├── Controllers/
│   ├── PhoneValidationController.cs
│   └── SmsController.cs
├── Data/
│   ├── MigrationApplier.cs
│   ├── SmsDbContext.cs
│   ├── SmsDbContextFactory.cs
│   └── create-migration.ps1
├── Extensions/
│   └── SmsServiceExtensions.cs
├── Interfaces/
│   ├── ISmsService.cs
│   └── IValidatedPhoneNumberRepository.cs
├── Model/
│   └── ValidatedPhoneNumber.cs
├── Services/
│   ├── PhoneValidationService.cs
│   ├── TwilioSmsService.cs
│   └── ValidatedPhoneNumberRepository.cs
├── README.md
└── CLAUDE.md
```