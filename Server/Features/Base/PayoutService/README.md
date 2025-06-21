/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

# Payout Service Module

This module provides payout capabilities to fundraisers using various payment providers, starting with PayPal.

## Features

- Processing payouts to fundraisers via PayPal
- Single and batch payout processing
- Storing payout transaction information in a database
- Tracking payout status
- Retrieving payout history for fundraisers
- Checking batch payout status from PayPal
- Extensible design for adding new payout providers

## Database Schema

The module uses its own database context (`PayoutDbContext`) to store payout transactions:

- **Payouts**: Stores payout transaction details, including:
  - `Id`: Unique identifier for the payout
  - `FundraiserId`: ID of the fundraiser receiving the payout
  - `PaypalEmail`: Email address associated with the fundraiser's PayPal account
  - `Amount`: The payout amount
  - `Currency`: The currency code (e.g., USD, EUR)
  - `Status`: The current status of the payout (enum: Pending, Processing, Completed, Failed)
  - `Notes`: Optional notes about the payout
  - `PaypalBatchId`: The batch ID returned from PayPal after processing
  - `ErrorMessage`: Error message if the payout failed
  - `ProcessedAt`: When the payout was processed
  - `IsBatchPayout`: Whether this payout was part of a batch
  - Audit fields: `IsActive`, `IsDeleted`, `CreatedBy`, `CreatedOn`, `ModifiedBy`, `ModifiedOn`

## Configuration

Add the following configuration to your `appsettings.json`:

```json
{
  "PayPal": {
    "ClientId": "your-client-id",
    "Secret": "your-client-secret",
    "ApiUrl": "https://api.sandbox.paypal.com",  // or "https://api.paypal.com" for production
    "WebhookId": "your-webhook-id"
  }
}
```

## Usage

### Service Registration

The service and its dependencies are registered using the extension method:

```csharp
builder.Services.AddPayoutServices(builder.Configuration);
```

### Creating a Payout

```csharp
public class FundraiserService
{
    private readonly IPayoutService _payoutService;

    public FundraiserService(IPayoutService payoutService)
    {
        _payoutService = payoutService;
    }

    public async Task<Payout> CreateFundraiserPayout(string fundraiserId, string paypalEmail, decimal amount)
    {
        return await _payoutService.CreatePayoutAsync(
            fundraiserId, 
            paypalEmail, 
            amount, 
            currency: "USD", 
            notes: "Monthly fundraiser payout");
    }
}
```

### Processing a Payout

```csharp
public class PayoutProcessor
{
    private readonly IPayoutService _payoutService;

    public PayoutProcessor(IPayoutService payoutService)
    {
        _payoutService = payoutService;
    }

    public async Task<Payout> ProcessSinglePayout(string payoutId)
    {
        return await _payoutService.ProcessPayoutAsync(payoutId);
    }

    public async Task<List<Payout>> ProcessBatchPayouts(List<string> payoutIds)
    {
        return await _payoutService.ProcessBatchPayoutsAsync(payoutIds);
    }
}
```

### Retrieving Payout History

```csharp
public class FundraiserDashboardService
{
    private readonly IPayoutService _payoutService;

    public FundraiserDashboardService(IPayoutService payoutService)
    {
        _payoutService = payoutService;
    }

    public async Task<List<Payout>> GetFundraiserPayoutHistory(string fundraiserId)
    {
        return await _payoutService.GetFundraiserPayoutHistoryAsync(fundraiserId, page: 1, pageSize: 10);
    }
}
```

### Checking Batch Payout Status

```csharp
public class PayoutMonitoringService
{
    private readonly IPayoutService _payoutService;
    private readonly ILogger<PayoutMonitoringService> _logger;

    public PayoutMonitoringService(
        IPayoutService payoutService,
        ILogger<PayoutMonitoringService> logger)
    {
        _payoutService = payoutService;
        _logger = logger;
    }

    public async Task CheckBatchStatus(string batchId)
    {
        try
        {
            var batchStatus = await _payoutService.GetBatchPayoutStatusAsync(batchId);
            
            _logger.LogInformation(
                "Batch {BatchId} status: {Status}, Success: {SuccessCount}, Error: {ErrorCount}",
                batchStatus.BatchId,
                batchStatus.Status,
                batchStatus.SuccessCount,
                batchStatus.ErrorCount);
                
            // Take appropriate actions based on status
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking batch status: {ErrorMessage}", ex.Message);
        }
    }
}
```

## Batch Size Limitations

The PayPal payout service has a maximum batch size limit (PayPal's limit is 15,000 items per batch), but this implementation sets a conservative limit of 1,000 payouts per batch. This can be adjusted in the `PayPalPayoutService` class if needed.

## Error Handling

The service includes comprehensive error handling for both single payouts and batch payouts. Failed payouts will have their status set to `Failed` and include an error message. All payout processing operations are logged for debugging and auditing purposes.

## Testing

Unit tests for the PayoutService are available in the `Server.Tests.Features.Base.PayoutService` namespace.
