# Generic Repository Pattern

This document explains the generic repository pattern implementation for the application.

## Overview

The generic repository pattern provides a standardized way to perform CRUD (Create, Read, Update, Delete) operations on database entities. Our implementation includes support for:

- Soft delete functionality
- Active/inactive status tracking
- Audit tracking (created by, modified by, timestamps)

## Core Components

### IAuditableEntity Interface

Located in `Shared/Models/IAuditableEntity.cs`, this interface defines properties for tracking entity status and auditing:

- `IsActive`: Indicates if the entity is active
- `IsDeleted`: Indicates if the entity has been soft-deleted
- `CreatedOn`: When the entity was created
- `CreatedBy`: Who created the entity
- `ModifiedOn`: When the entity was last modified
- `ModifiedBy`: Who last modified the entity

### BaseEntity Class

Located in `Shared/Models/BaseEntity.cs`, this abstract class implements `IAuditableEntity` and provides default values:

- `Id`: Primary key for the entity
- Default values for `IsActive` (true), `IsDeleted` (false), and `CreatedOn` (current UTC time)

### IGenericRepository Interface

Located in `Server/Common/Data/Repositories/IGenericRepository.cs`, this interface defines methods for:

- Getting all entities (with options to include inactive or deleted entities)
- Finding entities by predicate
- Getting an entity by ID
- Adding a new entity
- Updating an existing entity
- Soft deleting an entity
- Permanently deleting an entity
- Activating/deactivating an entity
- Getting only active, inactive, or deleted entities
- Restoring a soft-deleted entity
- Checking if an entity exists

### GenericRepository Implementation

Located in `Server/Common/Data/Repositories/GenericRepository.cs`, this class implements `IGenericRepository` and provides:

- Type parameters for the entity type and DbContext type
- Implementation of all CRUD operations with audit trail tracking
- Filtering for active/inactive and deleted/non-deleted entities

## Usage

### Making an Entity Use the Generic Repository

1. Inherit from `BaseEntity`:

```csharp
public class YourEntity : BaseEntity
{
    // Your entity properties here
}
```

2. Create a repository interface extending IGenericRepository:

```csharp
public interface IYourEntityRepository : IGenericRepository<YourEntity>
{
    // Add any custom methods specific to your entity
}
```

3. Create a repository implementation:

```csharp
public class YourEntityRepository : GenericRepository<YourEntity, YourDbContext>, IYourEntityRepository
{
    public YourEntityRepository(YourDbContext context) : base(context)
    {
    }
    
    // Implement any custom methods
}
```

4. Register the repository in dependency injection:

```csharp
services.AddScoped<IYourEntityRepository, YourEntityRepository>();
```

Or using the extension method:

```csharp
services.AddGenericRepository<YourEntity, YourDbContext>();
```

### Using the Repository

```csharp
public class YourService
{
    private readonly IYourEntityRepository _repository;
    
    public YourService(IYourEntityRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<YourEntity> GetEntityAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }
    
    public async Task<YourEntity> CreateEntityAsync(YourEntity entity, string createdBy)
    {
        return await _repository.AddAsync(entity, createdBy);
    }
    
    public async Task<bool> DeleteEntityAsync(int id, string deletedBy)
    {
        return await _repository.DeleteAsync(id, deletedBy);
    }
    
    // Other methods using repository
}
```

## Benefits

- Consistent handling of CRUD operations across the application
- Built-in audit trail for all entities
- Simplified querying with standard filtering for active/inactive and deleted entities
- Code reuse through generic implementation
- Separation of concerns between data access and business logic
- Easier unit testing through abstraction

## Database Considerations

The `DbContext` should override `SaveChanges` and `SaveChangesAsync` to automatically set audit properties when entities are added or modified. See the `SmsDbContext` implementation for an example.
