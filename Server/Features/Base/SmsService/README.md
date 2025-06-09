// filepath: c:\Users\bschn\source\repos\MSIH\msih.p4g\Server\Features\Base\SmsService\README.md
# SMS Service Module

This module provides SMS messaging capabilities using the Twilio service.

## Features

- Sending SMS messages
- Validating phone numbers (basic format validation)
- Validating phone numbers using Twilio's Lookup service
- Storing validated phone numbers in a database for future reference

## Database Schema

The module uses its own database context (`SmsDbContext`) to store phone number validation results:

- **ValidatedPhoneNumbers**: Stores validated phone numbers with their validation results, including:
  - `PhoneNumber`: The E.164 formatted phone number
  - `IsValid`: Whether the number is valid
  - `IsMobile`: Whether the number is a mobile phone (if known)
  - `Carrier`: The carrier name (if available)
  - `CountryCode`: The country code
  - `ValidatedOn`: When the number was validated
  - Audit fields: `IsActive`, `IsDeleted`, `CreatedBy`, `CreatedOn`, `ModifiedBy`, `ModifiedOn`

## Configuration

Add the following configuration to your `appsettings.json`:

```json
{
  "Twilio": {
    "AccountSid": "your-account-sid",
    "AuthToken": "your-auth-token",
    "FromNumber": "your-twilio-phone-number"
  }
}
```

## Usage

### Service Registration

The service and its dependencies are registered using the extension method:

```csharp
builder.Services.AddSmsServices(builder.Configuration);
```

### Sending SMS Messages

```csharp
public class MyService
{
    private readonly ISmsService _smsService;

    public MyService(ISmsService smsService)
    {
        _smsService = smsService;
    }

    public async Task SendNotification(string phoneNumber, string message)
    {
        await _smsService.SendSmsAsync(phoneNumber, message);
    }
}
```

### Validating Phone Numbers

```csharp
public class MyService
{
    private readonly ISmsService _smsService;

    public MyService(ISmsService smsService)
    {
        _smsService = smsService;
    }

    public async Task<bool> IsValidMobileNumber(string phoneNumber)
    {
        var result = await _smsService.ValidatePhoneNumberAsync(
            phoneNumber, 
            useCache: true, 
            usePaidService: true);
            
        return result.IsValid && result.IsMobile;
    }
}
```

### Working with Validated Phone Numbers

```csharp
public class MyService
{
    private readonly IValidatedPhoneNumberRepository _repository;

    public MyService(IValidatedPhoneNumberRepository repository)
    {
        _repository = repository;
    }

    public async Task<ValidatedPhoneNumber> GetPhoneNumberAsync(string phoneNumber)
    {
        return await _repository.GetByPhoneNumberAsync(phoneNumber);
    }
    
    // Access to the generic repository methods
    public async Task<IEnumerable<ValidatedPhoneNumber>> GetAllValidatedPhoneNumbersAsync()
    {
        // Cast to IGenericRepository to access its methods
        var genericRepo = _repository as IGenericRepository<ValidatedPhoneNumber>;
        return await genericRepo.GetAllAsync();
    }
    
    public async Task<IEnumerable<ValidatedPhoneNumber>> GetInactivePhoneNumbersAsync()
    {
        var genericRepo = _repository as IGenericRepository<ValidatedPhoneNumber>;
        return await genericRepo.GetInactiveOnlyAsync();
    }
    
    public async Task<bool> MarkPhoneNumberAsInactiveAsync(int phoneNumberId)
    {
        var genericRepo = _repository as IGenericRepository<ValidatedPhoneNumber>;
        return await genericRepo.SetActiveStatusAsync(phoneNumberId, false, "System");
    }
}
```

## Migrations

The module uses its own migrations in the `Server/Features/Base/SmsService/Data/Migrations` folder.

To create a new migration using PowerShell:

```powershell
./AddSmsMigration.ps1 -MigrationName YourMigrationName
```

To apply pending migrations:

```powershell
./ApplySmsMigrations.ps1
```

Migrations are also automatically applied at application startup through the `MigrationApplier` hosted service.

## Generic Repository Pattern

The ValidatedPhoneNumberRepository implements the generic repository pattern defined in `Server/Common/Data/Repositories`. This provides a consistent way to handle CRUD operations with support for:

- Soft deletes (using IsDeleted flag)
- Active/inactive status tracking
- Audit trails (CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)

See the [Repository Pattern Documentation](../../Common/Data/Repositories/README.md) for more details.
