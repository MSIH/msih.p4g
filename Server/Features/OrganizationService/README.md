# Organization Service

The Organization Service manages non-profit organization data in the Platform for Good application. It provides a complete set of CRUD operations for organizations and maintains relationships with campaigns and donations.

## Features

- Full CRUD operations for organizations
- Relationship management with campaigns and donations
- Data validation
- Automatic tracking of creation and modification timestamps
- Active/inactive status management

## Key Components

### Server-Side

- **Models**: Organization entity class with properties for non-profit organization data
- **Repositories**: Data access layer for Organization entity
- **Services**: Business logic for Organization operations
- **Data Configuration**: Entity Framework Core configuration for Organization entity

### Client-Side

- **DTOs**: Data transfer objects for Organization entity
- **Components**: Organization detail component for displaying organization information
- **Pages**: Admin pages for managing organizations:
  - Organization Manager: Lists all organizations
  - Organization Editor: Add/edit organization information

## Data Model

The Organization entity includes:
- Basic information (name, tax ID, website, contact details)
- Address information (street, city, state, etc.)
- Mission statement and description
- Relationships with campaigns and donations

## Setup Instructions

See the detailed [setup instructions](SETUP-INSTRUCTIONS.md) for step-by-step guidance on integrating the Organization service into your application.

## Usage Examples
// Inject the organization service
@inject IOrganizationService OrganizationService

// Get an organization by ID
var organization = await OrganizationService.GetByIdAsync(id);

// Get an organization with its related data
var organizationWithRelatedData = await OrganizationService.GetWithRelatedDataAsync(id);

// Add a new organization
var newOrg = new Organization
{
    LegalName = "Example Non-Profit",
    TaxId = "12-3456789",
    EmailAddress = "contact@example.org",
    // Set other properties...
};
await OrganizationService.AddAsync(newOrg);

// Update an organization
organization.Website = "https://www.example.org";
await OrganizationService.UpdateAsync(organization);

// Deactivate an organization
await OrganizationService.SetActiveStatusAsync(id, false);
