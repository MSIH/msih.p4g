# FundraiserService Feature - Complete Fundraiser Management

## Overview
The FundraiserService feature provides comprehensive management for fundraisers/affiliates in the MSIH Platform for Good. This feature handles fundraiser profiles, payout account management, suspension handling, performance statistics, and commission tracking. It integrates closely with the donation system to track referral-based fundraising activities.

## Architecture

### Core Components
- **FundraiserService**: Main service for fundraiser operations and business logic
- **FundraiserStatisticsService**: Service for generating fundraiser performance statistics and analytics
- **FundraiserRepository**: Data access layer for fundraiser entities with user relationship management
- **FundraiserStatisticsRepository**: Data access layer for aggregated fundraiser performance data

### Data Models
- **Fundraiser**: Core entity representing fundraiser profiles with payout information and suspension handling
- **FundraiserStatistics**: Performance analytics model with donation metrics and statistics
- **DonationInfo**: Detailed donation information for fundraiser tracking
- **FirstTimeDonorInfo**: First-time donor tracking for commission calculations

### Database Integration
- Uses ApplicationDbContext with partial class pattern
- Fundraiser entity configuration in `ApplicationDbContext.Fundraiser.cs`
- Relationships with User entities for comprehensive profile management
- Support for soft deletes and audit trail tracking

## Key Features

### Fundraiser Profile Management
- Complete fundraiser registration and profile setup
- PayPal and Venmo payout account configuration with multiple format support
- W9 tax document storage and management
- User relationship linking for comprehensive profile data

### Account Status Management
- Fundraiser account suspension with detailed reason tracking
- Suspension date logging and audit trail
- Account reactivation capabilities
- Status-based access control and filtering

### Performance Statistics & Analytics
- Real-time donation count tracking by fundraiser
- Total amount raised calculations with referral attribution
- Average donation amount analytics
- Detailed donation history with donor information
- First-time donor identification and tracking

### Payout Account Management
- Multiple account type support (PayPal, Venmo)
- Flexible account format handling (Email, Mobile, Handle)
- Secure payout account information storage
- Integration with PayoutService for commission processing

### Administrative Features
- Paginated fundraiser listing with search capabilities
- Bulk operations for fundraiser management
- Advanced filtering by status, suspension state, account type
- Comprehensive audit logging for all fundraiser operations

## Database Schema

### Fundraiser Entity
```sql
- Id: Primary key (inherited from BaseEntity)
- PayoutAccount: VARCHAR(200) - Account identifier for payouts
- PayoutAccountType: ENUM(PayPal, Venmo) - Type of payout account
- PayoutAccountFormat: ENUM(Email, Mobile, Handle) - Format of account identifier
- W9Document: VARCHAR(500) - Path to W9 tax document
- IsSuspended: BOOLEAN - Account suspension status
- SuspensionReason: VARCHAR(500) - Detailed reason for suspension
- SuspendedDate: DATETIME - Timestamp of suspension
- UserId: INT - Foreign key to User entity
- Standard audit fields: CreatedOn, CreatedBy, ModifiedOn, ModifiedBy, IsActive
```

### FundraiserStatistics Model
```sql
- DonationCount: INT - Total number of donations received
- TotalRaised: DECIMAL - Total amount raised through referrals
- AverageDonation: DECIMAL - Average donation amount
- Donations: List<DonationInfo> - Detailed donation records
```

## Service Integration

### User Service Integration
- Seamless integration with UserService for profile data
- User relationship management through navigation properties
- Coordinated user and fundraiser lifecycle management

### Donation Service Integration
- Real-time donation tracking by referral code
- Commission calculation based on donation amounts
- Recurring donation impact on fundraiser statistics

### Payout Service Integration
- Automated payout processing to configured accounts
- Commission distribution based on fundraiser performance
- Payment transaction tracking and reconciliation

### Message Service Integration
- Automated notifications for account status changes
- Commission payment confirmations
- Performance milestone celebrations

## Usage Examples

### Basic Fundraiser Operations
```csharp
// Get fundraiser by ID
var fundraiser = await _fundraiserService.GetByIdAsync(fundraiserId);

// Get fundraiser by user ID
var userFundraiser = await _fundraiserService.GetByUserIdAsync(userId);

// Create new fundraiser
var newFundraiser = new Fundraiser
{
    PayoutAccount = "fundraiser@example.com",
    PayoutAccountType = AccountType.PayPal,
    PayoutAccountFormat = AccountFormat.Email,
    UserId = userId
};
await _fundraiserService.AddAsync(newFundraiser);

// Update fundraiser information
fundraiser.PayoutAccount = "new-account@example.com";
await _fundraiserService.UpdateAsync(fundraiser);

// Suspend fundraiser account
await _fundraiserService.SetActiveAsync(fundraiserId, false, "AdminUser");
```

### Statistics and Analytics Operations
```csharp
// Get comprehensive fundraiser statistics
var stats = await _fundraiserStatisticsService.GetStatisticsAsync(fundraiserId);

// Get first-time donors brought by fundraiser
var firstTimeDonors = await _fundraiserStatisticsService.GetFirstTimeDonorsAsync(fundraiserId);

// Get paginated fundraiser list with user data
var paginatedResult = await _fundraiserService.GetPaginatedWithUserDataAsync(paginationParameters);
```

### Integration with Other Services
```csharp
// Check if user is a fundraiser (used by other services)
var fundraiser = await _fundraiserService.GetByUserIdAsync(userId);
var isFundraiser = fundraiser != null && fundraiser.IsActive && !fundraiser.IsSuspended;

// Process donations with fundraiser attribution
if (!string.IsNullOrEmpty(referralCode))
{
    var referringFundraiser = await _fundraiserService.GetByUserIdAsync(referralUserId);
    // Process commission and update statistics
}
```

## Client Integration

### Admin Management Interface
- FundraiserManager.razor provides comprehensive admin interface
- Real-time editing of fundraiser information
- Suspension management with detailed confirmation dialogs
- Search and filtering capabilities across all fundraiser data

### Fundraiser Dashboard
- MyFundraiser.razor for fundraiser self-service
- AffiliateComissions.razor for commission tracking
- Integration with donation statistics and performance metrics

## Files

### Server Components
```
Server/Features/FundraiserService/
├── Data/
│   └── FundraiserDbContext.cs
├── Interfaces/
│   ├── IFundraiserRepository.cs
│   ├── IFundraiserService.cs
│   ├── IFundraiserStatisticsRepository.cs
│   └── IFundraiserStatisticsService.cs
├── Model/
│   ├── Fundraiser.cs
│   └── FundraiserStatistics.cs
├── Repositories/
│   ├── FundraiserRepository.cs
│   └── FundraiserStatisticsRepository.cs
├── Services/
│   ├── FundraiserService.cs
│   └── FundraiserStatisticsService.cs
└── CLAUDE.md
```

### Related Components
```
Server/Common/Data/ApplicationDbContext.Fundraiser.cs
Client/Features/FundraiserManagement/ (UI components)
Client/Features/Admin/Pages/FundraiserManager.razor
Client/Common/Components/AffiliateCommissionTable.razor
Client/Common/Components/ReferralLinkComponent.razor
```

## Testing and Quality Assurance

### Key Testing Areas
- Fundraiser CRUD operations with proper validation
- Suspension and reactivation workflows
- Statistics calculation accuracy
- Payout account validation and security
- Integration with donation and user services
- Performance with large datasets

### Security Considerations
- Secure storage of payout account information
- Access control for fundraiser management operations
- Audit trail for all administrative actions
- Data protection for sensitive fundraiser information
- Proper validation of suspension reasons and documentation

This feature serves as the foundation for the affiliate/referral system in the MSIH Platform for Good, enabling comprehensive fundraiser management while maintaining security and providing detailed analytics for performance tracking.