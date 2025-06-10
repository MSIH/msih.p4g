using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

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
                .Build();            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<SmsDbContext>();
            optionsBuilder.UseMySql(
                connectionString, 
                new MySqlServerVersion(new Version(8, 0, 26)),
                mySqlOptions => mySqlOptions.MigrationsAssembly("msih.p4g")
            );

            return new SmsDbContext(optionsBuilder.Options);
        }
    }
}
