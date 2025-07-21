using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.Base.ProfileService.Model;
using msih.p4g.Shared.Models;

namespace msih.p4g.Server.Features.Base.ProfileService.Interfaces
{
    public interface IProfileRepository : IGenericRepository<Profile>
    {
        /// <summary>
        /// Gets all profiles with User navigation properties included
        /// </summary>
        /// <returns>A list of profiles with related data</returns>
        Task<List<Profile>> GetAllWithUserDataAsync();

        /// <summary>
        /// Gets paginated profiles with User navigation properties included
        /// </summary>
        /// <param name="paginationParameters">Pagination and search parameters</param>
        /// <returns>A paginated result of profiles with related data</returns>
        Task<PagedResult<Profile>> GetPaginatedWithUserDataAsync(PaginationParameters paginationParameters);
    }
}
