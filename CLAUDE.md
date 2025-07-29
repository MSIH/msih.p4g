# MSIH Platform for Good - Development Guidelines

## Project Overview
This is a **Blazor Server** application for the Make Sure It Happens Platform for Good - a fundraising/donation platform that enables organizations to run campaigns, process donations, manage fundraisers/affiliates, and handle payouts with comprehensive messaging capabilities.

## 🚨 CRITICAL ARCHITECTURE RULES

### Application Type
- **Blazor Server Application** - NOT a Web API
- **DO NOT CREATE** Web API controllers (use Blazor components instead)
- **DO NOT CREATE** separate API endpoints or REST controllers
- Services are consumed directly in Razor components via dependency injection

### Database Context Pattern
- **NEVER** create separate DbContext classes for individual services
- **ALWAYS** use the partial class pattern with `ApplicationDbContext`
- **Pattern**: Create `Server/Common/Data/ApplicationDbContext.{ServiceName}.cs` 
- Add entity configuration in partial `Configure{EntityName}Model(ModelBuilder modelBuilder)` method
- Update main `ApplicationDbContext.cs` to call your configuration method

#### Example DbContext Implementation:
```csharp
// File: Server/Common/Data/ApplicationDbContext.YourService.cs
namespace msih.p4g.Server.Common.Data
{
    public partial class ApplicationDbContext
    {
        public DbSet<YourEntity> YourEntities { get; set; }

        partial void ConfigureYourEntityModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<YourEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                // Configure entity properties and relationships
            });
        }
    }
}

// Update main ApplicationDbContext.cs OnModelCreating method:
ConfigureYourEntityModel(modelBuilder);

// Add partial method declaration:
partial void ConfigureYourEntityModel(ModelBuilder modelBuilder);
```

### Repository Pattern
- Repositories should inherit from `GenericRepository<T>` in `Server/Common/Data/Repositories/`
- Repositories should use `ApplicationDbContext`, NOT separate contexts
- Constructor: `public YourRepository(ApplicationDbContext context) : base(context)`

### Service Registration
- Create extension methods in `Extensions/{ServiceName}ServiceExtensions.cs`
- Register repositories and services with DI container
- **DO NOT** register separate DbContext instances (use shared ApplicationDbContext)

## Service Architecture Standards

### Required Service Structure
```
Server/Features/{ServiceName}/
├── Interfaces/
│   ├── I{ServiceName}Service.cs
│   └── I{ServiceName}Repository.cs
├── Models/
│   ├── {EntityName}.cs
│   └── {EntityName}Dto.cs (if needed)
├── Services/
│   └── {ServiceName}Service.cs
├── Repositories/
│   └── {ServiceName}Repository.cs
├── Extensions/
│   └── {ServiceName}ServiceExtensions.cs
└── CLAUDE.md (documentation - must be uppercase)
```

### Entity Design
- All entities inherit from `BaseEntity` (provides Id, audit fields)
- Implements `IAuditableEntity` for automatic audit trail
- Use data annotations for validation
- Configure relationships in ApplicationDbContext partial class

### Background Services
- For automatic processing (like recurring donations), create hosted services
- Inherit from `BackgroundService`
- Register with `services.AddHostedService<YourBackgroundService>()`

## Base vs Domain Services

### Base Services (`Server/Features/Base/`)
Core infrastructure services used by multiple features:
- EmailService, MessageService, PaymentService, PayoutService
- SmsService, UserService, SettingsService, etc.

### Domain Services (`Server/Features/`)
Business-specific services:
- CampaignService, DonationService, DonorService
- FundraiserService, OrganizationService, etc.

## Blazor Component Integration

### Service Usage in Components
```csharp
@page "/your-page"
@inject IYourService YourService

<h3>Your Component</h3>

@code {
    private async Task HandleAction()
    {
        var result = await YourService.DoSomethingAsync();
        // Handle result
    }
}
```

### Data Flow
1. User interacts with Blazor component
2. Component calls service methods directly (no API layer)
3. Service processes business logic
4. Repository handles data access
5. Result returned to component for UI update

## Code Standards

### Naming Conventions
- Services: `{DomainName}Service` (e.g., `DonationService`)
- Interfaces: `I{ServiceName}` (e.g., `IDonationService`)
- Repositories: `{EntityName}Repository` (e.g., `DonationRepository`)
- Models: `{EntityName}` (e.g., `Donation`)
- DTOs: `{EntityName}Dto` or `{Action}{EntityName}Dto` (e.g., `CreateDonationDto`)

### Error Handling
- Use structured logging with `ILogger<T>`
- Implement try-catch blocks for external service calls
- Return appropriate result types (bool, entity, or custom result objects)
- Log errors but don't expose sensitive information to UI

### Security & Data Protection
- All entities include audit fields (CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
- Use soft deletes (IsDeleted flag) when appropriate  
- Validate input data with data annotations
- Follow principle of least privilege for database operations

## Documentation Requirements

### CLAUDE.md Files
Each service MUST include a `CLAUDE.md` file (all uppercase) with:
- **Overview**: Purpose and key functionality
- **Architecture**: Components, models, interfaces
- **Key Features**: Bullet list of capabilities
- **Database Schema**: Entity descriptions
- **Usage**: Code examples for common operations
- **Integration**: How it connects with other services
- **Files**: List of all files in the service

### Comments
- Use XML documentation comments for public methods
- Include business context in comments, not just technical details
- Document complex business rules and calculations

## Migration and Deployment

### Database Migrations
- Use Entity Framework migrations through ApplicationDbContext
- Test migrations in development before applying to production
- Include rollback plans for breaking changes

### Testing
- Unit tests in `Tests/Server.Tests/Features/{ServiceName}/`
- Test business logic, not just basic CRUD operations
- Mock external dependencies (payment providers, email services, etc.)

## Common Patterns

### Service Implementation Template
```csharp
public class YourService : IYourService
{
    private readonly IYourRepository _repository;
    private readonly ILogger<YourService> _logger;

    public YourService(IYourRepository repository, ILogger<YourService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<YourEntity> CreateAsync(YourEntity entity, string createdBy = "System")
    {
        entity.CreatedBy = createdBy;
        entity.CreatedOn = DateTime.UtcNow;
        entity.IsActive = true;

        var result = await _repository.AddAsync(entity, createdBy);
        _logger.LogInformation("Created {EntityType} with ID {Id}", typeof(YourEntity).Name, result.Id);
        return result;
    }
}
```

## Integration Points

### Payment Processing
- Use existing `PaymentService` for all payment operations
- Create `PaymentTransaction` records for audit trail
- Handle payment failures gracefully

### Messaging
- Use `MessageService` for email/SMS communications
- Create templates for consistent messaging
- Handle message delivery failures

### User Management
- Use `UserService` and `DonorService` for user operations
- Respect user permissions and roles
- Maintain referral code integrity

## Remember: This is a Blazor Server App!
- No REST API controllers needed
- Services are consumed directly in components
- Use SignalR for real-time updates if needed
- State management through Blazor's built-in mechanisms