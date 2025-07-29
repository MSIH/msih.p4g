# DonationService Feature - Complete Donation Processing System

## Overview
The DonationService feature is a comprehensive donation processing system that handles all aspects of donations in the MSIH Platform for Good. This feature manages one-time and recurring donations, payment processing integration, donor management coordination, transaction fee handling, and automated messaging. It serves as the central hub for processing donations from donors while coordinating with payment providers, donor management, user profiles, and affiliate monitoring systems.

## Architecture
The DonationService follows the established service architecture pattern with clean separation of concerns:

### Core Components
- **Service Layer**: `DonationService` - Main business logic for donation processing
- **Background Service**: `RecurringDonationProcessingService` - Automated recurring donation processing
- **Repository Layer**: `DonationRepository` - Data access with advanced querying capabilities
- **Data Transfer Objects**: `DonationRequestDto` - Client request structure
- **Entity Model**: `Donation` - Core donation entity with comprehensive relationships
- **Database Configuration**: Partial `ApplicationDbContext` with entity relationships
- **Service Registration**: `DonationServiceExtensions` - Dependency injection setup

### Integration Points
- **PaymentService**: Processes payments and manages transaction records
- **DonorService**: Creates and manages donor records
- **UserService**: Manages user accounts and authentication
- **ProfileService**: Handles user profile information and addresses
- **MessageService**: Sends confirmation emails and notifications
- **AffiliateMonitoringService**: Tracks and manages affiliate relationships
- **CampaignService**: Links donations to specific campaigns

## Key Features

### Donation Processing
- **One-time Donations**: Complete payment processing with immediate confirmation
- **Recurring Donations**: Monthly and annual subscription setup with automated processing
- **Transaction Fee Handling**: Configurable fee structure (2.9% + $0.30) with optional donor coverage
- **Minimum Donation Amount**: $25.00 minimum enforced at validation level
- **Payment Integration**: Braintree payment processing with stored payment tokens
- **Self-referral Prevention**: Automatic detection and prevention of users using their own referral codes

### Donor Management Integration
- **Automatic User Creation**: Creates user accounts and profiles for new donors
- **Existing User Updates**: Updates profile information for returning donors
- **Donor Record Management**: Links donations to donor profiles with referral tracking
- **Profile Synchronization**: Maintains consistency between donation and profile data

### Recurring Donation System
- **Setup Records**: Parent donation records that define recurring parameters
- **Payment Records**: Child donation records created for each automated payment
- **Token Storage**: Secure storage of payment method tokens for automated processing
- **Schedule Management**: Automatic calculation of next processing dates
- **Background Processing**: Hourly automated processing of due recurring donations
- **User Management**: Cancel, update, and payment method change capabilities

### Advanced Querying and Reporting
- **Pagination Support**: Efficient handling of large donation datasets
- **Search Capabilities**: Multi-field search across donations, campaigns, and referrals
- **Filter Options**: Filter by donation type (one-time, monthly, annual, recurring)
- **User-specific Queries**: Secure access to user's own donation history
- **Campaign Analytics**: Total amounts raised per campaign
- **Referral Tracking**: Donations attributed to specific referral codes

### Audit and Compliance
- **Complete Audit Trail**: Created/modified timestamps and user tracking
- **Soft Deletes**: IsActive flag for safe record management
- **Transaction Linking**: Direct links to payment processor transaction records
- **Message Tracking**: Integration with messaging system for confirmations

## Database Schema

### Donation Entity
```sql
Donations
├── Id (int, PK) - Primary key
├── DonationAmount (decimal, required) - Base donation amount
├── PayTransactionFee (bool) - Whether donor covers transaction fees
├── PayTransactionFeeAmount (decimal) - Amount of transaction fee covered
├── DonorId (int, FK, required) - Reference to Donor
├── PaymentTransactionId (int, FK, nullable) - Reference to PaymentTransaction
├── IsMonthly (bool) - Monthly recurring donation flag
├── IsAnnual (bool) - Annual recurring donation flag
├── NextProcessDate (datetime, nullable) - Next recurring payment date
├── RecurringPaymentToken (string, nullable) - Stored payment method
├── ParentRecurringDonationId (int, FK, nullable) - Reference to setup donation
├── DonationMessage (string, nullable) - Optional donor message
├── ReferralCode (string, nullable) - Affiliate referral code
├── CampaignCode (string, nullable) - Campaign identifier
├── CampaignId (int, FK, nullable) - Reference to Campaign
└── BaseEntity fields (IsActive, Created/Modified audit fields)
```

### Relationships
- **Donor (1:N)**: Each donation belongs to one donor; donors can have multiple donations
- **PaymentTransaction**: Links to payment processor transaction record
- **Campaign**: Optional association with specific fundraising campaigns
- **Self-referencing**: Parent/child relationship for recurring donation management

## Business Logic

### Donation Processing Workflow
1. **Request Validation**: Validate donation amount, payment token, and user information
2. **User/Donor Resolution**: Find existing user/donor or create new records
3. **Profile Management**: Update or create user profile with donation information
4. **Self-referral Check**: Prevent users from using their own referral codes
5. **Payment Processing**: Process payment through PaymentService integration
6. **Donation Record Creation**: Create donation record with all relationships
7. **Recurring Setup**: Configure recurring parameters if applicable
8. **Confirmation Messaging**: Send thank you email to donor
9. **Affiliate Monitoring**: Check affiliate status for suspicious activity

### Recurring Donation Processing
1. **Scheduled Discovery**: Hourly background service finds due recurring donations
2. **Payment Execution**: Process stored payment methods for due donations
3. **Record Creation**: Create child donation records for successful payments
4. **Schedule Update**: Calculate and set next processing date
5. **Error Handling**: Log failures and maintain donation setup for retry

### Transaction Fee Calculation
- **Percentage Fee**: 2.9% of donation amount
- **Flat Fee**: $0.30 per transaction
- **Optional Coverage**: Donor can choose to cover fees
- **Total Calculation**: Base amount + fees (if covered by donor)

## Integration Points

### PaymentService Integration
- **Payment Processing**: Submit payment requests with donor information
- **Transaction Tracking**: Link donations to payment processor transactions
- **Token Management**: Store and reuse payment tokens for recurring donations
- **Error Handling**: Manage payment failures and retry logic

### DonorService Integration
- **Donor Creation**: Automatic donor record creation for new users
- **Record Updates**: Maintain donor information consistency
- **Referral Management**: Track referral codes and affiliate relationships

### UserProfileService Integration
- **Account Management**: Create user accounts with profiles in single operation
- **Profile Updates**: Synchronize profile changes from donation form data
- **Address Management**: Handle address information for donations

### MessageService Integration
- **Thank You Emails**: Automated confirmation emails using templates
- **Template Processing**: Dynamic content replacement with donation details
- **Delivery Tracking**: Log email delivery success/failure

### AffiliateMonitoringService Integration
- **Suspension Checks**: Monitor affiliate accounts for suspicious activity
- **Referral Validation**: Verify referral codes and affiliate status
- **Compliance Monitoring**: Track patterns that might indicate fraud

## Usage Examples

### Basic One-time Donation Processing
```csharp
var donationRequest = new DonationRequestDto
{
    FirstName = "John",
    LastName = "Doe",
    Email = "john.doe@example.com",
    DonationAmount = 100.00m,
    PayTransactionFee = true,
    PayTransactionFeeAmount = 3.20m,
    PaymentToken = "payment_token_123",
    ReferralCode = "AFFILIATE123"
};

var donation = await donationService.ProcessDonationAsync(donationRequest);
```

### Recurring Donation Setup
```csharp
var recurringRequest = new DonationRequestDto
{
    // ... basic donation fields
    IsMonthly = true,
    PaymentToken = "stored_payment_token"
};

var recurringDonation = await donationService.ProcessDonationAsync(recurringRequest);
// Automatically sets up NextProcessDate and RecurringPaymentToken
```

### User Donation History with Pagination
```csharp
var paginationParams = new PaginationParameters
{
    PageNumber = 1,
    PageSize = 10,
    FilterType = "recurring",
    SearchTerm = "campaign"
};

var donationHistory = await donationService.GetPagedByUserEmailAsync(
    "user@example.com", 
    paginationParams
);
```

### Recurring Donation Management
```csharp
// Update recurring donation amount
await donationService.UpdateRecurringDonationAsync(
    "user@example.com", 
    donationId, 
    150.00m, 
    true
);

// Cancel recurring donation
await donationService.CancelRecurringDonationAsync(
    "user@example.com", 
    donationId
);

// Update payment method
await donationService.UpdateRecurringPaymentMethodAsync(
    "user@example.com", 
    donationId, 
    "new_payment_token"
);
```

### Campaign and Referral Analytics
```csharp
// Get total donations for a campaign
var campaignTotal = await donationService.GetTotalAmountByCampaignIdAsync(campaignId);

// Get donations by referral code with pagination
var referralDonations = await donationService.GetPagedByReferralCodeAsync(
    "AFFILIATE123", 
    paginationParams
);

// Get donor's lifetime donation total
var donorTotal = await donationService.GetTotalAmountByDonorIdAsync(donorId);
```

## Files

### Core Service Files
- `Services/DonationService.cs` - Main donation processing service with comprehensive business logic
- `Services/RecurringDonationProcessingService.cs` - Background service for automated recurring payments

### Data Access Layer
- `Repositories/DonationRepository.cs` - Advanced repository with pagination and search capabilities
- `Interfaces/IDonationRepository.cs` - Repository contract with specialized query methods
- `Data/DonationDbContext.cs` - Entity Framework configuration with relationships

### Service Interface and Models
- `Interfaces/IDonationService.cs` - Comprehensive service contract with all operations
- `Models/Donation.cs` - Core entity with full relationship mapping
- `Models/DonationRequestDto.cs` - Client request structure with validation

### Configuration and Extensions
- `Extensions/DonationServiceExtensions.cs` - Dependency injection configuration for background services

### Integration Dependencies
- **External Services**: PaymentService, DonorService, UserService, ProfileService, MessageService
- **Background Processing**: RecurringDonationProcessingService registered as HostedService
- **Database Integration**: Partial ApplicationDbContext with complete entity configuration

## Security and Performance Considerations

### Security Features
- **Self-referral Prevention**: Automatic detection and silent blocking of self-referrals
- **Payment Token Security**: Secure storage and handling of payment method tokens
- **User Verification**: Email-based user verification before processing
- **Audit Compliance**: Complete audit trail for all donation activities

### Performance Optimizations
- **Efficient Queries**: Optimized database queries with proper indexing
- **Pagination Support**: Scalable data retrieval for large datasets
- **Background Processing**: Asynchronous recurring donation processing
- **Connection Management**: Proper DbContext lifecycle management with factory pattern

### Error Handling
- **Payment Failures**: Graceful handling of payment processor errors
- **Email Failures**: Non-blocking email delivery with error logging
- **Recurring Processing**: Retry logic and error tracking for automated payments
- **Validation Errors**: Comprehensive input validation with detailed error messages

This feature represents the core financial processing capability of the MSIH Platform for Good, handling millions of dollars in donations with high reliability, security, and performance requirements.