using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using msih.p4g.Server.Common.Data.Extensions;
using System;
using System.IO;

namespace msih.p4g.Server.Features.Base.UserService.Data
{
    /// <summary>
    /// Design-time factory for UserDbContext to support EF Core migrations
    /// </summary>
    public class UserDbContextFactory : IDesignTimeDbContextFactory<UserDbContext>
    {
        public UserDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
            DatabaseConfigurationHelper.ConfigureDbContextOptions(
                optionsBuilder,
                configuration,
                isDevelopment: Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development");

            return new UserDbContext(optionsBuilder.Options);
        }
    }
}
