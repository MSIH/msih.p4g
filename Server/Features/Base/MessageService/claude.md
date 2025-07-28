# MessageService

## Overview
Comprehensive messaging system supporting both email and SMS communications with templating, scheduling, and background processing capabilities for the MSIH Platform for Good application.

## Architecture
- **Services**: MessageService, MessageProcessingService (background), TemplateProcessor
- **Repositories**: MessageRepository, MessageTemplateRepository
- **Models**: Message, MessageTemplate, MessageTemplateUsage
- **Data**: MessageDbContext with migration support
- **Utilities**: TemplateProcessor for placeholder handling

## Key Features
- **Multi-Channel**: Unified interface for email and SMS
- **Template System**: Create, manage, and use message templates with placeholders
- **Message Scheduling**: Schedule messages for future delivery
- **Background Processing**: Automatic processing of scheduled messages
- **Message History**: Track delivery status and errors
- **Template Categories**: Organize templates by purpose (ThankYou, DonationSolicitation, etc.)
- **Default Templates**: Set default templates per category

## Database Entities

### Message
- MessageType (Email/SMS), From, To, Subject, Content
- SentOn, IsSent, ErrorMessage, RetryCount
- ScheduledFor (for scheduled messages)

### MessageTemplate  
- Name, Description, MessageType, Category
- DefaultSubject, TemplateContent, IsHtml
- AvailablePlaceholders, IsDefault, DefaultSender

### MessageTemplateUsage
- Links messages to templates used
- Stores placeholder values as JSON

## Template System
Templates support `{{PlaceholderName}}` format:
```html
<p>Dear {{firstName}},</p>
<p>Thank you for your donation of {{amount}} to {{organizationName}}.</p>
```

## Usage
```csharp
// Registration
builder.Services.AddMessageServices(builder.Configuration, builder.Environment);

// Direct messaging
await _messageService.SendEmailAsync(
    to: email,
    subject: "Registration Confirmation", 
    htmlContent: $"<p>Dear {name}, thank you for registering.</p>",
    from: "noreply@example.com"
);

// Templated messaging
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

// Scheduled messaging
await _messageService.ScheduleEmailAsync(
    to: email,
    subject: "Event Reminder",
    htmlContent: reminderContent,
    scheduledFor: reminderDate
);
```

## Integration
- **EmailService**: Uses IEmailService for email delivery
- **SmsService**: Uses ISmsService for SMS delivery
- **UserService**: Registration confirmations
- **DonationService**: Thank you messages
- **PayoutService**: Payout notifications

## Background Processing
MessageProcessingService runs as hosted service, automatically processing scheduled messages with retry logic and error handling.

## Files
- `Services/MessageService.cs`
- `Services/MessageProcessingService.cs`
- `Repositories/MessageRepository.cs`
- `Repositories/MessageTemplateRepository.cs`
- `Models/Message.cs`
- `Models/MessageTemplate.cs`
- `Models/MessageTemplateUsage.cs`
- `Utilities/TemplateProcessor.cs`
- `Data/MessageDbContext.cs`