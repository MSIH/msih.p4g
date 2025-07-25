/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

# Payout Service Module

This module provides payout capabilities to fundraisers using various payment providers, starting with PayPal. It manages the entire payout lifecycle, from creation to processing and status tracking.

## Features

- Processing payouts to fundraisers via PayPal
- Support for multiple account types (PayPal, Bank, Venmo)
- Support for different account formats (Email, Phone, AccountNumber, Username)
- Single and batch payout processing
- Storing payout transaction information in a database
- Tracking payout status (Pending, Processing, Completed, Failed, Cancelled)
- Retrieving payout history for fundraisers
- Checking batch payout status from PayPal
- Extensible design for adding new payout providers

## Database Schema

The Payout model includes the following properties:

- **Payout**:
  - `Id`: Unique identifier for the payout
  - `FundraiserId`: ID of the fundraiser receiving the payout
  - `PayoutAccount`: Account identifier for the payout (e.g., PayPal email, bank account)
  - `PayoutAccountType`: Type of account (PayPal, Bank, Venmo, Other)
  - `PayoutAccountFormat`: Format of the account identifier (Email, Phone, AccountNumber, Username, Other)
  - `Amount`: The payout amount
  - `Currency`: The currency code (e.g., USD, EUR)
  - `Status`: The current status of the payout (Pending, Processing, Completed, Failed, Cancelled)
  - `PaypalBatchId`: The batch ID returned from PayPal for batch payouts
  - `PaypalPayoutItemId`: The payout item ID for individual payouts
  - `PaypalTransactionId`: Transaction ID returned by PayPal
  - `CreatedAt`: When the payout record was created
  - `ProcessedAt`: When the payout was processed
  - `Notes`: Optional notes about the payout
  - `ErrorMessage`: Error message if the payout failed
  - `IsBatchPayout`: Whether this payout was part of a batch

## Configuration

Add the following configuration to your `appsettings.json`:
{
  "PayPal": {
    "ClientId": "your-client-id",
    "Secret": "your-client-secret",
    "ApiUrl": "https://api.sandbox.paypal.com",  // or "https://api.paypal.com" for production
    "WebhookId": "your-webhook-id"
  }
}
## Usage

### Service Registration

The service and its dependencies are registered using the extension method:
builder.Services.AddPayoutServices(builder.Configuration, builder.Environment);
### Creating a Payout
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
### Processing a Payout
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
### Retrieving Payout History
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
### Checking Batch Payout Status
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
## Repository Functionality

The `IPayoutRepository` interface provides these core operations:

- `GetPayoutsByStatusAsync`: Get payouts filtered by status
- `GetPayoutsByFundraiserIdAsync`: Get payouts for a specific fundraiser
- `GetPayoutsByBatchIdAsync`: Get payouts that are part of a specific batch
- `GetPayoutsByIdsAsync`: Get multiple payouts by their IDs
- `UpdateRangeAsync`: Update multiple payout entities at once

## Account Types and Formats

The service supports different account types and formats:

- **Account Types**:
  - PayPal
  - Bank
  - Venmo
  - Other

- **Account Formats**:
  - Email
  - Phone
  - AccountNumber
  - Username
  - Other

## Batch Size Limitations

The PayPal payout service has a maximum batch size limit (PayPal's limit is 15,000 items per batch), but this implementation sets a conservative limit of 1,000 payouts per batch. This can be adjusted in the `PayPalPayoutService` class if needed.

## Error Handling

The service includes comprehensive error handling for both single payouts and batch payouts. Failed payouts will have their status set to `Failed` and include an error message. All payout processing operations are logged for debugging and auditing purposes.

## Testing

Unit tests for the PayoutService are available in the `Server.Tests.Features.Base.PayoutService` namespace.
