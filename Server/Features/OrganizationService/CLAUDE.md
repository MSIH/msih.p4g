# OrganizationService Feature - Complete Non-Profit Organization Management

## Overview
The OrganizationService feature provides comprehensive management for non-profit organizations in the MSIH Platform for Good. This feature handles organization registration, profile management, campaign associations, donation tracking, and administrative oversight. It serves as the central hub for managing the non-profit entities that benefit from the fundraising platform.

## Architecture

### Core Components
- **OrganizationService**: Main service for organization operations and business logic
- **OrganizationRepository**: Data access layer with enhanced querying capabilities
- **OrganizationDataSeeder**: Automated data seeding for initial organization setup
- **ServiceCollectionExtensions**: Dependency injection configuration

### Data Models
- **Organization**: Core entity representing non-profit organizations with comprehensive profile information
- **Campaign Integration**: Relationship management with campaign entities
- **Donation Integration**: Tracking of donations received by organizations

### Database Integration
- Uses ApplicationDbContext with dedicated OrganizationDbContext for specialized operations
- Entity relationships with Campaign and Donation entities
- Support for soft deletes and comprehensive audit trails
- Advanced querying with related data loading

## Key Features

### Organization Profile Management
- Complete organization registration with legal entity information
- Tax ID (EIN) management with uniqueness validation
- Contact information management (email, phone, website)
- Physical address storage with full address components
- Logo and branding asset management

### Legal and Compliance Features
- Organization type classification (501(c)(3), 501(c)(4), etc.)
- Tax ID uniqueness enforcement across all organizations
- Mission statement and description management
- Compliance documentation support

### Campaign Association Management
- One-to-many relationship with campaigns
- Campaign lifecycle management per organization
- Performance tracking across multiple campaigns
- Consolidated reporting for organization-wide activities

### Donation Tracking and Analytics
- Real-time donation tracking by organization
- Total donation amount calculations
- Donation count analytics
- Historical donation trend analysis
- Integration with donation processing systems

### Administrative Features
- Comprehensive organization listing with filtering
- Active/inactive status management
- Bulk operations for organization administration
- Advanced search capabilities across all organization data
- Audit trail tracking for all administrative actions

## Database Schema

### Organization Entity
```sql
- Id: Primary key (inherited from BaseEntity)
- LegalName: VARCHAR(200) - Official legal name of organization
- TaxId: VARCHAR(50) - Tax ID/EIN (unique constraint)
- Website: VARCHAR(255) - Organization website URL
- EmailAddress: VARCHAR(100) - Primary contact email
- Phone: VARCHAR(20) - Primary contact phone number
- MissionStatement: VARCHAR(1000) - Organization mission statement
- ShortDescription: VARCHAR(500) - Brief organization description
- OrganizationType: VARCHAR(50) - Type of non-profit (501c3, etc.)
- LogoUrl: VARCHAR(255) - URL to organization logo
- Street: VARCHAR(200) - Street address
- City: VARCHAR(100) - City
- State: VARCHAR(100) - State/province
- PostalCode: VARCHAR(20) - Postal/ZIP code
- Country: VARCHAR(100) - Country
- Standard audit fields: CreatedOn, CreatedBy, ModifiedOn, ModifiedBy, IsActive
```

### Entity Relationships
```sql
- Campaigns: One-to-many relationship with Campaign entities
- Donations: One-to-many relationship with Donation entities
```

## Service Integration

### Campaign Service Integration
- Seamless campaign creation and management per organization
- Campaign performance aggregation at organization level
- Multi-campaign fundraising coordination

### Donation Service Integration
- Real-time donation attribution to organizations
- Donation processing with organization context
- Financial reporting and analytics integration

### User Service Integration
- Organization admin user management
- Permission-based access control for organization data
- User role assignment within organization context

### Message Service Integration
- Automated notifications for organization milestones
- Donation acknowledgment processing
- Campaign status updates and communications

## Service Operations

### Core CRUD Operations
```csharp
// Get all organizations with optional inactive inclusion
Task<IEnumerable<Organization>> GetAllAsync(bool includeInactive = false)

// Get organizations with related data (campaigns, donations)
Task<IEnumerable<Organization>> GetAllWithRelatedDataAsync(bool includeInactive = false)

// Get organization by ID
Task<Organization?> GetByIdAsync(int id, bool includeInactive = false)

// Get organization by Tax ID (unique lookup)
Task<Organization?> GetByTaxIdAsync(string taxId, bool includeInactive = false)

// Get organization with all related data
Task<Organization?> GetWithRelatedDataAsync(int id, bool includeInactive = false)

// Create new organization with validation
Task<Organization> AddAsync(Organization organization, string createdBy = "OrganizationService")

// Update existing organization
Task<Organization> UpdateAsync(Organization organization, string modifiedBy = "OrganizationService")

// Set active status (soft delete/restore)
Task<bool> SetActiveStatusAsync(int id, bool isActive, string modifiedBy = "OrganizationService")
```

### Business Logic Features
- Tax ID uniqueness validation across all organizations
- Comprehensive data validation for organization information
- Related data loading for performance optimization
- Status management with audit trail support

## Usage Examples

### Basic Organization Operations
```csharp
// Create new organization
var organization = new Organization
{
    LegalName = "Make Sure It Happens Foundation",
    TaxId = "12-3456789",
    EmailAddress = "contact@msih.org",
    Phone = "(555) 123-4567",
    Website = "https://msih.org",
    OrganizationType = "501(c)(3)",
    MissionStatement = "Empowering communities through technology and fundraising.",
    Street = "123 Main Street",
    City = "Anytown",
    State = "CA",
    PostalCode = "12345",
    Country = "United States"
};

var createdOrg = await _organizationService.AddAsync(organization, "AdminUser");

// Get organization with all related data
var orgWithData = await _organizationService.GetWithRelatedDataAsync(orgId);

// Update organization information
orgWithData.EmailAddress = "newemail@msih.org";
await _organizationService.UpdateAsync(orgWithData, "AdminUser");

// Deactivate organization
await _organizationService.SetActiveStatusAsync(orgId, false, "AdminUser");
```

### Advanced Querying
```csharp
// Get all active organizations with campaigns and donations
var orgsWithData = await _organizationService.GetAllWithRelatedDataAsync(includeInactive: false);

// Find organization by Tax ID
var orgByTaxId = await _organizationService.GetByTaxIdAsync("12-3456789");

// Validate Tax ID uniqueness before creation
var existingOrg = await _organizationService.GetByTaxIdAsync(newOrg.TaxId, includeInactive: true);
if (existingOrg != null)
{
    throw new InvalidOperationException("Organization with this Tax ID already exists");
}
```

## Client Integration

### Admin Management Interface
- OrganizationManager.razor provides comprehensive admin interface
- OrganizationEditor.razor for detailed organization editing
- OrganizationDetail.razor component for displaying organization information
- Real-time status management and bulk operations

### Public Organization Display
- Integration with campaign pages to show organization information
- Donation pages display organization details and mission statement
- Public organization profile pages with campaign listings

## Testing and Quality Assurance

### Automated Testing
- OrganizationServiceTests.cs provides comprehensive test coverage
- Unit tests for all CRUD operations
- Business logic validation testing
- Integration testing with related services

### Key Testing Areas
- Tax ID uniqueness validation
- Organization data validation and constraints
- Related data loading and performance
- Status management and audit trails
- Integration with campaign and donation services

## Data Seeding and Setup

### OrganizationDataSeeder
- Automated seeding of initial organization data
- Support for development and production environments
- Configurable organization templates and defaults
- Integration with application startup processes

### Setup Instructions
- SETUP-INSTRUCTIONS.md provides detailed setup guidance
- Database migration scripts for organization schema
- Configuration examples for various environments
- Integration testing setup and validation

## Files

### Server Components
```
Server/Features/OrganizationService/
├── Data/
│   ├── OrganizationDataSeeder.cs
│   ├── OrganizationDbContext.cs
│   └── create-migration.ps1
├── Interfaces/
│   ├── IOrganizationRepository.cs
│   └── IOrganizationService.cs
├── Models/
│   └── Organization.cs
├── Repositories/
│   └── OrganizationRepository.cs
├── Services/
│   └── OrganizationService.cs
├── Tests/
│   └── OrganizationServiceTests.cs
├── ServiceCollectionExtensions.cs
├── Program.cs.additions.txt
├── README.md
├── SETUP-INSTRUCTIONS.md
├── SETUP-INSTRUCTIONS.txt
└── CLAUDE.md
```

### Related Components
```
Server/Common/Data/ApplicationDbContext.cs (integration)
Client/Features/Admin/Pages/OrganizationManager.razor
Client/Features/Admin/Pages/OrganizationEditor.razor
Client/Features/Admin/Components/OrganizationDetail.razor
Shared/OrganizationService/Dtos/OrganizationDto.cs
```

## Security Considerations

### Data Protection
- Secure storage of sensitive organization information
- Access control for organization management operations
- Audit trail for all administrative actions
- Data validation and sanitization for all inputs

### Compliance Features
- Tax ID uniqueness enforcement for legal compliance
- Support for various non-profit organization types
- Audit trail maintenance for regulatory requirements
- Secure handling of financial and legal information

## Performance Optimization

### Query Optimization
- Efficient related data loading strategies
- Pagination support for large organization datasets
- Indexed queries for common lookup operations
- Caching strategies for frequently accessed organization data

### Scalability Features
- Support for large numbers of organizations
- Efficient bulk operations for administrative tasks
- Optimized database queries with proper indexing
- Integration with caching layers for improved performance

This feature serves as the foundational component for managing non-profit organizations within the MSIH Platform for Good, providing comprehensive organization lifecycle management while maintaining compliance and security standards.