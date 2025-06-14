using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using msih.p4g.Server.Common.Data.Extensions;

namespace msih.p4g.Server.Features.Base.SmsService.Data
{
    /// <summary>
    /// Factory for creating SmsDbContext instances during design-time operations such as migrations
    /// </summary>
    public class SmsDbContextFactory : IDesignTimeDbContextFactory<SmsDbContext>
    {
        public SmsDbContext CreateDbContext(string[] args)
        {
            // Build configuration from appsettings.json in the root directory
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            // For design-time we need to use the environment variable approach
            // since we don't have access to IHostEnvironment through DI
            bool isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

            // Use the centralized database configuration logic
            var optionsBuilder = new DbContextOptionsBuilder<SmsDbContext>();
            DatabaseConfigurationHelper.ConfigureDbContextOptions(
                optionsBuilder,
                configuration,
                isDevelopment);

            return new SmsDbContext(optionsBuilder.Options);
        }
    }
}