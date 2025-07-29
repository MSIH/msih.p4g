# AuthorizationService

## Overview
The AuthorizationService provides client-side role-based authorization functionality for Blazor Server components. It validates user access permissions, manages redirect flows for unauthorized users, and integrates with the AuthService to provide seamless security checks across the MSIH Platform for Good application.

## Architecture

### Components
- **AuthorizationService**: Main service class handling authorization logic
- **AuthService**: Dependency for retrieving current user information
- **NavigationManager**: Blazor navigation service for redirects
- **UserRole**: Enum defining user roles (Donor, Fundraiser, Admin)

### Dependencies
- Microsoft.AspNetCore.Components (NavigationManager)
- AuthService from Client.Features.Authentication.Services
- UserRole from Server.Features.Base.UserService.Models
- Task-based async patterns for Blazor Server compatibility

### Client-side Patterns
- Async authorization checks compatible with Blazor lifecycle
- Non-blocking redirect patterns using Task.Run
- Exception handling with console logging for client debugging
- Role-based access control with flexible role arrays

## Key Features
- Role-based access validation with multiple role support
- Automatic redirect functionality for unauthorized users
- Admin-specific access control with specialized methods
- Current user role retrieval and caching
- Error handling with graceful fallbacks
- Non-blocking navigation to prevent render cycle issues

## Blazor Integration

### Component Authorization Pattern
```csharp
@inject AuthorizationService AuthorizationService
@implements IDisposable

protected override async Task OnInitializedAsync()
{
    // Check authorization before rendering sensitive content
    var hasAccess = await AuthorizationService.CheckAccessOrRedirectAsync(
        new[] { UserRole.Admin, UserRole.Fundraiser }
    );
    
    if (!hasAccess)
    {
        return; // Component will redirect automatically
    }
    
    // Continue with authorized initialization
    await LoadSensitiveData();
}
```

### Admin-Only Pages
```csharp
@code {
    protected override async Task OnInitializedAsync()
    {
        // Simple admin check with automatic redirect
        if (!await AuthorizationService.AdminAccessOnlyAsync())
        {
            return; // Redirects to /404 automatically
        }
        
        await LoadAdminData();
    }
}
```

### Conditional UI Rendering
```csharp
@code {
    private UserRole? currentUserRole;
    
    protected override async Task OnInitializedAsync()
    {
        currentUserRole = await AuthorizationService.GetCurrentUserRoleAsync();
        StateHasChanged();
    }
}

@if (currentUserRole == UserRole.Admin)
{
    <button class="btn btn-danger" @onclick="DeleteItem">Delete</button>
}
@if (currentUserRole == UserRole.Fundraiser || currentUserRole == UserRole.Admin)
{
    <a href="/fundraiser-dashboard" class="btn btn-primary">Dashboard</a>
}
```

## Usage

### Basic Authorization Checks
```csharp
@inject AuthorizationService AuthorizationService

// Check if user has specific role access
var hasAdminAccess = await AuthorizationService.HasAccessAsync(UserRole.Admin);
var hasMultiRoleAccess = await AuthorizationService.HasAccessAsync(
    UserRole.Admin, UserRole.Fundraiser
);

// Get current user's role
var currentRole = await AuthorizationService.GetCurrentUserRoleAsync();

if (currentRole == UserRole.Admin)
{
    // Show admin-specific UI
}
```

### Authorization with Redirect
```csharp
// Check access and redirect if unauthorized
var isAuthorized = await AuthorizationService.CheckAccessOrRedirectAsync(
    allowedRoles: new[] { UserRole.Admin },
    redirectUrl: "/unauthorized"
);

if (isAuthorized)
{
    // Continue with authorized logic
    await LoadRestrictedContent();
}
```

### Admin Authorization Pattern
```csharp
// Specialized admin check with automatic 404 redirect
protected override async Task OnInitializedAsync()
{
    if (!await AuthorizationService.AdminAccessOnlyAsync())
    {
        return; // Automatically redirects to /404
    }
    
    // Admin-only initialization logic
    await InitializeAdminPanel();
}
```

### Error Handling
```csharp
try
{
    var hasAccess = await AuthorizationService.HasAccessAsync(UserRole.Fundraiser);
    
    if (hasAccess)
    {
        await PerformAuthorizedAction();
    }
    else
    {
        ShowUnauthorizedMessage();
    }
}
catch (Exception ex)
{
    // Service handles exceptions gracefully and logs to console
    // Application continues to function with denied access
    ShowErrorMessage("Unable to verify authorization");
}
```

## Integration

### AuthService Integration
- **User Retrieval**: Leverages AuthService.GetCurrentUserAsync() for user data
- **Authentication State**: Depends on AuthService authentication state
- **Role Information**: Extracts UserRole from authenticated User entity
- **Session Management**: Integrates with AuthService session lifecycle

### NavigationManager Integration
- **Redirect Functionality**: Uses NavigationManager.NavigateTo() for redirects
- **Non-blocking Navigation**: Implements Task.Run pattern to avoid render cycle conflicts
- **Force Load**: Uses forceLoad parameter for reliable navigation
- **URL Management**: Supports both relative and absolute redirect URLs

### Blazor Server Patterns
- **Async Component Lifecycle**: Compatible with OnInitializedAsync patterns
- **StateHasChanged Integration**: Works with Blazor's state management
- **Prerendering Support**: Handles server-side prerendering scenarios
- **Exception Safety**: Prevents authorization failures from breaking component render

### Security Features
- **Client-side Validation**: Provides immediate feedback for unauthorized access
- **Server-side Backing**: Relies on server-side AuthService for actual user data
- **Role-based Security**: Implements proper role-based access control
- **Graceful Degradation**: Handles authentication failures without breaking UI

## Security Considerations

### Client-side Nature
- This service provides **UI-level authorization only**
- Server-side services must enforce their own authorization
- Client-side checks are for user experience, not security enforcement
- Always validate permissions on server-side service calls

### Best Practices
```csharp
// Always check authorization in OnInitializedAsync
protected override async Task OnInitializedAsync()
{
    if (!await AuthorizationService.AdminAccessOnlyAsync())
        return;
    
    // Server-side services should also validate permissions
    var data = await AdminService.GetSensitiveDataAsync(); // Server validates admin role
}
```

## Files

```
Client/Common/Services/
├── AuthorizationService.cs      # Main authorization service implementation
└── CLAUDE.md                   # This documentation file
```

## Related Files

```
Client/Features/Authentication/Services/
└── AuthService.cs              # Authentication service dependency

Server/Features/Base/UserService/Models/
└── User.cs                     # User entity with Role property
└── UserRole.cs                 # Role enumeration

Client/Layout/Components/
├── AdminLayout.razor           # Uses authorization for admin pages
└── NavMenu.razor               # Conditional navigation based on roles
```