# DonorService

## Overview
The DonorService is a domain service that manages donor profiles and tracking within the MSIH Platform for Good. This feature handles donor registration, profile management, referral code tracking, and donor lifecycle operations. It serves as the central hub for all donor-related functionality, linking users to their donation activities and affiliate relationships.

## Architecture

### Components
- **Service**: `DonorService` - Core donor management business logic
- **Interface**: `IDonorService` - Service contract
- **Repository**: `DonorRepository` - Data access layer
- **Interface**: `IDonorRepository` - Repository contract
- **Model**: `Donor` - Core donor entity
- **Data Context**: `DonorDbContext` - Entity Framework context

### Dependencies
- `ApplicationDbContext` - Shared database context
- `IGenericRepository<Donor>` - Base repository functionality
- `UserService` - User profile integration
- `DonationService` - Donation tracking relationship
- Entity Framework Core for data persistence

## Key Features

- **Donor Profile Management**: Create, update, and manage donor profiles
- **User Integration**: Links donors to user accounts and profiles
- **Referral Code Tracking**: Manages affiliate referral relationships
- **Payment Processor Integration**: Stores external payment system donor IDs
- **Donation Relationship**: Maintains connections to donation history
- **Search and Filtering**: Advanced donor search capabilities
- **Pagination Support**: Efficient handling of large donor lists
- **Active Status Management**: Enable/disable donor accounts
- **Audit Trail**: Comprehensive tracking of all donor changes

## Database Schema

### Donor Entity
```sql
Donors (
    Id                      INT PRIMARY KEY IDENTITY,
    UserId                  INT NOT NULL,
    PaymentProcessorDonorId NVARCHAR(MAX),
    ReferralCode           NVARCHAR(100),
    IsActive               BIT DEFAULT 1,
    CreatedBy              NVARCHAR(255),
    CreatedOn              DATETIME2,
    ModifiedBy             NVARCHAR(255),
    ModifiedOn             DATETIME2,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
)
```

### Relationships
- **One-to-One**: Donor ↔ User (each donor linked to one user account)
- **One-to-Many**: Donor → Donations (donor can have multiple donations)
- **Many-to-One**: Donor → Fundraiser (via ReferralCode for affiliate tracking)

## Business Logic

### Donor Registration Workflow
1. User creates account through registration
2. Donor profile automatically created and linked to User
3. Referral code captured if provided during registration
4. Payment processor donor ID assigned during first donation
5. Audit trail established for all subsequent changes

### Referral Code Processing
- Validates referral codes against active fundraisers
- Links donor to referring affiliate for commission tracking
- Maintains referral relationship throughout donor lifecycle
- Integrates with AffiliateMonitoringService for compliance

### Search and Discovery
- Full-text search across donor profiles
- Filter by active status, referral codes, and date ranges
- Pagination for efficient handling of large donor datasets
- Integration with user data for comprehensive donor views

## Usage

### Service Registration
```csharp
// In Program.cs or service extensions
builder.Services.AddScoped<IDonorService, DonorService>();
builder.Services.AddScoped<IDonorRepository, DonorRepository>();
```

### Creating a New Donor
```csharp
public async Task<Donor> CreateDonorAsync(int userId, string? referralCode = null)
{
    var donor = new Donor
    {
        UserId = userId,
        ReferralCode = referralCode,
        IsActive = true
    };
    
    return await _donorService.AddAsync(donor);
}
```

### Searching Donors
```csharp
public async Task<PagedResult<Donor>> SearchDonorsAsync(
    string searchTerm, 
    PaginationParameters pagination)
{
    return await _donorService.GetPaginatedWithUserDataAsync(pagination);
}
```

### Managing Donor Status
```csharp
public async Task<bool> DeactivateDonorAsync(int donorId, string reason)
{
    return await _donorService.SetActiveAsync(donorId, false, reason);
}
```

## Integration Points

### With UserService
- Links donor profiles to user accounts
- Maintains referential integrity between users and donors
- Provides comprehensive user-donor data views

### With DonationService
- Tracks donation history for each donor
- Calculates donor lifetime value and giving patterns
- Supports recurring donation relationships

### With FundraiserService
- Processes affiliate referral codes
- Links donors to referring fundraisers for commission tracking
- Supports affiliate performance analytics

### With PaymentService
- Stores payment processor donor identifiers
- Facilitates payment processing for donor contributions
- Maintains payment method associations

### With AffiliateMonitoringService
- Provides donor data for affiliate compliance checking
- Supports suspension workflows based on donor activity patterns
- Tracks unqualified donor accounts

## Advanced Features

### Pagination and Performance
- Efficient pagination with `PagedResult<T>` pattern
- Optimized queries with user data joins
- Configurable page sizes and sorting options

### Data Integrity
- Referential integrity with Users table
- Cascade operations for related donation data
- Audit trail for all donor modifications

### Extensibility
- Generic repository pattern for common operations
- Interface-based design for testability
- Separation of concerns between service and repository layers

## Files

```
Server/Features/DonorService/
├── Data/
│   └── DonorDbContext.cs
├── Interfaces/
│   ├── IDonorRepository.cs
│   └── IDonorService.cs
├── Model/
│   └── Donor.cs
├── Repositories/
│   └── DonorRepository.cs
├── Services/
│   └── DonorService.cs
└── CLAUDE.md
```