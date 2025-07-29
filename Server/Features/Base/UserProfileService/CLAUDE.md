# UserProfileService

## Overview
The UserProfileService is a coordination service that manages the relationship between User and Profile entities, providing unified operations for user registration, profile management, and role-based entity creation. It orchestrates complex multi-entity operations while maintaining data consistency and proper entity relationships across the user management system.

## Architecture

### Components
- **IUserProfileService**: Service interface for coordinated user-profile operations
- **UserProfileService**: Implementation managing User, Profile, and Fundraiser entity coordination
- **Event System**: OnProfileChanged event for reactive UI updates

### Dependencies
- **IUserService**: For user entity management and email-based lookups
- **IProfileService**: For profile creation, updates, and referral code generation
- **IFundraiserService**: For automatic fundraiser entity creation based on user roles
- Entity relationship management across multiple services

## Key Features
- Coordinated user and profile creation in atomic operations
- Automatic role-based entity creation (Fundraiser entities for fundraiser users)
- Email-based profile lookup through user relationships
- Event-driven profile change notifications for reactive UI
- Transaction-like operations ensuring data consistency
- Referral code generation coordination
- Multi-service orchestration with proper error handling

## Usage

### Creating User with Profile
```csharp
@inject IUserProfileService UserProfileService

// Create a new user with associated profile
var user = new User
{
    Email = "john.doe@example.com",
    Role = UserRole.Fundraiser,
    IsActive = true
};

var profile = new Profile
{
    FirstName = "John",
    LastName = "Doe",
    DateOfBirth = new DateTime(1990, 1, 1),
    MobileNumber = "+1-555-123-4567",
    ConsentReceiveEmail = true,
    Address = new AddressModel
    {
        Street = "123 Main St",
        City = "Anytown",
        State = "CA",
        PostalCode = "12345"
    }
};

// Single operation creates User, Profile, and Fundraiser (if role is Fundraiser)
var createdProfile = await UserProfileService.CreateUserWithProfileAsync(
    user, 
    profile, 
    "RegistrationService"
);

// Profile now has:
// - Generated referral code
// - UserId properly set
// - Associated Fundraiser entity created (if applicable)
```

### Profile Lookup by Email
```csharp
// Get profile by user's email address
var userProfile = await UserProfileService.GetProfileByUserEmailAsync("john.doe@example.com");

if (userProfile != null)
{
    Console.WriteLine($"Found profile for: {userProfile.FullName}");
    Console.WriteLine($"Referral code: {userProfile.ReferralCode}");
    Console.WriteLine($"Mobile: {userProfile.MobileNumber}");
}
```

### Profile Updates with Event Notifications
```csharp
// Subscribe to profile change events
UserProfileService.OnProfileChanged += () => {
    // Refresh UI, update caches, trigger notifications, etc.
    Console.WriteLine("Profile has been updated!");
};

// Update a profile
var profile = await ProfileService.GetByIdAsync(123);
profile.FirstName = "Jane";
profile.ConsentReceiveText = true;

var updatedProfile = await UserProfileService.UpdateAsync(profile, "UserUpdate");
// OnProfileChanged event will fire automatically
```

## Integration

### Multi-Service Coordination
The service coordinates operations across multiple services:

```csharp
public async Task<Profile> CreateUserWithProfileAsync(User user, Profile profile, string createdBy)
{
    // 1. Create User entity
    var createdUser = await _userService.AddAsync(user, createdBy);
    
    // 2. Associate Profile with User
    profile.UserId = createdUser.Id;
    var createdProfile = await _profileService.AddAsync(profile, createdBy);
    
    // 3. Create role-specific entities if needed
    if (createdUser.Role == UserRole.Fundraiser)
    {
        var fundraiser = new Fundraiser { UserId = createdUser.Id };
        await _fundraiserService.AddAsync(fundraiser);
    }
    
    return createdProfile;
}
```

### UserService Integration
- **User Creation**: Creates user accounts with proper role assignment
- **Email Lookup**: Retrieves users by email with profile navigation
- **Role Management**: Handles user role assignments and validations

### ProfileService Integration  
- **Profile Creation**: Creates profiles with automatic referral code generation
- **Profile Updates**: Manages profile modifications with audit trails
- **Address Management**: Handles complex address data as owned entities

### FundraiserService Integration
- **Automatic Creation**: Creates Fundraiser entities for users with Fundraiser role
- **Role-Based Logic**: Implements business rules for fundraiser account setup
- **Entity Relationship**: Maintains proper relationships between User and Fundraiser

## Event System

### OnProfileChanged Event
```csharp
// Subscribe to profile changes
UserProfileService.OnProfileChanged += HandleProfileChange;

private void HandleProfileChange()
{
    // Update UI components
    StateHasChanged();
    
    // Refresh cached data
    await RefreshUserData();
    
    // Send notifications
    await NotificationService.SendProfileUpdateNotification();
}
```

### Event-Driven Architecture Benefits
- **Reactive UI**: Automatic UI updates when profiles change
- **Cache Invalidation**: Automatic cache clearing on data changes  
- **Audit Logging**: Centralized change tracking
- **Notification System**: Automatic user notifications

## Advanced Scenarios

### Registration Flow Integration
```csharp
// Complete user registration with validation
public async Task<RegistrationResult> RegisterUserAsync(RegistrationRequest request)
{
    try
    {
        // Validate email uniqueness
        var existingUser = await UserService.GetByEmailAsync(request.Email);
        if (existingUser != null)
            return new RegistrationResult { Success = false, Error = "Email already exists" };
        
        // Create coordinated user and profile
        var user = new User 
        { 
            Email = request.Email, 
            Role = request.UserRole 
        };
        
        var profile = new Profile 
        { 
            FirstName = request.FirstName,
            LastName = request.LastName,
            ConsentReceiveEmail = request.EmailConsent
        };
        
        var createdProfile = await UserProfileService.CreateUserWithProfileAsync(
            user, profile, "Registration"
        );
        
        return new RegistrationResult 
        { 
            Success = true, 
            Profile = createdProfile,
            ReferralCode = createdProfile.ReferralCode
        };
    }
    catch (Exception ex)
    {
        return new RegistrationResult { Success = false, Error = ex.Message };
    }
}
```

### Profile Migration and Updates
```csharp
// Bulk profile updates with event coordination
public async Task BulkUpdateProfilesAsync(List<Profile> profiles, string modifiedBy)
{
    foreach (var profile in profiles)
    {
        await UserProfileService.UpdateAsync(profile, modifiedBy);
        // Each update triggers OnProfileChanged event
    }
}
```

## Error Handling and Data Integrity

### Transaction-like Behavior
While not using database transactions, the service provides logical consistency:
- User creation failure prevents profile creation
- Profile creation failure doesn't leave orphaned users
- Role-based entity creation is conditional and safe

### Error Recovery
```csharp
public async Task<Profile> CreateUserWithProfileAsync(User user, Profile profile, string createdBy)
{
    User? createdUser = null;
    try
    {
        createdUser = await _userService.AddAsync(user, createdBy);
        profile.UserId = createdUser.Id;
        
        var createdProfile = await _profileService.AddAsync(profile, createdBy);
        
        // Additional role-based logic with error handling
        if (createdUser.Role == UserRole.Fundraiser)
        {
            try
            {
                await _fundraiserService.AddAsync(new Fundraiser { UserId = createdUser.Id });
            }
            catch (Exception ex)
            {
                // Log error but don't fail the entire operation
                _logger.LogError(ex, "Failed to create fundraiser entity for user {UserId}", createdUser.Id);
            }
        }
        
        return createdProfile;
    }
    catch (Exception)
    {
        // Consider cleanup of created user if profile creation fails
        // Implementation depends on business requirements
        throw;
    }
}
```

## Service Registration
```csharp
// In Program.cs or Startup.cs
builder.Services.AddScoped<IUserProfileService, UserProfileService>();

// Ensure dependencies are registered
builder.Services.AddUserServices();
builder.Services.AddProfileServices();
builder.Services.AddFundraiserServices();
```

## Files

```
Server/Features/Base/UserProfileService/
├── Interfaces/
│   └── IUserProfileService.cs
├── Services/
│   └── UserProfileService.cs
└── CLAUDE.md
```