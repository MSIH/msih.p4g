# Generic Repository Pattern

This document explains the generic repository pattern implementation for the application.

## Overview

The generic repository pattern provides a standardized way to perform CRUD (Create, Read, Update, Delete) operations on database entities. Our implementation includes support for:

- Active/inactive status tracking
- Audit tracking (created by, modified by, timestamps)

## Core Components

### IAuditableEntity Interface

Located in `Server/Common/Models/IAuditableEntity.cs`, this interface defines properties for tracking entity status and auditing:

- `IsActive`: Indicates if the entity is active
- `CreatedOn`: When the entity was created
- `CreatedBy`: Who created the entity
- `ModifiedOn`: When the entity was last modified
- `ModifiedBy`: Who last modified the entity

### BaseEntity Class

Located in `Server/Common/Models/BaseEntity.cs`, this abstract class implements `IAuditableEntity` and provides default values:

- `Id`: Primary key for the entity
- Default values for `IsActive` (true) and `CreatedOn` (current UTC time)

### IGenericRepository Interface

Located in `Server/Common/Data/Repositories/IGenericRepository.cs`, this interface defines methods for:

- Getting all entities (with options to include inactive entities)
- Finding entities by predicate
- Getting an entity by ID
- Adding a new entity
- Updating an existing entity
- Permanently deleting an entity
- Activating/deactivating an entity
- Getting only active or inactive entities
- Checking if an entity exists

### GenericRepository Implementation

Located in `Server/Common/Data/Repositories/GenericRepository.cs`, this class implements `IGenericRepository` and provides:

- Type parameters for the entity type and DbContext type
- Implementation of all CRUD operations with audit trail tracking
- Filtering for active/inactive entities

## Usage

### Making an Entity Use the Generic Repository

1. Inherit from `BaseEntity`:
public class YourEntity : BaseEntity
{
    // Your entity properties here
}
2. Create a repository interface extending IGenericRepository:
public interface IYourEntityRepository : IGenericRepository<YourEntity>
{
    // Add any custom methods specific to your entity
}
3. Create a repository implementation:
public class YourEntityRepository : GenericRepository<YourEntity, YourDbContext>, IYourEntityRepository
{
    public YourEntityRepository(YourDbContext context) : base(context)
    {
    }
    
    // Implement any custom methods
}
4. Register the repository in dependency injection:
services.AddScoped<IYourEntityRepository, YourEntityRepository>();
Or using the extension method:
services.AddGenericRepository<YourEntity, YourDbContext>();
### Using the Repository

The generic repository provides several methods for interacting with entities:
// Get all active entities
var activeEntities = await repository.GetAllAsync();

// Get all entities including inactive ones
var allEntities = await repository.GetAllAsync(includeInactive: true);

// Get entity by ID
var entity = await repository.GetByIdAsync(id);

// Add a new entity
var newEntity = await repository.AddAsync(entity, "UserName");

// Update an entity
var updatedEntity = await repository.UpdateAsync(entity, "UserName");

// Delete an entity
var isDeleted = await repository.DeleteAsync(id, "UserName");

// Toggle active status
var isToggled = await repository.SetActiveStatusAsync(id, false, "UserName");

// Get only active entities
var onlyActive = await repository.GetActiveOnlyAsync();

// Get only inactive entities
var onlyInactive = await repository.GetInactiveOnlyAsync();
## Benefits

- Consistent handling of CRUD operations across the application
- Built-in audit trail for all entities
- Simplified querying with standard filtering for active/inactive entities
- Code reuse through generic implementation
- Separation of concerns between data access and business logic
- Easier unit testing through abstraction

## Database Considerations

The `DbContext` should override `SaveChanges` and `SaveChangesAsync` to automatically set audit properties when entities are added or modified. See the `SmsDbContext` implementation for an example.
