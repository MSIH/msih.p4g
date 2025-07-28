# RecurringDonationService

## Overview
Complete recurring donation system that enables donors to set up automatic monthly or yearly donations. Integrates with the existing payment and donation systems to process recurring payments seamlessly.

## Architecture
- **Services**: RecurringDonationService, RecurringDonationProcessingService (background)
- **Repository**: RecurringDonationRepository
- **Models**: RecurringDonation, RecurringDonationDto, CreateRecurringDonationDto, UpdateRecurringDonationDto
- **Controller**: RecurringDonationController
- **Data**: RecurringDonationDbContext
- **Extensions**: RecurringDonationServiceExtensions for DI registration

## Key Features
- **Automatic Processing**: Background service processes recurring donations hourly
- **Flexible Frequency**: Monthly and annual recurring donations
- **Payment Integration**: Uses existing PaymentService for transaction processing
- **Failure Handling**: Retry logic with maximum attempt limits
- **Status Management**: Active, Paused, Cancelled, Failed, Expired statuses
- **Donor Control**: Full donor management - pause, resume, cancel, update amounts
- **Payment Method Updates**: Update payment methods for existing recurring donations
- **Audit Trail**: Complete tracking of processing history and failures

## Database Schema

### RecurringDonation
- **Core**: DonorId, Amount, Frequency, Currency, Status
- **Scheduling**: StartDate, EndDate, NextProcessDate, LastProcessedDate  
- **Payment**: PaymentMethodToken, PayTransactionFee, PayTransactionFeeAmount
- **Content**: DonationMessage, ReferralCode, CampaignCode, CampaignId
- **Tracking**: SuccessfulDonationsCount, FailedAttemptsCount, LastErrorMessage
- **Cancellation**: CancelledDate, CancelledBy, CancellationReason

### Enums
- **RecurringFrequency**: Monthly (1), Annually (12)
- **RecurringDonationStatus**: Active, Paused, Cancelled, Failed, Expired

## Usage

### Service Registration
```csharp
// In Program.cs
builder.Services.AddRecurringDonationServices(builder.Configuration, builder.Environment);

// Ensure database creation (development)
await app.Services.EnsureRecurringDonationDatabaseAsync();
```

### Creating Recurring Donations
```csharp
var recurringDonation = new RecurringDonation
{
    DonorId = donor.Id,
    Amount = 100.00m,
    Frequency = RecurringFrequency.Monthly,
    StartDate = DateTime.UtcNow,
    PaymentMethodToken = "payment_method_token",
    PayTransactionFee = true,
    PayTransactionFeeAmount = 3.20m,
    DonationMessage = "Monthly support",
    ReferralCode = "FRIEND123",
    CampaignId = 1
};

var result = await _recurringDonationService.CreateRecurringDonationAsync(recurringDonation, "user@email.com");
```

### Processing Logic
```csharp
// Background service automatically processes every hour
// Or manually trigger:
var processedCount = await _recurringDonationService.ProcessDueRecurringDonationsAsync();

// Process specific recurring donation:
var success = await _recurringDonationService.ProcessRecurringDonationAsync(recurringDonationId);
```

### Management Operations
```csharp
// Pause/Resume
await _recurringDonationService.PauseRecurringDonationAsync(id, "user@email.com");
await _recurringDonationService.ResumeRecurringDonationAsync(id, "user@email.com");

// Cancel
await _recurringDonationService.CancelRecurringDonationAsync(id, "user@email.com", "No longer needed");

// Update amount
await _recurringDonationService.UpdateAmountAsync(id, 150.00m, "user@email.com");

// Update payment method
await _recurringDonationService.UpdatePaymentMethodAsync(id, "new_payment_token", "user@email.com");
```

## API Endpoints
- `POST /api/recurringdonation` - Create recurring donation
- `GET /api/recurringdonation/{id}` - Get by ID
- `GET /api/recurringdonation/user/{email}` - Get user's recurring donations
- `PUT /api/recurringdonation/{id}` - Update recurring donation
- `POST /api/recurringdonation/{id}/pause` - Pause
- `POST /api/recurringdonation/{id}/resume` - Resume  
- `POST /api/recurringdonation/{id}/cancel` - Cancel

## Background Processing
- **RecurringDonationProcessingService**: Hosted service runs every hour
- **Automatic Processing**: Finds due recurring donations and processes them
- **Failure Handling**: Retries failed payments up to 3 times
- **Status Updates**: Updates next process dates and counters
- **Error Logging**: Comprehensive logging for debugging

## Integration Points
- **PaymentService**: Processes recurring payments using stored payment methods
- **DonationService**: Creates donation records for each successful recurring payment
- **MessageService**: Sends thank you emails for successful recurring donations
- **UserService**: Links to donor and user information

## Error Handling
- **Payment Failures**: Retry up to 3 times before marking as failed
- **Invalid Payment Methods**: Automatic status change to failed
- **Network Issues**: Robust retry logic with exponential backoff
- **Database Errors**: Transaction rollback and proper error logging

## Files
- `Models/RecurringDonation.cs` - Main entity
- `Models/RecurringDonationDto.cs` - API DTOs
- `Services/RecurringDonationService.cs` - Core service logic
- `Services/RecurringDonationProcessingService.cs` - Background processing
- `Repositories/RecurringDonationRepository.cs` - Data access
- `Controllers/RecurringDonationController.cs` - REST API
- `Data/RecurringDonationDbContext.cs` - Database context
- `Extensions/RecurringDonationServiceExtensions.cs` - DI registration