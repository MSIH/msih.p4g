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
            // Determine if we should use SQLite based on configuration
            var useLocalDb = hostEnvironment.IsDevelopment() && configuration.GetValue<bool>("UseLocalSqlite", false);


            if (useLocalDb)
            {
                // Use SQLite for local development
                var connectionString = configuration.GetConnectionString("SqliteConnection") ?? "Data Source=msih_p4g.db";
                services.AddDbContext<TContext>(options =>
                    options.UseSqlite(
                        connectionString,
                        sqliteOptions => sqliteOptions.MigrationsAssembly("msih.p4g")
                    ));
            }
            else
            {
                // Use MySQL for production or when explicitly configured
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                services.AddDbContext<TContext>(options =>
                    options.UseMySql(
                        connectionString,
                        new MySqlServerVersion(new Version(8, 0, 26)),
                        mySqlOptions => mySqlOptions.MigrationsAssembly("msih.p4g")
                    ));
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
            // Determine if we should use SQLite based on configuration
            var useLocalDb = hostEnvironment.IsDevelopment() && configuration.GetValue<bool>("UseLocalSqlite", false);

            if (useLocalDb)
            {
                // Use SQLite for local development
                var connectionString = configuration.GetConnectionString("SqliteConnection") ?? "Data Source=msih_p4g.db";
                optionsBuilder.UseSqlite(
                    connectionString,
                    sqliteOptions => sqliteOptions.MigrationsAssembly("msih.p4g")
                );
            }
            else
            {
                // Use MySQL for production or when explicitly configured
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                optionsBuilder.UseMySql(
                    connectionString,
                    new MySqlServerVersion(new Version(8, 0, 26)),
                    mySqlOptions => mySqlOptions.MigrationsAssembly("msih.p4g")
                );
            }
        }

        // For design-time factory where IHostEnvironment is not available
        public static void ConfigureDbContextOptions<TContext>(
            DbContextOptionsBuilder<TContext> optionsBuilder,
            IConfiguration configuration,
            bool isDevelopment)

            where TContext : DbContext
        {
            // Determine if we should use SQLite based on configuration
            var useLocalDb = isDevelopment && configuration.GetValue<bool>("UseLocalSqlite", false);

            if (useLocalDb)
            {
                // Use SQLite for local development
                var connectionString = configuration.GetConnectionString("SqliteConnection") ?? "Data Source=msih_p4g.db";
                optionsBuilder.UseSqlite(
                    connectionString,
                    sqliteOptions => sqliteOptions.MigrationsAssembly("msih.p4g")
                );
            }
            else
            {
                // Use MySQL for production or when explicitly configured
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                optionsBuilder.UseMySql(
                    connectionString,
                    new MySqlServerVersion(new Version(8, 0, 26)),
                    mySqlOptions => mySqlOptions.MigrationsAssembly("msih.p4g")
                );
            }
        }
    }
}