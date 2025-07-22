using MSIH.Core.Common.Data.Repositories;
using MSIH.Core.Common.Models;
using MSIH.Core.Services.Profile.Model;

namespace MSIH.Core.Services.Profile.Interfaces
{
    public interface IProfileRepository : IGenericRepository<MSIH.Core.Services.Profile.Model.Profile>
    {
        /// <summary>
        /// Gets all profiles with User navigation properties included
        /// </summary>
        /// <returns>A list of profiles with related data</returns>
        Task<List<MSIH.Core.Services.Profile.Model.Profile>> GetAllWithUserDataAsync();

        /// <summary>
        /// Gets paginated profiles with User navigation properties included
        /// </summary>
        /// <param name="paginationParameters">Pagination and search parameters</param>
        /// <returns>A paginated result of profiles with related data</returns>
        Task<PagedResult<MSIH.Core.Services.Profile.Model.Profile>> GetPaginatedWithUserDataAsync(PaginationParameters paginationParameters);
    }
}
