# Email Service

This module provides email sending capabilities supporting multiple providers (SMTP2GO, SendGrid, AWS SES).

## Features

- Sending HTML emails
- Email address validation
- Automatic conversion of HTML to plain text
- Configuration via settings service or appsettings.json
- Multiple provider support

## Implementation Details

The service makes direct HTTP calls to email provider APIs rather than using client libraries. This provides:

- Simplified dependencies
- Direct control over the API request format
- Easier customization of error handling
- Consistent behavior across all email service implementations

## Configuration

Add the following configuration to your `appsettings.json`:

```json
{
  "MessageService": {
    "EmailProvider": "SendGrid", // Options: "SendGrid", "SMTP2GO", "AWS"
  },
  "SendGrid": {
    "ApiKey": "your-sendgrid-api-key",
    "FromEmail": "no-reply@yourdomain.com",
    "FromName": "Your Application Name"
  },
  "SMTP2GO": {
    "ApiKey": "your-smtp2go-api-key",
    "FromEmail": "no-reply@yourdomain.com",
    "FromName": "Your Application Name"
  },
  "AWS": {
    "AccessKey": "your-aws-access-key",
    "SecretKey": "your-aws-secret-key",
    "Region": "us-east-1",
    "FromEmail": "no-reply@yourdomain.com",
    "FromName": "Your Application Name"
  }
}
```

## Usage

The service is automatically registered in the DI container when you call `AddEmailServices()`:

```csharp
// In Program.cs
builder.Services.AddEmailServices(builder.Configuration);
```

### Sending Emails Directly

```csharp
public class MyService
{
    private readonly IEmailService _emailService;

    public MyService(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task SendWelcomeEmail(string email, string name)
    {
        string htmlContent = $@"
            <html>
                <body>
                    <h1>Welcome, {name}!</h1>
                    <p>Thank you for registering with our service.</p>
                </body>
            </html>";

        await _emailService.SendEmailAsync(
            to: email,
            from: null, // Will use default from email
            subject: "Welcome to Our Service",
            htmlContent: htmlContent
        );
    }
}
```

### Using with MessageService

The `IEmailService` is also used by the `MessageService` for more advanced scenarios, including:

- Template-based emails
- Scheduled emails
- Email tracking

See the MessageService documentation for more details on these features.