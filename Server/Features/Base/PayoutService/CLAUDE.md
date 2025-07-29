# PayoutService

## Overview
The PayoutService is a comprehensive payment processing service that handles disbursing funds to fundraisers via PayPal. It manages payout creation, batch processing, status tracking, and integration with PayPal's Payouts API. This service enables organizations to efficiently distribute earnings to their fundraisers with full audit trails and error handling.

## Architecture

### Components
- **IPayoutService**: Main service interface for payout operations
- **PayPalPayoutService**: Implementation handling PayPal API integration
- **IPayoutRepository**: Data access interface for payout entities
- **PayoutRepository**: Repository implementation with specialized queries
- **IPayPalApiClient**: Interface for PayPal API communication
- **PayPalApiClient**: PayPal API client implementation

### Dependencies
- Entity Framework Core for data persistence
- HttpClient for PayPal API communication
- Microsoft.Extensions.Options for configuration
- Microsoft.Extensions.Logging for structured logging
- ApplicationDbContext for database operations

## Key Features
- Create individual payout records for fundraisers
- Process batch payouts with up to 1,000 items per batch
- Real-time PayPal batch status tracking and updates
- Support for PayPal and Venmo account types
- Comprehensive error handling and logging
- Automatic access token management for PayPal API
- Paged queries for large result sets
- Audit trail with creation and processing timestamps

## Database Schema

### Payout Entity
- **Id** (int): Primary key, auto-generated
- **FundraiserId** (string): Reference to fundraiser receiving payout
- **PayoutAccount** (string): PayPal email or account identifier
- **PayoutAccountType** (enum): PayPal or Venmo account type
- **PayoutAccountFormat** (enum): Email, Mobile, or Handle format
- **Amount** (decimal): Payout amount
- **Currency** (string): Currency code (default: USD)
- **BatchStatus** (enum): PayPal batch processing status
- **TransactionStatus** (enum): Individual transaction status
- **FeeAmount** (decimal?): PayPal processing fee
- **PaypalSenderId** (string?): PayPal sender batch ID
- **PaypalBatchId** (string?): PayPal batch ID
- **PaypalPayoutItemId** (string?): PayPal payout item ID
- **PaypalTransactionId** (string?): PayPal transaction ID
- **CreatedAt** (DateTime): Record creation timestamp
- **ProcessedAt** (DateTime?): PayPal processing timestamp
- **Notes** (string?): Optional payout notes
- **ErrorMessage** (string?): Error details if processing failed
- **IsBatchPayout** (bool): Indicates if part of batch processing

### PayPal Status Enums
- **PayPalBatchStatusEnum**: PENDING, PROCESSING, SUCCESS, DENIED, CANCELED, ERROR
- **PayPalTransactionStatusEnum**: SUCCESS, FAILED, PENDING, UNCLAIMED, RETURNED, ONHOLD, BLOCKED, REFUNDED, REVERSED

## Usage

### Creating a Payout
```csharp
@inject IPayoutService PayoutService

var payout = await PayoutService.CreatePayoutAsync(
    fundraiserId: "fundraiser123",
    paypalEmail: "fundraiser@example.com",
    amount: 250.00m,
    currency: "USD",
    notes: "Monthly payout for campaign ABC"
);
```

### Processing Batch Payouts
```csharp
var payoutIds = new List<string> { "1", "2", "3", "4", "5" };
var processedPayouts = await PayoutService.ProcessBatchPayoutsAsync(payoutIds);

foreach (var payout in processedPayouts)
{
    Console.WriteLine($"Payout {payout.Id} - Status: {payout.BatchStatus}");
}
```

### Retrieving Payout History
```csharp
// Get payout history for a fundraiser
var history = await PayoutService.GetFundraiserPayoutHistoryAsync(
    fundraiserId: "fundraiser123",
    page: 1,
    pageSize: 20
);

// Get payouts by status
var pendingPayouts = await PayoutService.GetPayoutsByStatusAsync(
    status: PayPalTransactionStatusEnum.PENDING,
    page: 1,
    pageSize: 50
);
```

### Checking Batch Status
```csharp
var batchStatus = await PayoutService.GetBatchPayoutStatusAsync("BATCH_ID_12345");
Console.WriteLine($"Batch Status: {batchStatus.BatchHeader.BatchStatus}");
Console.WriteLine($"Items Count: {batchStatus.BatchHeader.PayoutItemCount}");
```

## Integration

### PayPal Configuration
The service requires PayPal API configuration in appsettings.json:
```json
{
  "PayPal": {
    "ClientId": "your-paypal-client-id",
    "Secret": "your-paypal-secret",
    "ApiUrl": "https://api.paypal.com" // or sandbox URL
  }
}
```

### Service Registration
```csharp
// In Program.cs or Startup.cs
builder.Services.AddPayoutServices(builder.Configuration, builder.Environment);
```

### FundraiserService Integration
- Uses FundraiserService AccountType and AccountFormat enums
- Validates fundraiser existence before payout creation
- Integrates with fundraiser payout account settings

### Audit and Monitoring
- All operations logged with structured logging
- Automatic audit trail through BaseEntity inheritance
- Error tracking and reporting for failed transactions
- Integration with application monitoring systems

## Files

```
Server/Features/Base/PayoutService/
├── Controllers/
│   └── PayoutController.cs
├── Extensions/
│   └── PayoutServiceExtensions.cs
├── Interfaces/
│   ├── IPayPalApiClient.cs
│   ├── IPayoutRepository.cs
│   └── IPayoutService.cs
├── Models/
│   ├── Configuration/
│   │   └── PayPalOptions.cs
│   ├── PayPal/
│   │   ├── PayPalBatchStatus.cs
│   │   ├── PayPalBatchStatusDto.cs
│   │   ├── PayPalPayoutRequest.cs
│   │   ├── PayPalPayoutResponse.cs
│   │   └── PayPalTokenResponse.cs
│   ├── Payout.cs
│   ├── PayoutDto.cs
│   └── PayoutStatus.cs
├── Repositories/
│   └── PayoutRepository.cs
├── Services/
│   ├── PayPalApiClient.cs
│   └── PayPalPayoutService.cs
├── Utilities/
│   └── PayoutExtensions.cs
├── README.md
└── CLAUDE.md
```