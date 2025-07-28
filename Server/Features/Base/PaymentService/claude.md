# PaymentService

## Overview
Payment processing service with multi-provider support, starting with PayPal Braintree. Handles transaction processing, refunds, client token generation, and transaction storage for the MSIH Platform for Good application.

## Architecture
- **Services**: BraintreePaymentService, PaymentServiceFactory
- **Repositories**: PaymentTransactionRepository
- **Models**: PaymentTransaction, PaymentModels (request/response), ClientTokenRequest
- **Data**: PaymentDbContext with migration support
- **Extensions**: PaymentServiceExtensions for DI registration

## Key Features
- **Multi-Provider Design**: Extensible factory pattern for adding payment providers
- **Transaction Storage**: Complete transaction history with audit trails
- **Refund Support**: Full and partial refund capabilities
- **Client Token Generation**: Secure client-side payment token generation
- **Status Tracking**: Comprehensive payment status management
- **Error Handling**: Detailed error messaging and logging

## Database Schema

### PaymentTransaction
- TransactionId, Provider, Amount, Currency, Status
- Description, ProcessedOn, CustomerEmail
- AdditionalData (JSON), ErrorMessage, OrderReference
- Audit fields: IsActive, IsDeleted, Created/Modified tracking

## Configuration
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
```csharp
// Registration
builder.Services.AddPaymentServices(builder.Configuration, builder.Environment);

// Processing payments
var paymentService = _paymentServiceFactory.GetDefaultPaymentService();
var result = await paymentService.ProcessPaymentAsync(request);

// Generating client tokens
var tokenResult = await paymentService.GenerateClientTokenAsync(new ClientTokenRequest());

// Processing refunds
var refundResult = await paymentService.ProcessRefundAsync(refundRequest);
```

## Payment Flow
1. Generate client token for secure frontend payment form
2. Process payment with payment nonce from frontend
3. Store transaction details in database
4. Handle success/failure responses
5. Support refunds and voids as needed

## Integration
- **DonationService**: Primary consumer for donation payments
- **FundraiserService**: Payment processing for fundraiser activities
- **UserService**: Payment method management
- **PayoutService**: Integration for payment-to-payout workflows

## Extensibility
To add new payment providers:
1. Implement `IPaymentService` interface
2. Register in `PaymentServiceFactory`
3. Update `PaymentServiceExtensions` for DI registration
4. Add provider-specific configuration

## Files
- `Services/BraintreePaymentService.cs`
- `Services/PaymentServiceFactory.cs`
- `Repositories/PaymentTransactionRepository.cs`
- `Models/PaymentTransaction.cs`
- `Models/PaymentModels.cs`
- `Models/ClientTokenRequest.cs`
- `Data/PaymentDbContext.cs`
- `Extensions/PaymentServiceExtensions.cs`