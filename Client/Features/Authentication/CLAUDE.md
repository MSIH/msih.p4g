# AuthService

## Overview
The AuthService provides client-side authentication state management for the MSIH Platform for Good Blazor Server application. It handles email-based login workflows, user session persistence using browser storage, authentication state events, and integration with server-side user services for seamless authentication experiences.

## Architecture

### Components
- **AuthService**: Main authentication service managing user sessions
- **ProtectedLocalStorage**: Blazor Server secure browser storage
- **IUserService**: Server-side user management service
- **IEmailVerificationService**: Server-side email verification service
- **User**: User entity from server-side models
- **AuthStateChanged**: Event for authentication state notifications

### Dependencies
- Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage
- Server.Features.Base.UserService (IUserService, User model)
- Server.Features.Base.UserService (IEmailVerificationService)
- Task-based async patterns for Blazor Server compatibility

### Client-side Patterns
- Protected browser storage for secure session persistence
- Event-driven authentication state management
- Async initialization compatible with Blazor lifecycle
- Prerendering-safe storage operations
- Exception handling for JavaScript interop issues

## Key Features
- Email-based authentication without passwords
- Secure browser storage for session persistence
- Authentication state change notifications
- Email verification integration
- Automatic session restoration on app reload
- Prerendering compatibility
- Graceful error handling for storage operations
- User logout with server-side cleanup

## Blazor Integration

### Component Authentication State
```csharp
@inject AuthService AuthService
@implements IDisposable

<AuthorizeView>
    <Authorized>
        <p>Welcome, @AuthService.CurrentUser?.Email!</p>
        <button @onclick="HandleLogout">Logout</button>
    </Authorized>
    <NotAuthorized>
        <a href="/login">Please log in</a>
    </NotAuthorized>
</AuthorizeView>

@code {
    protected override async Task OnInitializedAsync()
    {
        AuthService.AuthStateChanged += OnAuthStateChanged;
        
        if (!AuthService.IsInitialized)
        {
            await AuthService.InitializeAuthenticationStateAsync();
        }
    }
    
    private void OnAuthStateChanged() => InvokeAsync(StateHasChanged);
    
    private async Task HandleLogout()
    {
        await AuthService.LogoutAsync();
    }
    
    public void Dispose()
    {
        AuthService.AuthStateChanged -= OnAuthStateChanged;
    }
}
```

### Login Page Integration  
```csharp
@page "/login"
@inject AuthService AuthService

<EditForm Model="loginModel" OnValidSubmit="HandleLogin">
    <InputText @bind-Value="loginModel.Email" />
    <button type="submit">Send Login Link</button>
</EditForm>

@code {
    private LoginModel loginModel = new();
    
    private async Task HandleLogin()
    {
        var (success, message) = await AuthService.RequestLoginEmailAsync(loginModel.Email);
        
        if (success)
        {
            // Show success message and redirect to verification page
            NavigationManager.NavigateTo("/verify-email");
        }
        else
        {
            // Show error message  
            errorMessage = message;
        }
    }
}
```

### Email Verification Integration
```csharp
@page "/verify"
@inject AuthService AuthService

@code {
    protected override async Task OnInitializedAsync()
    {
        // Check for verification token in URL
        var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("token", out var token))
        {
            await HandleEmailVerification(token);
        }
    }
    
    private async Task HandleEmailVerification(string token)
    {
        // Server-side verification would happen here
        // On success, user would be logged in automatically
        var user = await UserService.VerifyEmailTokenAsync(token);
        if (user != null)
        {
            await AuthService.LoginAsync(user);
            NavigationManager.NavigateTo("/dashboard");
        }
    }
}
```

## Usage

### Authentication Lifecycle
```csharp
@inject AuthService AuthService

// Initialize authentication on app startup
protected override async Task OnInitializedAsync()
{
    await AuthService.InitializeAuthenticationStateAsync();
    
    // Check if user is authenticated
    if (AuthService.IsAuthenticated)
    {
        var currentUser = AuthService.CurrentUser;
        Console.WriteLine($"User logged in: {currentUser.Email}");
    }
}

// Request login email
var (success, message) = await AuthService.RequestLoginEmailAsync("user@example.com");
if (success)
{
    // Email sent successfully
    ShowSuccessMessage(message);
}

// Login user after email verification
await AuthService.LoginAsync(verifiedUser);

// Logout user
await AuthService.LogoutAsync();
```

### State Management
```csharp
// Subscribe to authentication state changes
AuthService.AuthStateChanged += OnAuthStateChanged;

private void OnAuthStateChanged()
{
    if (AuthService.IsAuthenticated)
    {
        // User logged in
        LoadUserSpecificData();
    }
    else
    {
        // User logged out
        ClearUserData();
        NavigateTo("/login");
    }
    
    InvokeAsync(StateHasChanged);
}

// Get current user safely
var currentUser = await AuthService.GetCurrentUserAsync();
if (currentUser != null)
{
    // User is authenticated and email verified
    ProcessUserData(currentUser);
}
```

### Error Handling
```csharp
try
{
    await AuthService.InitializeAuthenticationStateAsync();
}
catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop"))
{
    // Expected during prerendering - initialization will complete after render
    Console.WriteLine("Auth initialization deferred due to prerendering");
}
catch (Exception ex)
{
    // Other storage errors - app continues to function
    Console.WriteLine($"Auth initialization error: {ex.Message}");
    // User will need to log in again
}
```

## Integration

### Server-side UserService Integration
- **User Lookup**: Uses IUserService.GetByEmailAsync() for user retrieval
- **User Validation**: Validates user existence and email confirmation status
- **Session Management**: Integrates with IUserService.LogOutUserByIdAsync()
- **Profile Data**: Supports profile inclusion in user queries

### Email Verification Integration
- **Verification Emails**: Uses IEmailVerificationService.SendVerificationEmailAsync()
- **Token Validation**: Integrates with server-side token verification
- **Email Confirmation**: Validates user.EmailConfirmed before authentication
- **Automatic Logout**: Logs out users with unconfirmed emails

### Browser Storage Integration
- **Protected Storage**: Uses ProtectedLocalStorage for security
- **Session Persistence**: Stores userId for session restoration
- **Storage Operations**: Handles Set, Get, and Delete operations
- **Prerendering Safety**: Manages JavaScript interop limitations

### Authentication Flow
1. **User requests login** via email address
2. **Server sends verification email** with login token
3. **User clicks email link** or enters token manually
4. **Server verifies token** and confirms email
5. **Client stores user session** in protected browser storage
6. **Authentication state updated** and components notified

## Security Features

### Protected Browser Storage
- Uses Blazor Server's ProtectedLocalStorage for secure client-side storage
- Data is encrypted and tied to the current user session
- Automatic cleanup when browser storage is cleared
- Protected against XSS attacks through Blazor's security model

### Email Verification Requirements
- Users must verify email before authentication completes
- Unverified users are automatically logged out
- Email confirmation status checked on every session restoration
- Integration with server-side verification workflows

### Session Management
- Server-side logout updates user state
- Client-side storage cleanup on logout
- Automatic session restoration with verification
- Graceful handling of expired or invalid sessions

## Email-Based Authentication Flow

### Login Process
```
1. User enters email address
2. AuthService.RequestLoginEmailAsync() called
3. Server validates user exists
4. Server sends verification email via IEmailVerificationService
5. User receives email with verification link
6. User clicks link or enters token
7. Server verifies token and returns user
8. AuthService.LoginAsync() called with verified user
9. User session stored in protected browser storage
10. AuthStateChanged event fired
11. Components updated with authenticated state
```

### Logout Process
```
1. AuthService.LogoutAsync() called
2. IUserService.LogOutUserByIdAsync() updates server state
3. Protected browser storage cleared
4. Current user set to null
5. AuthStateChanged event fired
6. Components updated with unauthenticated state
```

## Files

```
Client/Features/Authentication/Services/
├── AuthService.cs              # Main authentication service implementation
└── CLAUDE.md                   # This documentation file
```

## Related Files

```
Client/Features/Authentication/Pages/
├── Login.razor                 # Login page using AuthService
└── VerifyEmail.razor           # Email verification page

Server/Features/Base/UserService/
├── Interfaces/
│   ├── IUserService.cs         # User management service interface
│   └── IEmailVerificationService.cs # Email verification interface
├── Services/
│   ├── UserService.cs          # User management implementation  
│   └── EmailVerificationService.cs # Email verification implementation
└── Models/
    └── User.cs                 # User entity model

Client/Common/Services/
└── AuthorizationService.cs     # Uses AuthService for role-based authorization
```