/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.EntityFrameworkCore;

namespace msih.p4g.Server.Common.Data.Extensions
{
    /// <summary>
    /// Helper class for database configuration to centralize provider selection logic
    /// </summary>
    public static class DatabaseConfigurationHelper
    {
        /// <summary>
        /// Configures database provider selection based on environment and configuration
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <param name="hostEnvironment">The hosting environment</param>
        /// <typeparam name="TContext">The DbContext type</typeparam>
        public static void AddConfiguredDbContext<TContext>(
            IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment hostEnvironment)
            where TContext : DbContext
        {
            var databaseProvider = GetDatabaseProvider(configuration, hostEnvironment);

            switch (databaseProvider)
            {
                case DatabaseProvider.SQLite:
                    AddSqliteDbContext<TContext>(services, configuration);
                    break;
                case DatabaseProvider.SqlServer:
                    AddSqlServerDbContext<TContext>(services, configuration);
                    break;
                case DatabaseProvider.MySQL:
                default:
                    AddMySqlDbContext<TContext>(services, configuration);
                    break;
            }
        }

        /// <summary>
        /// Configures DbContextOptions for a specific context type based on environment and configuration
        /// </summary>
        /// <typeparam name="TContext">The DbContext type</typeparam>
        /// <param name="optionsBuilder">The DbContextOptionsBuilder</param>
        /// <param name="configuration">The configuration</param>
        /// <param name="hostEnvironment">The hosting environment</param>
        public static void ConfigureDbContextOptions<TContext>(
            DbContextOptionsBuilder<TContext> optionsBuilder,
            IConfiguration configuration,
            IHostEnvironment hostEnvironment)
            where TContext : DbContext
        {
            var databaseProvider = GetDatabaseProvider(configuration, hostEnvironment);

            switch (databaseProvider)
            {
                case DatabaseProvider.SQLite:
                    ConfigureSqliteOptions(optionsBuilder, configuration);
                    break;
                case DatabaseProvider.SqlServer:
                    ConfigureSqlServerOptions(optionsBuilder, configuration);
                    break;
                case DatabaseProvider.MySQL:
                default:
                    ConfigureMySqlOptions(optionsBuilder, configuration);
                    break;
            }
        }

        // For design-time factory where IHostEnvironment is not available
        public static void ConfigureDbContextOptions<TContext>(
            DbContextOptionsBuilder<TContext> optionsBuilder,
            IConfiguration configuration,
            bool isDevelopment)
            where TContext : DbContext
        {
            var databaseProvider = GetDatabaseProvider(configuration, isDevelopment);

            switch (databaseProvider)
            {
                case DatabaseProvider.SQLite:
                    ConfigureSqliteOptions(optionsBuilder, configuration);
                    break;
                case DatabaseProvider.SqlServer:
                    ConfigureSqlServerOptions(optionsBuilder, configuration);
                    break;
                case DatabaseProvider.MySQL:
                default:
                    ConfigureMySqlOptions(optionsBuilder, configuration);
                    break;
            }
        }

        private static DatabaseProvider GetDatabaseProvider(IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            return GetDatabaseProvider(configuration, hostEnvironment.IsDevelopment());
        }

        private static DatabaseProvider GetDatabaseProvider(IConfiguration configuration, bool isDevelopment)
        {
            // Check for SQLite first (local development)
            if (isDevelopment && configuration.GetValue<bool>("UseLocalSqlite", false))
            {
                return DatabaseProvider.SQLite;
            }

            // Check for SQL Server
            if (configuration.GetValue<bool>("UseSqlServer", false))
            {
                return DatabaseProvider.SqlServer;
            }

            // Default to MySQL
            return DatabaseProvider.MySQL;
        }

        private static void AddSqliteDbContext<TContext>(IServiceCollection services, IConfiguration configuration)
            where TContext : DbContext
        {
            var connectionString = configuration.GetConnectionString("SqliteConnection") ?? "Data Source=msih_p4g.db";
            services.AddDbContextFactory<TContext>(options =>
                options.UseSqlite(
                    connectionString,
                    sqliteOptions => sqliteOptions.MigrationsAssembly("msih.p4g")
                ));
        }

        private static void AddSqlServerDbContext<TContext>(IServiceCollection services, IConfiguration configuration)
            where TContext : DbContext
        {
            var connectionString = configuration.GetConnectionString("SqlServerConnection")
                ?? throw new InvalidOperationException("SQL Server connection string 'SqlServerConnection' is not configured.");

            services.AddDbContextFactory<TContext>(options =>
                options.UseSqlServer(
                    connectionString,
                    sqlServerOptions => sqlServerOptions.MigrationsAssembly("msih.p4g")
                ));
        }

        private static void AddMySqlDbContext<TContext>(IServiceCollection services, IConfiguration configuration)
            where TContext : DbContext
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("MySQL connection string 'DefaultConnection' is not configured.");

            services.AddDbContextFactory<TContext>(options =>
                options.UseMySQL(
                    connectionString,
                    mySqlOptions => mySqlOptions.MigrationsAssembly("msih.p4g")
                ));
        }

        private static void ConfigureSqliteOptions<TContext>(DbContextOptionsBuilder<TContext> optionsBuilder, IConfiguration configuration)
            where TContext : DbContext
        {
            var connectionString = configuration.GetConnectionString("SqliteConnection") ?? "Data Source=msih_p4g.db";
            optionsBuilder.UseSqlite(
                connectionString,
                sqliteOptions => sqliteOptions.MigrationsAssembly("msih.p4g")
            );
        }

        private static void ConfigureSqlServerOptions<TContext>(DbContextOptionsBuilder<TContext> optionsBuilder, IConfiguration configuration)
            where TContext : DbContext
        {
            var connectionString = configuration.GetConnectionString("SqlServerConnection")
                ?? throw new InvalidOperationException("SQL Server connection string 'SqlServerConnection' is not configured.");

            optionsBuilder.UseSqlServer(
                connectionString,
                sqlServerOptions => sqlServerOptions.MigrationsAssembly("msih.p4g")
            );
        }

        private static void ConfigureMySqlOptions<TContext>(DbContextOptionsBuilder<TContext> optionsBuilder, IConfiguration configuration)
            where TContext : DbContext
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("MySQL connection string 'DefaultConnection' is not configured.");

            optionsBuilder.UseMySQL(
                connectionString,
                mySqlOptions => mySqlOptions.MigrationsAssembly("msih.p4g")
            );
        }
    }

    /// <summary>
    /// Enumeration of supported database providers
    /// </summary>
    public enum DatabaseProvider
    {
        MySQL,
        SqlServer,
        SQLite
    }
}
