# Message Service Module

This module provides a comprehensive messaging system that supports both email and SMS communications with built-in templating capabilities and scheduled message delivery.

## Features

- **Multi-Channel Messaging**: Send both email and SMS messages through a unified interface
- **Templating System**: Create, manage, and use message templates with placeholder substitution
- **Message Scheduling**: Schedule messages to be sent at a specific time in the future
- **Message History**: Track message history and delivery status
- **Background Processing**: Automatic processing of scheduled messages via a background service
- **Template Categories**: Organize templates by category and message type
- **Default Templates**: Set default templates for specific categories

## Database Schema

The module uses the following database entities:

### Message

Stores information about sent and scheduled messages:

- `MessageType`: Type of message (Email or SMS)
- `From`: Sender email or phone number
- `To`: Recipient email or phone number
- `Subject`: Subject line (for emails)
- `Content`: Message content
- `IsHtml`: Whether the content is HTML (for emails)
- `SentOn`: When the message was sent
- `IsSent`: Whether the message was successfully sent
- `ErrorMessage`: Any error that occurred during sending
- `RetryCount`: Number of retry attempts
- `ScheduledFor`: When the message should be sent (for scheduled messages)

### MessageTemplate

Stores reusable message templates:

- `Name`: Template name
- `Description`: Template description
- `MessageType`: Type of message (Email or SMS)
- `Category`: Template category (e.g., ThankYou, DonationSolicitation)
- `DefaultSubject`: Default subject line (for email templates)
- `TemplateContent`: Template content with placeholders
- `IsHtml`: Whether the content is HTML (for email templates)
- `AvailablePlaceholders`: List of available placeholders
- `IsDefault`: Whether this is the default template for its category
- `DefaultSender`: Default sender email or phone number

### MessageTemplateUsage

Links messages to the templates used to create them:

- `MessageId`: ID of the message
- `TemplateId`: ID of the template used
- `PlaceholderValuesJson`: JSON-serialized dictionary of placeholder values

## Template Placeholders

Templates support placeholders in the format `{{PlaceholderName}}`. For example:

```html
<p>Dear {{firstName}},</p>
<p>Thank you for your donation of {{amount}} to {{organizationName}}.</p>
```

## Configuration

No additional configuration is required for the Message Service itself, but it depends on:

1. Email Service configuration (SendGrid or AWS SES)
2. SMS Service configuration (Twilio)

## Usage

### Service Registration

The service and its dependencies are registered using the extension method:

```csharp
builder.Services.AddMessageServices(builder.Configuration, builder.Environment);
```

### Sending Email Messages

```csharp
public class MyService
{
    private readonly IMessageService _messageService;

    public MyService(IMessageService messageService)
    {
        _messageService = messageService;
    }

    public async Task SendConfirmationEmail(string email, string name)
    {
        await _messageService.SendEmailAsync(
            to: email,
            subject: "Registration Confirmation",
            htmlContent: $"<p>Dear {name},</p><p>Thank you for registering.</p>",
            from: "noreply@example.com"
        );
    }
}
```

### Sending SMS Messages

```csharp
public class MyService
{
    private readonly IMessageService _messageService;

    public MyService(IMessageService messageService)
    {
        _messageService = messageService;
    }

    public async Task SendConfirmationSms(string phoneNumber, string name)
    {
        await _messageService.SendSmsAsync(
            to: phoneNumber,
            content: $"Dear {name}, thank you for registering.",
            from: "+12125551234"
        );
    }
}
```

### Using Templates

```csharp
public class MyService
{
    private readonly IMessageService _messageService;

    public MyService(IMessageService messageService)
    {
        _messageService = messageService;
    }

    public async Task SendDonationThankYou(string email, string name, decimal amount)
    {
        var placeholders = new Dictionary<string, string>
        {
            ["firstName"] = name,
            ["amount"] = amount.ToString("C"),
            ["organizationName"] = "Make Sure It Happens"
        };

        await _messageService.SendTemplatedMessageByNameAsync(
            templateName: "Donor Thank You Email",
            to: email,
            placeholderValues: placeholders
        );
    }
}
```

### Scheduling Messages

```csharp
public class MyService
{
    private readonly IMessageService _messageService;

    public MyService(IMessageService messageService)
    {
        _messageService = messageService;
    }

    public async Task ScheduleReminder(string email, string name, DateTime reminderDate)
    {
        await _messageService.ScheduleEmailAsync(
            to: email,
            subject: "Event Reminder",
            htmlContent: $"<p>Dear {name},</p><p>This is a reminder about your upcoming event.</p>",
            scheduledFor: reminderDate
        );
    }
}
```

### Managing Templates

```csharp
public class TemplateManagerService
{
    private readonly IMessageService _messageService;

    public TemplateManagerService(IMessageService messageService)
    {
        _messageService = messageService;
    }

    public async Task<IEnumerable<MessageTemplate>> GetEmailTemplates()
    {
        return await _messageService.GetAllTemplatesAsync("Email");
    }

    public async Task<MessageTemplate> CreateTemplate(MessageTemplate template)
    {
        return await _messageService.CreateTemplateAsync(template);
    }

    public async Task<bool> SetAsDefault(int templateId)
    {
        return await _messageService.SetTemplateAsDefaultAsync(templateId);
    }
}
```

## Background Processing

The `MessageProcessingService` is a background service that periodically checks for and sends scheduled messages. It runs automatically and requires no manual intervention.

## Template Processing

The `TemplateProcessor` class handles:

1. Extracting placeholders from templates
2. Validating that all required placeholders have values
3. Replacing placeholders with actual values

## Dependencies

- `IEmailService`: Interface for sending email messages
- `ISmsService`: Interface for sending SMS messages
- `IMessageRepository`: Repository for Message entities
- `IMessageTemplateRepository`: Repository for MessageTemplate entities
