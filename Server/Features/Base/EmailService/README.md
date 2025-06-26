# SMTP2GO Email Service

This module provides email sending capabilities using the SMTP2GO API.

## Features

- Sending HTML emails
- Email address validation
- Automatic conversion of HTML to plain text
- Configuration via settings service or appsettings.json

## Implementation Details

The service makes direct HTTP calls to the SMTP2GO API endpoint (`https://api.smtp2go.com/v3/email/send`) rather than using a client library. This provides:

- Simplified dependencies
- Direct control over the API request format
- Easier customization of error handling
- Consistent behavior with other email service implementations

### Note on API Client Library

While SMTP2GO provides an official .NET client library (`Smtp2Go.ApiClient`), we've chosen to use direct HTTP calls for several reasons:

1. The client library has compatibility issues with some .NET versions
2. Direct HTTP calls provide more control over the request/response lifecycle
3. This approach is consistent with our other email service implementations (SendGrid, AWS SES)
4. It reduces external dependencies, making the code easier to maintain

If you prefer to use the official client library, you would need to modify the implementation to use `Smtp2GoApiClient` instead of `HttpClient`:
var apiClient = new Smtp2GoApiClient(_apiKey);
var message = new EmailMessage(sender, to) {
    Subject = subject,
    BodyHtml = htmlContent,
    BodyText = textContent
};
var response = await apiClient.SendReceive<EmailMessage, EmailResponse>(
    message, 
    new EmailResponse(), 
    "email/send"
);
## Configuration

Add the following configuration to your `appsettings.json`:
{
  "EmailProvider": "SMTP2GO", // Set this to use SMTP2GO as the default provider
  "SMTP2GO": {
    "ApiKey": "your-api-key",
    "FromEmail": "no-reply@yourdomain.com",
    "FromName": "Your Application Name"
  }
}
## Usage

The service is automatically registered in the DI container when you call `AddEmailServices()` in `Program.cs`.
// In Program.cs
builder.Services.AddEmailServices(builder.Configuration);
### Sending Emails Directly
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
### Using with MessageService

The `IEmailService` is also used by the `MessageService` for more advanced scenarios, including:

- Template-based emails
- Scheduled emails
- Email tracking

See the MessageService documentation for more details on these features.

## API Documentation

For more information about the SMTP2GO API, see the [official documentation](https://apidoc.smtp2go.com/documentation/).
