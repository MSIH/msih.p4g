# UserManagement Feature - Complete User Profile and Registration Interface

## Overview
The UserManagement feature provides comprehensive user profile management, registration, and administrative interfaces within the MSIH Platform for Good. This feature handles user profile editing, new user registration, user administration, and communication preferences management. It serves as the central hub for user lifecycle management and self-service profile operations.

## Architecture

### Core Components
- **EditProfile Page**: Comprehensive user profile editing interface
- **RegisterUser Page**: New user registration with fundraiser capabilities
- **Users Page**: Administrative user management interface
- **Profile Integration**: Deep integration with profile and user services
- **Communication Preferences**: Complete preference management system

### UI Structure
- **Self-service Pages**: User-facing profile and registration interfaces
- **Administrative Pages**: Admin-only user management interfaces
- **Form-based Interfaces**: Comprehensive form handling with validation
- **Responsive Design**: Mobile-optimized user management interfaces

## Key Features

### Comprehensive Profile Management
- **EditProfile.razor**: Complete user profile editing interface featuring:
  - Personal information management (name, date of birth, contact details)
  - Complete address management with US state dropdown integration
  - Communication preference management (email, SMS, mail consent)
  - Referral link display and management for fundraisers
  - Mobile number management with validation
  - Date of birth selection with dropdown interface

### User Registration System
- **RegisterUser.razor**: Advanced user registration interface including:
  - Multi-route support (`/register`, `/student`, `/influencer`)
  - Email and personal information collection
  - Automatic email consent (required for platform participation)
  - Referral link generation and display post-registration
  - Email verification workflow integration
  - Affiliate commission table display for marketers

### Administrative User Management
- **Users.razor**: Comprehensive admin interface for user oversight
- Bulk user operations and management capabilities
- User account status management and administration
- Advanced user search and filtering functionality
- User role assignment and permission management

### Communication Preferences Management
- **Comprehensive Consent System**: Complete consent management including:
  - Email communication consent and unsubscribe options
  - SMS/text message consent and unsubscribe controls
  - Physical mail consent and unsubscribe management
  - Preference interdependencies (unsubscribe only available if consent given)
  - Visual feedback for preference states and availability

### Address Management System
- **Complete US Address Support**: Full address management featuring:
  - Street address with apartment/suite number support
  - City and postal code management
  - Complete US state dropdown with abbreviations and full names
  - Support for territories (DC, AS, GU, MP, PR, VI)
  - Country selection (currently focused on US)
  - Address validation and formatting

## UI Components

### EditProfile.razor Features

#### Personal Information Management
- **User Identification**: Display of user email (readonly) from authentication
- **Name Management**: First name and last name editing with validation
- **Mobile Number**: Phone number management with validation support
- **Date of Birth**: Comprehensive date selection using:
  - Month dropdown with full month names
  - Day dropdown (1-31) with proper validation
  - Year dropdown (current year to 120 years ago)
  - Automatic date validation and error handling

#### Address Management Interface
- **Street Address**: Complete street address input with suite/apartment support
- **Location Details**: City, state, and ZIP code management
- **State Selection**: Comprehensive US state dropdown including:
  - All 50 states with abbreviations and full names
  - District of Columbia support
  - US territories (American Samoa, Guam, Northern Mariana Islands, Puerto Rico, US Virgin Islands)
- **Country Support**: Country selection dropdown (currently US-focused)

#### Communication Preferences Interface
- **Email Preferences**:
  - Email consent checkbox (disabled - required for platform)
  - Email unsubscribe option (only enabled if consent given)
  - Visual feedback for preference availability
- **SMS Preferences**:
  - SMS consent checkbox with full user control
  - SMS unsubscribe option (only enabled if consent given)
  - Clear labeling for marketing text message preferences
- **Mail Preferences**:
  - Physical mail consent checkbox with user control
  - Mail unsubscribe option (only enabled if consent given)
  - Clear distinction between transactional and marketing mail

#### Fundraiser Integration
- **Referral Link Display**: For fundraisers, displays complete referral link management:
  - Referral URL generation and display
  - Copy-to-clipboard functionality
  - Error handling and user feedback
  - Integration with ReferralLinkComponent

#### Form Management
- **Save Functionality**: Comprehensive save process with loading states
- **Validation**: Real-time form validation with error display
- **Success Feedback**: Clear success messages and confirmation
- **Error Handling**: User-friendly error messages and recovery options

### RegisterUser.razor Features

#### Multi-route Registration Support
- **Flexible Routing**: Support for multiple registration entry points:
  - `/register` - General registration
  - `/student` - Student-specific registration
  - `/influencer` - Influencer-specific registration
- **Context-aware Interface**: Registration form adapts based on entry route

#### Registration Form Interface
- **Essential Information**: Core registration data collection including:
  - Email address with validation and uniqueness checking
  - First and last name collection
  - Required field validation and error display
- **Consent Management**: Email consent (required and pre-checked)
- **Processing States**: Loading indicators and processing feedback

#### Post-registration Experience
- **Success Confirmation**: Registration success display with clear messaging
- **Referral Link Display**: Immediate referral link generation and display
- **Email Verification**: Instructions for email verification process
- **Affiliate Features**: Commission table display for marketing-focused registrations

#### Specialized Features
- **Marketer Support**: Special interface elements for affiliate/marketer registrations
- **Commission Display**: AffiliateCommissionTable integration for marketers
- **Role-based Elements**: Different interface elements based on registration type

### Users.razor Features (Administrative)
- **User Listing**: Comprehensive user directory with search and filtering
- **Account Management**: User account status control and administration
- **Bulk Operations**: Mass user operations for administrative efficiency
- **Advanced Search**: User search across multiple criteria and fields
- **Role Management**: User role assignment and permission control

## Integration Points

### Server Service Integration
- **UserProfileService**: Complete profile management and data persistence
- **AuthService**: User authentication and session management
- **SettingsService**: Configuration and preference management
- **UserService**: Core user operations and account management

### Authentication and Authorization
- **Current User Integration**: Seamless current user identification and management
- **Session Management**: Secure session handling for profile operations
- **Permission Checking**: Role-based access control for administrative functions
- **Security Validation**: Comprehensive validation for all user operations

### Profile and Fundraiser Integration
- **Profile Service**: Complete integration with profile management system
- **Fundraiser Integration**: Special handling for fundraiser profiles and capabilities
- **Referral System**: Integration with referral link generation and management
- **Commission System**: Integration with affiliate commission tracking

### Communication System Integration
- **Message Service**: Integration for preference-based messaging
- **Email Service**: Email preference enforcement and management
- **SMS Service**: SMS preference handling and opt-out management
- **Notification System**: Preference-based notification delivery

## User Workflows

### Profile Editing Workflow
1. **Profile Access**: User navigates to profile editing page
2. **Data Loading**: System loads current user profile information
3. **Form Population**: Profile form populated with existing data
4. **Information Updates**: User modifies profile information as needed
5. **Preference Management**: User updates communication preferences
6. **Address Updates**: User updates address information with validation
7. **Save Processing**: System validates and saves profile changes
8. **Confirmation**: User receives confirmation of successful updates

### User Registration Workflow
1. **Registration Access**: New user accesses registration page (various routes)
2. **Information Entry**: User enters required registration information
3. **Validation**: System validates registration data in real-time
4. **Registration Processing**: System creates new user account
5. **Success Display**: Registration success confirmation displayed
6. **Referral Generation**: System generates referral link for new user
7. **Email Verification**: User receives email verification instructions
8. **Account Activation**: User completes email verification process

### Communication Preferences Workflow
1. **Preference Access**: User accesses communication preferences in profile
2. **Current Status Review**: User reviews current preference settings
3. **Preference Updates**: User modifies consent and unsubscribe settings
4. **Validation**: System validates preference combinations and dependencies
5. **Preference Saving**: System saves updated preference settings
6. **Confirmation**: User receives confirmation of preference updates
7. **System Updates**: All communication systems updated with new preferences

### Administrative User Management Workflow
1. **Admin Access**: Administrator accesses user management interface
2. **User Search**: Administrator searches for specific users or user groups
3. **User Selection**: Administrator selects users for management operations
4. **Account Management**: Administrator performs account status or role changes
5. **Bulk Operations**: Administrator executes bulk operations as needed
6. **Validation**: System validates administrative operations
7. **Audit Logging**: All administrative actions logged for audit trail

## Usage Examples

### Profile Editing Interface
```razor
@page "/profile"
@inject IUserProfileService UserProfileService
@inject AuthService AuthService

<div class="container">
    <div class="row">
        <div class="col-md-8 offset-md-2">
            <div class="card">
                <div class="card-header">
                    <h3 class="card-title">My Profile</h3>
                </div>
                <div class="card-body">
                    @if (profile != null)
                    {
                        <EditForm Model="@profile" OnValidSubmit="HandleProfileUpdate">
                            <DataAnnotationsValidator />
                            <ValidationSummary />

                            <!-- Personal Information -->
                            <div class="row mb-4">
                                <div class="col-md-6">
                                    <label for="firstName" class="form-label">First Name</label>
                                    <InputText id="firstName" @bind-Value="profile.FirstName" class="form-control" />
                                </div>
                                <div class="col-md-6">
                                    <label for="lastName" class="form-label">Last Name</label>
                                    <InputText id="lastName" @bind-Value="profile.LastName" class="form-control" />
                                </div>
                            </div>

                            <!-- Address Information -->
                            <div class="mb-3">
                                <label for="street" class="form-label">Street Address</label>
                                <InputText id="street" @bind-Value="address.Street" class="form-control" />
                            </div>

                            <!-- State Selection -->
                            <div class="col-md-6">
                                <label for="state" class="form-label">State/Province</label>
                                <InputSelect id="state" @bind-Value="address.State" class="form-control">
                                    @foreach (var state in UsStates)
                                    {
                                        <option value="@state.Abbreviation">@state.Name</option>
                                    }
                                </InputSelect>
                            </div>

                            <!-- Communication Preferences -->
                            <div class="form-check mb-3">
                                <InputCheckbox id="consentText" @bind-Value="profile.ConsentReceiveText" class="form-check-input" />
                                <label class="form-check-label" for="consentText">
                                    I consent to receive text messages
                                </label>
                            </div>

                            <div class="form-check mb-3">
                                <InputCheckbox id="unsubscribeMobile" @bind-Value="profile.UnsubscribeMobile" 
                                             class="form-check-input" disabled="@(!profile.ConsentReceiveText)" />
                                <label class="form-check-label @(!profile.ConsentReceiveText ? "text-muted" : "")" 
                                       for="unsubscribeMobile">
                                    Unsubscribe from marketing text messages
                                </label>
                            </div>
                        </EditForm>
                    }
                </div>
            </div>
        </div>
    </div>
</div>
```

### User Registration Interface
```razor
@page "/register"
@using msih.p4g.Server.Features.Base.UserService.Models

<div class="card">
    @if (isRegistered)
    {
        <div class="card-body">
            <div class="alert alert-success">
                Registration successful!
            </div>

            <ReferralLinkComponent Profile="@profile"
                                   OnError="HandleReferralError" />

            <div class="mb-4">
                <h5>Check Your Email</h5>
                <p>We've sent you an email to verify your email address. Please check your email (including spam folder).</p>
            </div>
        </div>
    }
    else
    {
        <div class="card-header">
            <h3 class="card-title">Register New User</h3>
        </div>
        <div class="card-body">
            <EditForm Model="@user" OnValidSubmit="HandleRegistration">
                <DataAnnotationsValidator />

                <div class="row">
                    <div class="col-md-4 mb-3">
                        <label for="email" class="form-label">Email</label>
                        <InputText id="email" class="form-control" @bind-Value="user.Email" required />
                        <ValidationMessage For="@(() => user.Email)" />
                    </div>
                    <div class="col-md-3 mb-3">
                        <label for="first" class="form-label">First</label>
                        <InputText id="first" class="form-control" @bind-Value="profile.FirstName" required />
                        <ValidationMessage For="@(() => profile.FirstName)" />
                    </div>
                    <div class="col-md-3 mb-3">
                        <label for="last" class="form-label">Last</label>
                        <InputText id="last" class="form-control" @bind-Value="profile.LastName" required />
                        <ValidationMessage For="@(() => profile.LastName)" />
                    </div>
                </div>

                <div class="mb-3">
                    <button type="submit" class="btn btn-primary" disabled="@isProcessing">
                        @if (isProcessing)
                        {
                            <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
                            <span> Processing...</span>
                        }
                        else
                        {
                            <span>Register</span>
                        }
                    </button>
                </div>
            </EditForm>
        </div>
    }
</div>
```

### Communication Preferences Management
```csharp
// Preference interdependency handling
private async Task HandleProfileUpdate()
{
    isSaving = true;
    message = string.Empty;

    try
    {
        // Set date of birth from inputs
        SetDateOfBirth();

        // Set address
        profile.Address = address;

        // Save profile with updated preferences
        var updatedProfile = await UserProfileService.UpdateAsync(profile);
        if (updatedProfile != null)
        {
            profile = updatedProfile;
            message = "Profile updated successfully!";
        }
    }
    catch (Exception ex)
    {
        message = $"Error updating profile: {ex.Message}";
    }
    finally
    {
        isSaving = false;
        StateHasChanged();
    }
}
```

## Files

### User Management Pages
```
Client/Features/UserManagement/
├── Pages/
│   ├── EditProfile.razor
│   ├── EditProfile.razor.cs
│   ├── RegisterUser.razor
│   ├── RegisterUser.razor.cs
│   └── Users.razor
└── CLAUDE.md
```

### Related Server Components
```
Server/Features/Base/UserService/ (user management)
Server/Features/Base/ProfileService/ (profile management)
Server/Features/Base/UserProfileService/ (profile integration)
```

### Integration Points
```
Client/Features/Authentication/Services/AuthService.cs
Client/Common/Components/ReferralLinkComponent.razor
Client/Common/Components/AffiliateCommissionTable.razor
```

## Security and Data Protection

### User Data Security
- **Authentication Required**: All profile operations require valid user authentication
- **Data Isolation**: Users can only access and modify their own profile data
- **Input Validation**: Comprehensive validation for all user inputs
- **Secure Storage**: Encrypted storage of sensitive profile information

### Privacy Compliance
- **Consent Management**: Comprehensive consent tracking and management
- **Preference Enforcement**: System-wide enforcement of communication preferences
- **Data Minimization**: Collection and storage of only necessary user information
- **User Control**: Full user control over personal information and preferences

### Administrative Security
- **Role-based Access**: Administrative functions restricted to authorized users
- **Audit Logging**: Comprehensive logging of all administrative actions
- **Secure Operations**: Protected administrative operations with validation
- **Access Control**: Granular access control for different administrative functions

## Performance Optimization

### Efficient Data Management
- **Optimized Loading**: Efficient user data loading and caching
- **Minimal Network Requests**: Optimized API calls for profile operations
- **State Management**: Efficient component state management
- **Form Optimization**: Optimized form handling and validation

### User Experience Optimization
- **Real-time Validation**: Live form validation for immediate feedback
- **Loading States**: Professional loading indicators during operations
- **Error Handling**: User-friendly error messages and recovery options
- **Responsive Design**: Mobile-optimized interfaces for all devices

### Component Efficiency
- **Modular Design**: Efficient component architecture with clear separation
- **Event Handling**: Optimized event handling and callback management
- **Memory Management**: Proper component lifecycle management
- **Data Binding**: Efficient two-way data binding for form controls

This feature provides comprehensive user profile management capabilities that enable users to effectively manage their personal information, communication preferences, and account settings while maintaining security, privacy, and user experience standards.