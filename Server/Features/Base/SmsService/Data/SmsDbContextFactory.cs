using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
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
            
            // Use the centralized database configuration logic
            var optionsBuilder = new DbContextOptionsBuilder<SmsDbContext>();
            DatabaseConfigurationHelper.ConfigureDbContextOptions(
                optionsBuilder, 
                configuration, 
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development");

            return new SmsDbContext(optionsBuilder.Options);
        }
    }
}
