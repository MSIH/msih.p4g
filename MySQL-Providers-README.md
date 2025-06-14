# MySQL Provider for EF Core 9

This application uses Oracle's official MySQL provider for Entity Framework Core 9:

## Provider Details

**MySql.EntityFrameworkCore** (Oracle's official provider)
- Oracle's official MySQL provider for EF Core
- Version: 9.0.3
- Directly maintained by Oracle
- Fully compatible with EF Core 9
- Supports all standard EF Core features

## Configuration

The application uses the Oracle MySQL provider by default for production environments. For local development, you can use SQLite by setting:
{
  "UseLocalSqlite": true
}
This is recommended for faster development cycles and eliminates the need for a local MySQL server.

## Local Development

When UseLocalSqlite is set to true, the application will use SQLite with the following connection string:
Data Source=msih_p4g.db
This is configured automatically in the DatabaseConfigurationHelper class.

## Migration Considerations

If you switch between SQLite and MySQL, you may need to regenerate your migrations, as each provider has its own SQL dialect and feature set.