# Organization Service Setup

This file provides step-by-step instructions for integrating the Organization Service into your application.

## 1. Update the ApplicationDbContext.cs file

Add the DbSet for Organization entity to the ApplicationDbContext class:

```csharp
// Add this DbSet property to the ApplicationDbContext class
public DbSet<Organization> Organizations { get; set; }
```

## 2. Call the configuration method in OnModelCreating

Add the following code to the OnModelCreating method in ApplicationDbContext.cs:

```csharp
// In the OnModelCreating method, add:
modelBuilder.ConfigureOrganizationEntity();
```

You'll need to add this after the existing code that configures other entities.

## 3. Register services in Program.cs

Add the following code to Program.cs to register the Organization service and its dependencies:

```csharp
// Register OrganizationRepository and OrganizationService for DI
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
```

## 4. Register the data seeder in Program.cs

Add the following code to Program.cs to register the OrganizationDataSeeder:

```csharp
// Register the data seeder
builder.Services.AddScoped<OrganizationDataSeeder>();
```

## 5. Seed initial data

In the Program.cs file, in the section where migrations are applied, add:

```csharp
// After other initialization code
var organizationSeeder = scope.ServiceProvider.GetRequiredService<OrganizationDataSeeder>();
await organizationSeeder.SeedAsync();
```

## 6. Update project references (if needed)

Make sure your project references include xUnit if you want to use the unit tests:

```xml
<PackageReference Include="xunit" Version="2.4.2" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.4.5" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="9.0.0" />
```

## 7. Create a migration

Run the following command to create a database migration:

```powershell
dotnet ef migrations add AddOrganization --context ApplicationDbContext --output-dir Server/Features/OrganizationService/Data/Migrations
```

This will create the necessary migration files to add the Organization entity to your database.
