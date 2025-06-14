using System.Threading.Tasks;
using msih.p4g.Shared.Models;
using System.Collections.Generic;

namespace msih.p4g.Server.Features.Base.Settings.Interfaces
{
    /// <summary>
    /// Service for managing application settings (Email, SMS, etc.)
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// Gets a setting value by key, checking DB, then appsettings, then environment variables.
        /// </summary>
        Task<string?> GetValueAsync(string key);
        
        /// <summary>
        /// Sets or updates a setting value in the DB.
        /// </summary>
        Task SetValueAsync(string key, string? value, string modifiedBy = "System");
        
        /// <summary>
        /// Gets all settings from the DB as a dictionary.
        /// </summary>
        Task<IReadOnlyDictionary<string, string?>> GetAllAsync();
    }
}
