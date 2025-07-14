// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

using System.Reflection;
using System.Text.Json;

namespace msih.p4g.Server.Common.Utilities
{
    /// <summary>
    /// Helper class to access version information at runtime for Azure Web App deployment
    /// Supports dual version system: Numeric (.NET compliant) and Semantic (human-readable)
    /// </summary>
    public static class VersionHelper
    {
        private static readonly Assembly _assembly = Assembly.GetExecutingAssembly();
        private static BuildInfo? _cachedBuildInfo;

        /// <summary>
        /// Gets the .NET numeric version (e.g., "7.7.14.1234") - MSBuild generated
        /// This is the version that .NET assemblies actually use (numeric only)
        /// </summary>
        public static string Version => _assembly.GetName().Version?.ToString() ?? "Unknown";

        /// <summary>
        /// Gets the semantic version with git hash (e.g., "7.7.14.a1b2c3d") from InformationalVersion
        /// This is the human-readable version for display purposes
        /// </summary>
        public static string SemanticVersion
        {
            get
            {
                var infoVersion = InformationalVersion;
                var plusIndex = infoVersion.IndexOf('+');
                if (plusIndex > 0)
                {
                    return infoVersion.Substring(0, plusIndex);
                }
                return infoVersion;
            }
        }

        /// <summary>
        /// Gets the informational version which includes commit SHA
        /// Format: "7.7.14.a1b2c3d+abc123def456..." or "7.7.14.a1b2c3d+local"
        /// </summary>
        public static string InformationalVersion =>
            _assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "Unknown";

        /// <summary>
        /// Gets the file version
        /// </summary>
        public static string FileVersion =>
            _assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "Unknown";

        /// <summary>
        /// Gets just the build date from the semantic version (YearSum.M.D)
        /// </summary>
        public static string BuildDate
        {
            get
            {
                // Use semantic version for display purposes
                var version = SemanticVersion;
                var parts = version.Split('.');
                if (parts.Length >= 3)
                {
                    return $"{parts[0]}.{parts[1]}.{parts[2]}";
                }
                return "Unknown";
            }
        }

        /// <summary>
        /// Gets the year sum from the semantic version (sum of last two digits of year)
        /// </summary>
        public static string YearSum
        {
            get
            {
                var version = SemanticVersion;
                var parts = version.Split('.');
                if (parts.Length >= 1)
                {
                    return parts[0];
                }
                return "Unknown";
            }
        }

        /// <summary>
        /// Gets the month from the semantic version
        /// </summary>
        public static string Month
        {
            get
            {
                var version = SemanticVersion;
                var parts = version.Split('.');
                if (parts.Length >= 2)
                {
                    return parts[1];
                }
                return "Unknown";
            }
        }

        /// <summary>
        /// Gets the day from the semantic version
        /// </summary>
        public static string Day
        {
            get
            {
                var version = SemanticVersion;
                var parts = version.Split('.');
                if (parts.Length >= 3)
                {
                    return parts[2];
                }
                return "Unknown";
            }
        }

        /// <summary>
        /// Gets the commit hash from the semantic version (e.g., "a1b2c3d")
        /// </summary>
        public static string CommitHash
        {
            get
            {
                var version = SemanticVersion;
                var parts = version.Split('.');
                if (parts.Length >= 4)
                {
                    return parts[3];
                }
                return "Unknown";
            }
        }

        /// <summary>
        /// Gets the numeric commit number from the .NET version (for uniqueness)
        /// </summary>
        public static string CommitNumber
        {
            get
            {
                var version = Version;
                var parts = version.Split('.');
                if (parts.Length >= 4)
                {
                    return parts[3];
                }
                return "Unknown";
            }
        }

        /// <summary>
        /// Gets the commit SHA from the informational version
        /// </summary>
        public static string CommitSha
        {
            get
            {
                var infoVersion = InformationalVersion;
                var plusIndex = infoVersion.IndexOf('+');
                if (plusIndex > 0 && plusIndex < infoVersion.Length - 1)
                {
                    var sha = infoVersion.Substring(plusIndex + 1);
                    return sha == "local" ? "local-build" : sha;
                }
                return "Unknown";
            }
        }

        /// <summary>
        /// Indicates if this is a local development build
        /// </summary>
        public static bool IsLocalBuild => CommitSha == "local-build";

        /// <summary>
        /// Indicates if this is a CI/CD build
        /// </summary>
        public static bool IsCiBuild => !IsLocalBuild && CommitSha != "Unknown";

        /// <summary>
        /// Reads build information from build-info.json deployed with the application
        /// Created by MSBuild during publish
        /// </summary>
        public static async Task<BuildInfo?> GetBuildInfoAsync()
        {
            if (_cachedBuildInfo != null)
                return _cachedBuildInfo;

            try
            {
                var contentRoot = Path.GetDirectoryName(_assembly.Location) ?? Directory.GetCurrentDirectory();
                var buildInfoPath = Path.Combine(contentRoot, "build-info.json");

                if (!File.Exists(buildInfoPath))
                    return null;

                var json = await File.ReadAllTextAsync(buildInfoPath);
                _cachedBuildInfo = JsonSerializer.Deserialize<BuildInfo>(json, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                return _cachedBuildInfo;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a formatted version string for display
        /// Uses semantic version for human readability
        /// </summary>
        public static string GetDisplayVersion()
        {
            var buildType = IsLocalBuild ? "Local" : IsCiBuild ? "CI" : "Unknown";
            return $"v{SemanticVersion} ({buildType} Build: {BuildDate}, Commit: {CommitHash})";
        }

        /// <summary>
        /// Gets detailed version information
        /// </summary>
        public static string GetDetailedVersionInfo()
        {
            return $"""
            Semantic Version: {SemanticVersion}
            .NET Version: {Version}
            Build Date: {BuildDate}
            Year Sum: {YearSum}
            Month: {Month}
            Day: {Day}
            Commit Hash: {CommitHash}
            Commit Number: {CommitNumber}
            Full Commit SHA: {CommitSha}
            File Version: {FileVersion}
            Informational Version: {InformationalVersion}
            Build Type: {(IsLocalBuild ? "Local Development" : IsCiBuild ? "CI/CD Pipeline" : "Unknown")}
            """;
        }

        /// <summary>
        /// Gets Azure Web App deployment information
        /// </summary>
        public static async Task<string> GetAzureDeploymentInfoAsync()
        {
            var buildInfo = await GetBuildInfoAsync();
            if (buildInfo == null)
            {
                return $"""
                🌐 Azure Web App: dev-gd4-org
                {GetDetailedVersionInfo()}
                📋 Note: Running with MSBuild-generated version (build-info.json not available)
                """;
            }

            return $"""
            🌐 Azure Web App: {buildInfo.AzureAppName}
            🏷️  Semantic Version: {buildInfo.SemanticVersion}
            🏷️  .NET Version: {buildInfo.Version}
            📅 Build Date: {buildInfo.BuildDate}
            🔗 Commit: {buildInfo.CommitHash} (#{buildInfo.CommitNumber}) ({buildInfo.FullCommitSha ?? "local"})
            🌿 Branch: {buildInfo.Branch ?? "local"}
            🏃 Workflow Run: #{buildInfo.RunNumber}
            🔧 Environment: {buildInfo.Environment}
            🏗️  Build Machine: {buildInfo.BuildMachine ?? "Unknown"}
            👤 Build User: {buildInfo.BuildUser ?? "Unknown"}
            📦 MSBuild Version: {buildInfo.MsbuildVersion ?? "Unknown"}
            🎯 Target Framework: {buildInfo.TargetFramework ?? "Unknown"}
            """;
        }

        /// <summary>
        /// Gets a health check response with version information
        /// </summary>
        public static async Task<object> GetHealthCheckAsync()
        {
            var buildInfo = await GetBuildInfoAsync();

            return new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                semanticVersion = SemanticVersion,
                numericVersion = Version,
                displayVersion = GetDisplayVersion(),
                buildType = IsLocalBuild ? "local" : IsCiBuild ? "ci" : "unknown",
                yearSum = YearSum,
                month = Month,
                day = Day,
                commitHash = CommitHash,
                commitNumber = CommitNumber,
                azureAppName = "dev-gd4-org",
                environment = "development",
                msbuildGenerated = true,
                buildInfo = buildInfo
            };
        }
    }

    /// <summary>
    /// Build information from build-info.json (created by MSBuild during publish)
    /// </summary>
    public record BuildInfo(
        string Version,              // Numeric version (e.g., "7.7.14.1234")
        string SemanticVersion,      // Semantic version (e.g., "7.7.14.a1b2c3d")
        string? NumericVersion,      // Same as Version, for clarity
        string BuildDate,
        int FullYear,
        int YearSum,
        int Month,
        int Day,
        string CommitHash,           // Git commit hash (e.g., "a1b2c3d")
        int CommitNumber,            // Numeric commit count
        string? FullCommitSha,
        string? Branch,
        string? Workflow,
        int? RunNumber,
        string AzureAppName,
        string Environment,
        string? BuildMachine,
        string? BuildUser,
        string? MsbuildVersion,
        string? TargetFramework
    );

    // Example usage in your Azure Web App with MSBuild versioning
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            // Display version on Azure Web App startup
            Console.WriteLine($"🚀 Starting dev-gd4-org {VersionHelper.GetDisplayVersion()}");

            // Show version details
            Console.WriteLine($"📋 .NET Version: {VersionHelper.Version}");
            Console.WriteLine($"📋 Semantic Version: {VersionHelper.SemanticVersion}");

            // Show whether this is a local or CI build
            if (VersionHelper.IsLocalBuild)
            {
                Console.WriteLine("🏠 Running local development build");
            }
            else if (VersionHelper.IsCiBuild)
            {
                Console.WriteLine("🔄 Running CI/CD pipeline build");
            }

            // Log detailed Azure deployment info
            var azureInfo = await VersionHelper.GetAzureDeploymentInfoAsync();
            Console.WriteLine(azureInfo);

            // Your application code here...
        }
    }

    // Example Web API controller for Azure Web App
    /*
    [ApiController]
    [Route("api/[controller]")]
    public class VersionController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetVersion()
        {
            var buildInfo = await VersionHelper.GetBuildInfoAsync();

            return Ok(new 
            {
                semanticVersion = VersionHelper.SemanticVersion,  // "7.7.14.a1b2c3d" 
                numericVersion = VersionHelper.Version,           // "7.7.14.1234"
                displayVersion = VersionHelper.GetDisplayVersion(),
                buildType = VersionHelper.IsLocalBuild ? "local" : VersionHelper.IsCiBuild ? "ci" : "unknown",
                yearSum = VersionHelper.YearSum,
                month = VersionHelper.Month,
                day = VersionHelper.Day,
                commitHash = VersionHelper.CommitHash,            // "a1b2c3d"
                commitNumber = VersionHelper.CommitNumber,        // "1234"
                azureAppName = "dev-gd4-org",
                environment = "development",
                generatedBy = "MSBuild",
                buildInfo = buildInfo
            });
        }

        [HttpGet("health")]
        public async Task<IActionResult> GetHealth()
        {
            return Ok(await VersionHelper.GetHealthCheckAsync());
        }

        [HttpGet("detailed")]
        public async Task<IActionResult> GetDetailedInfo()
        {
            var azureInfo = await VersionHelper.GetAzureDeploymentInfoAsync();

            return Ok(new 
            {
                detailedInfo = azureInfo,
                versionDetails = VersionHelper.GetDetailedVersionInfo(),
                buildTimestamp = DateTime.UtcNow
            });
        }
    }
    */
}
