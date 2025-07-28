# EmailService

## Overview
Multi-provider email service for the MSIH Platform for Good application. Supports AWS SES, SendGrid, and SMTP2GO providers with automatic HTML-to-text conversion and email validation.

## Architecture
- **Services**: AWSSESEmailService, SendGridEmailService, Smtp2GoEmailService
- **Interface**: IEmailService
- **Extensions**: EmailServiceExtensions for DI registration
- **Provider Selection**: Configurable via appsettings.json

## Key Features
- Multi-provider email sending (AWS SES, SendGrid, SMTP2GO)
- HTML email with automatic plain text conversion
- Email address validation
- Provider abstraction through factory pattern
- Configurable default sender information

## Configuration
```json
{
  "EmailProvider": "SMTP2GO", // AWS_SES | SendGrid | SMTP2GO
  "SMTP2GO": {
    "ApiKey": "your-api-key",
    "FromEmail": "no-reply@yourdomain.com",
    "FromName": "Your Application Name"
  }
}
```

## Usage
```csharp
// Registration in Program.cs
builder.Services.AddEmailServices(builder.Configuration);

// Direct usage
public class MyService
{
    private readonly IEmailService _emailService;
    
    public async Task SendWelcomeEmail(string email, string name)
    {
        string htmlContent = $@"
            <html><body>
                <h1>Welcome, {name}!</h1>
                <p>Thank you for registering.</p>
            </body></html>";
            
        await _emailService.SendEmailAsync(
            to: email,
            from: null, // Uses default
            subject: "Welcome",
            htmlContent: htmlContent
        );
    }
}
```

## Integration
- **MessageService**: Used for template-based and scheduled emails
- **UserService**: Email verification workflows
- **PayoutService**: Payout notifications

## Implementation Notes
- Direct HTTP API calls instead of client libraries for consistency
- Error handling with provider-specific responses
- Supports both HTML and plain text content
- Thread-safe and async throughout

## Files
- `Services/AWSSESEmailService.cs`
- `Services/SendGridEmailService.cs` 
- `Services/Smtp2GoEmailService.cs`
- `Interfaces/IEmailService.cs`
- `Extensions/EmailServiceExtensions.cs`