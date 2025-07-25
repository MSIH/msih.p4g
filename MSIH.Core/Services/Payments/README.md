/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

# Payment Service Module

This module provides payment processing capabilities using various payment providers, starting with PayPal Braintree.

## Features

- Processing payments through PayPal Braintree
- Storing payment transaction information in a database
- Tracking payment status
- Refunding and voiding transactions
- Extensible design for adding new payment providers

## Database Schema

The module uses its own database context (`PaymentDbContext`) to store payment transactions:

- **PaymentTransactions**: Stores payment transaction details, including:
  - `TransactionId`: The transaction identifier from the payment provider
  - `Provider`: The payment provider used (e.g., "Braintree")
  - `Amount`: The payment amount
  - `Currency`: The currency code (e.g., USD, EUR)
  - `Status`: The current status of the transaction (enum)
  - `Description`: Optional description of what the payment was for
  - `ProcessedOn`: When the payment was processed
  - `CustomerEmail`: Email of the customer who made the payment
  - `AdditionalData`: Any additional data in JSON format
  - `ErrorMessage`: Error message if the payment failed
  - `OrderReference`: Reference to the order or item being paid for
  - Audit fields: `IsActive`, `IsDeleted`, `CreatedBy`, `CreatedOn`, `ModifiedBy`, `ModifiedOn`

## Configuration

Add the following configuration to your `appsettings.json`:

```json
{
  "Braintree": {
    "MerchantId": "your-merchant-id",
    "PublicKey": "your-public-key",
    "PrivateKey": "your-private-key",
    "Environment": "Sandbox"  // or "Production"
  }
}
```

## Usage

### Service Registration

The service and its dependencies are registered using the extension method:

```csharp
builder.Services.AddPaymentServices(builder.Configuration, builder.Environment);
```

### Processing Payments

```csharp
public class PaymentController
{
    private readonly IPaymentServiceFactory _paymentServiceFactory;

    public PaymentController(IPaymentServiceFactory paymentServiceFactory)
    {
        _paymentServiceFactory = paymentServiceFactory;
    }

    public async Task<IActionResult> ProcessPayment(PaymentRequest request)
    {
        // Get the default payment service (Braintree)
        var paymentService = _paymentServiceFactory.GetDefaultPaymentService();
        
        // Or get a specific provider
        // var paymentService = _paymentServiceFactory.GetPaymentService("Braintree");
        
        var result = await paymentService.ProcessPaymentAsync(request);
        
        if (result.Success)
        {
            // Payment succeeded
            return Ok(result);
        }
        else
        {
            // Payment failed
            return BadRequest(result);
        }
    }
}
```

### Generating Client Tokens

```csharp
public class PaymentController
{
    private readonly IPaymentServiceFactory _paymentServiceFactory;

    public PaymentController(IPaymentServiceFactory paymentServiceFactory)
    {
        _paymentServiceFactory = paymentServiceFactory;
    }

    public async Task<IActionResult> GetClientToken()
    {
        var paymentService = _paymentServiceFactory.GetDefaultPaymentService();
        var result = await paymentService.GenerateClientTokenAsync(new ClientTokenRequest());
        
        if (result.Success)
        {
            return Ok(result);
        }
        else
        {
            return BadRequest(result);
        }
    }
}
```

### Processing Refunds

```csharp
public class PaymentController
{
    private readonly IPaymentServiceFactory _paymentServiceFactory;

    public PaymentController(IPaymentServiceFactory paymentServiceFactory)
    {
        _paymentServiceFactory = paymentServiceFactory;
    }

    public async Task<IActionResult> RefundPayment(RefundRequest request)
    {
        var paymentService = _paymentServiceFactory.GetDefaultPaymentService();
        var result = await paymentService.ProcessRefundAsync(request);
        
        if (result.Success)
        {
            return Ok(result);
        }
        else
        {
            return BadRequest(result);
        }
    }
}
```

## Adding New Payment Providers

To add a new payment provider:

1. Create a new class that implements `IPaymentService`
2. Register the new service in the `PaymentServiceFactory`
3. Update the `PaymentServiceExtensions` class to register the new service

## Migrations

The module uses its own migrations in the `Server/Features/Base/PaymentService/Data/Migrations` folder.

To create a new migration using PowerShell:

```powershell
./AddPaymentMigration.ps1 -MigrationName YourMigrationName
```

Migrations are automatically applied at application startup through the `MigrationApplier` hosted service.

## Generic Repository Pattern

The PaymentTransactionRepository implements the generic repository pattern defined in `Server/Common/Data/Repositories`. This provides a consistent way to handle CRUD operations with support for:

- Soft deletes (using IsDeleted flag)
- Active/inactive status tracking
- Audit trails (CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)