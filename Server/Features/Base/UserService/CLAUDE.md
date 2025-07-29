# UserService

## Overview
The UserService provides comprehensive user management functionality including authentication, role management, email verification, and user lifecycle operations. It serves as the foundation for user identity management across the platform, integrating with Profile, Donor, and Fundraiser entities while providing flexible eager loading capabilities and secure email verification workflows.

## Architecture

### Components
- **IUserService**: Main service interface for user operations
- **UserService**: Core service implementation handling business logic
- **IUserRepository**: Repository interface for user data access
- **UserRepository**: Repository with specialized queries and eager loading
- **IEmailVerificationService**: Interface for email verification workflows
- **EmailVerificationService**: Implementation handling verification tokens and emails
- **User**: Main entity representing system users
- **UserRole**: Enum defining user types (Donor, Fundraiser, Admin)

### Dependencies
- Entity Framework Core for data persistence
- ApplicationDbContext for database operations
- MessageService for email communication
- SettingsService for configuration management
- ProfileService integration for user profiles
- RandomStringGenerator for secure token generation

## Key Features
- Comprehensive user CRUD operations with audit trails
- Flexible eager loading for related entities (Profile, Donor, Fundraiser, Address)
- Email-based user lookup and authentication
- Referral code-based user discovery
- Secure email verification with token-based authentication
- Role-based user management (Donor, Fundraiser, Admin)
- User logout and session management
- Email verification token lifecycle management

## Database Schema

### User Entity
- **Id** (int): Primary key, auto-generated
- **Email** (string): Unique email address (required, email validation)
- **Role** (UserRole): User role enum (Donor, Fundraiser, Admin)
- **Profile** (navigation): One-to-one relationship with Profile entity
- **Donor** (navigation): One-to-one relationship with Donor entity
- **Fundraiser** (navigation): One-to-one relationship with Fundraiser entity
- **EmailConfirmed** (bool): Email verification status (default: false)
- **EmailConfirmedAt** (DateTime?): Timestamp when email was verified
- **LastEmailVerificationSentAt** (DateTime?): Last verification email sent timestamp
- **EmailVerificationToken** (string?): Current verification token
- **IsActive** (bool): Soft delete flag from BaseEntity
- **CreatedOn** (DateTime): Creation timestamp from BaseEntity
- **CreatedBy** (string): Creator identifier from BaseEntity
- **ModifiedOn** (DateTime?): Last modification timestamp from BaseEntity
- **ModifiedBy** (string?): Last modifier identifier from BaseEntity

### UserRole Enum
- **Donor**: Users who make donations
- **Fundraiser**: Users who run fundraising campaigns
- **Admin**: Users with administrative privileges

### Entity Relationships
- **User → Profile**: One-to-one, Profile.UserId foreign key
- **User → Donor**: One-to-one, Donor.UserId foreign key
- **User → Fundraiser**: One-to-one, Fundraiser.UserId foreign key

## Usage

### Basic User Operations
```csharp
@inject IUserService UserService

// Create a new user
var user = new User
{
    Email = "john.doe@example.com",
    Role = UserRole.Fundraiser
};
var createdUser = await UserService.AddAsync(user, "RegistrationService");

// Get user by email
var user = await UserService.GetByEmailAsync("john.doe@example.com");

// Get user by ID
var user = await UserService.GetByIdAsync(123);

// Update user
user.Role = UserRole.Admin;
await UserService.UpdateAsync(user, "AdminService");

// Deactivate user
await UserService.SetActiveAsync(123, false, "AdminService");
```

### Advanced User Queries with Eager Loading
```csharp
// Get user with profile information
var userWithProfile = await UserService.GetByEmailAsync(
    email: "john.doe@example.com",
    includeProfile: true
);

// Get user with profile and address
var userWithAddress = await UserService.GetByEmailAsync(
    email: "john.doe@example.com",
    includeProfile: true,
    includeAddress: true
);

// Get user with all related entities
var fullUserData = await UserService.GetByEmailAsync(
    email: "john.doe@example.com",
    includeProfile: true,
    includeAddress: true,
    includeDonor: true,
    includeFundraiser: true
);

Console.WriteLine($"User: {fullUserData.Email}");
Console.WriteLine($"Profile: {fullUserData.Profile?.FullName}");
Console.WriteLine($"Address: {fullUserData.Profile?.Address?.City}");
if (fullUserData.Role == UserRole.Fundraiser)
{
    Console.WriteLine($"Fundraiser ID: {fullUserData.Fundraiser?.Id}");
}
```

### Referral Code-Based Lookup
```csharp
// Find user by their profile's referral code
var user = await UserService.GetByReferralCodeAsync(
    referralCode: "ABC123",
    includeProfile: true,
    includeFundraiser: true
);

if (user != null)
{
    Console.WriteLine($"Found user: {user.Profile.FullName}");
    Console.WriteLine($"Role: {user.Role}");
}
```

### Email Verification Workflow
```csharp
@inject IEmailVerificationService EmailVerificationService

// Send verification email to user
var user = await UserService.GetByEmailAsync("john.doe@example.com", includeProfile: true);
var emailSent = await EmailVerificationService.SendVerificationEmailAsync(user);

if (emailSent)
{
    Console.WriteLine("Verification email sent successfully");
}

// Verify email using token (from email link or user input)
var verificationSuccess = await EmailVerificationService.VerifyEmailAsync("ABC12345");

if (verificationSuccess)
{
    Console.WriteLine("Email verified successfully");
}

// Check if user's email is verified
var isVerified = await EmailVerificationService.IsEmailVerifiedAsync(123);
```

### User Session Management
```csharp
// Get user by verification token
var user = await UserService.GetUserByTokenAsync("verification-token");

// Log out user (sets EmailConfirmed to false)
var logoutSuccess = await UserService.LogOutUserByIdAsync(123);
```

## Integration

### ProfileService Integration
- **Automatic Profile Association**: Users are linked to Profile entities via UserId
- **Referral Code Lookup**: Users can be found by their profile's referral code
- **Address Information**: User queries can include profile address data
- **Communication Preferences**: Profile consent flags control email delivery

### DonorService Integration
- **Donor Entity Creation**: Donor users have associated Donor records
- **Donation History**: User queries can include donor relationship data
- **Payment Information**: Donor records store payment and billing details

### FundraiserService Integration
- **Fundraiser Entity Creation**: Fundraiser users have associated Fundraiser records
- **Campaign Management**: Fundraiser records link to campaigns and payouts
- **Performance Metrics**: Fundraiser data includes campaign statistics

### MessageService Integration
- **Email Verification**: Uses MessageService for sending verification emails
- **Template Integration**: Supports HTML email templates with placeholders
- **Referral Links**: Includes personalized referral URLs in emails

### SettingsService Integration
- **Configuration Management**: Retrieves base URLs and email settings
- **Environment Flexibility**: Falls back to appsettings.json and environment variables
- **Token Expiration**: Configurable verification token expiration (default: 90 days)

## Email Verification System

### Verification Token Generation
```csharp
// Tokens are generated using user ID + timestamp for uniqueness
var currentTime = DateTime.UtcNow.ToString("HHmmss");
var currentTimeInt = int.Parse(currentTime);
var token = RandomStringGenerator.Generate(user.Id + currentTimeInt, 8, CharSet.All);
```

### Verification Email Template
The service includes a comprehensive HTML email template with:
- Personalized greeting with user's full name
- Clickable verification link
- Manual token entry option
- Personalized referral URL for sharing
- Professional branding and styling

### Token Security Features
- **Expiration**: Tokens expire after 90 days (configurable)
- **Uniqueness**: Generated using user ID and timestamp
- **Secure Random**: Uses cryptographically secure random generation
- **One-time Use**: Tokens are invalidated after successful verification

## Administrative Features

### User Role Management
```csharp
// Change user role
var user = await UserService.GetByIdAsync(123);
user.ChangeRole(UserRole.Admin);
await UserService.UpdateAsync(user, "AdminService");
```

### Bulk User Operations
```csharp
// Get all users (including inactive)
var allUsers = await UserService.GetAllAsync(includeInactive: true);

// Process users by role
var fundraisers = allUsers.Where(u => u.Role == UserRole.Fundraiser);
var donors = allUsers.Where(u => u.Role == UserRole.Donor);
var admins = allUsers.Where(u => u.Role == UserRole.Admin);
```

## Files

```
Server/Features/Base/UserService/
├── Data/
│   └── UserDbContext.cs
├── Interfaces/
│   ├── IEmailVerificationService.cs
│   ├── IUserRepository.cs
│   └── IUserService.cs
├── Models/
│   ├── User.cs
│   └── UserExtensions.cs
├── Repositories/
│   └── UserRepository.cs
├── Services/
│   ├── AdminInitializationService.cs
│   ├── EmailVerificationService.cs
│   └── UserService.cs
└── CLAUDE.md
```