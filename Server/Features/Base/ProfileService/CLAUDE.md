# ProfileService

## Overview
The ProfileService manages user profile information including personal details, contact preferences, and referral codes. It serves as the central service for managing user profiles with comprehensive contact information, communication consent tracking, and referral system integration. This service provides full CRUD operations with specialized queries for user data retrieval and pagination.

## Architecture

### Components
- **IProfileService**: Main service interface for profile operations
- **ProfileService**: Core service implementation handling business logic
- **IProfileRepository**: Data access interface for profile entities
- **ProfileRepository**: Repository implementation with specialized queries
- **Profile**: Main entity representing user profiles
- **AddressModel**: Value object for address information

### Dependencies
- Entity Framework Core for data persistence
- ApplicationDbContext for database operations
- GenericRepository for base CRUD operations
- UserService integration for user relationship
- RandomStringGenerator for referral code generation

## Key Features
- Complete profile CRUD operations with audit trails
- Unique referral code generation and management
- Communication consent tracking (email, SMS, mail)
- Address management as owned entity type
- User relationship navigation with eager loading
- Paginated queries with search functionality
- Active/inactive status management
- Full-text search across profile fields

## Database Schema

### Profile Entity
- **Id** (int): Primary key, auto-generated
- **UserId** (int): Foreign key to User entity
- **User** (navigation): Navigation property to User entity
- **DateOfBirth** (DateTime?): Optional birth date
- **FirstName** (string?): User's first name
- **LastName** (string?): User's last name
- **FullName** (computed): Computed property combining first and last name
- **Address** (AddressModel?): Owned entity for address information
- **MobileNumber** (string?): Phone number with validation
- **ConsentReceiveText** (bool): SMS consent flag (default: false)
- **UnsubscribeMobile** (bool): SMS unsubscribe flag (default: false)
- **ConsentReceiveEmail** (bool): Email consent flag (default: true)
- **UnsubscribeEmail** (bool): Email unsubscribe flag (default: false)
- **ConsentReceiveMail** (bool): Physical mail consent flag (default: false)
- **UnsubscribeMail** (bool): Mail unsubscribe flag (default: false)
- **ReferralCode** (string): Unique referral code (max 100 chars)

### AddressModel (Owned Entity)
- **Street** (string): Street address (max 100 chars)
- **City** (string): City name (max 100 chars)
- **State** (string): State/province (max 100 chars)
- **PostalCode** (string): ZIP/postal code (max 20 chars)
- **Country** (string?): Optional country name (max 100 chars)

### Database Constraints
- Unique index on ReferralCode
- Phone number validation on MobileNumber
- Required referral code with automatic generation

## Usage

### Creating a Profile
```csharp
@inject IProfileService ProfileService

var profile = new Profile
{
    UserId = 123,
    FirstName = "John",
    LastName = "Doe",
    DateOfBirth = new DateTime(1990, 1, 1),
    MobileNumber = "+1-555-123-4567",
    Address = new AddressModel
    {
        Street = "123 Main St",
        City = "Anytown",
        State = "CA",
        PostalCode = "12345",
        Country = "USA"
    },
    ConsentReceiveEmail = true,
    ConsentReceiveText = false
};

var createdProfile = await ProfileService.AddAsync(profile, "AdminUser", true);
// ReferralCode is automatically generated
```

### Retrieving Profiles
```csharp
// Get all profiles with user data
var allProfiles = await ProfileService.GetAllWithUserDataAsync();

// Get paginated profiles with search
var paginationParams = new PaginationParameters
{
    PageNumber = 1,
    PageSize = 20,
    SearchTerm = "john"
};
var pagedResult = await ProfileService.GetPaginatedWithUserDataAsync(paginationParams);

// Find by referral code
var profile = await ProfileService.GetByReferralCodeAsync("ABC123");
```

### Updating Profiles
```csharp
var profile = await ProfileService.GetByIdAsync(123);
profile.FirstName = "Jane";
profile.ConsentReceiveText = true;

var updatedProfile = await ProfileService.UpdateAsync(profile, "ModifierUser");
```

### Managing Active Status
```csharp
// Deactivate a profile
await ProfileService.SetActiveAsync(123, false, "AdminUser");

// Reactivate a profile
await ProfileService.SetActiveAsync(123, true, "AdminUser");
```

## Integration

### UserService Integration
- Strong relationship with User entity via foreign key
- Navigation property allows access to user authentication data
- Profile creation typically follows user registration

### Referral System Integration
- Automatic generation of unique referral codes
- Integration with campaign and donation tracking
- Referral code search functionality for affiliate tracking

### Communication Services Integration
- Consent flags control messaging permissions
- Email service respects ConsentReceiveEmail flag
- SMS service checks ConsentReceiveText flag
- Mail service uses ConsentReceiveMail flag

### Address Management
- Address stored as owned entity type in EF Core
- Full address validation and formatting
- Optional country field for international users

## Usage Examples

### Search Functionality
The service supports comprehensive search across multiple fields:
```csharp
// Searches across: FirstName, LastName, ReferralCode, MobileNumber, User.Email
var searchParams = new PaginationParameters
{
    PageNumber = 1,
    PageSize = 10,
    SearchTerm = "search query"
};
var results = await ProfileService.GetPaginatedWithUserDataAsync(searchParams);
```

### Consent Management
```csharp
// Update communication preferences
var profile = await ProfileService.GetByIdAsync(userId);
profile.ConsentReceiveEmail = true;
profile.ConsentReceiveText = false;
profile.UnsubscribeEmail = false;
await ProfileService.UpdateAsync(profile, "UserUpdate");
```

## Files

```
Server/Features/Base/ProfileService/
├── Data/
│   └── ProfileDbContext.cs
├── Interfaces/
│   ├── IProfileRepository.cs
│   └── IProfileService.cs
├── Model/
│   ├── AddressModel.cs
│   └── Profile.cs
├── Repositories/
│   └── ProfileRepository.cs
├── Services/
│   └── ProfileService.cs
└── CLAUDE.md
```