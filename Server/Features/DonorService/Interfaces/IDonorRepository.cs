/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.DonorService.Model;
using msih.p4g.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace msih.p4g.Server.Features.DonorService.Interfaces
{
    /// <summary>
    /// Repository interface for Donor entity
    /// </summary>
    public interface IDonorRepository : IGenericRepository<Donor>
    {
        /// <summary>
        /// Searches for donors based on the provided search term
        /// </summary>
        /// <param name="searchTerm">The term to search for in PaymentProcessorDonorId</param>
        /// <returns>A list of donors matching the search term</returns>
        Task<List<Donor>> SearchAsync(string searchTerm);

        /// <summary>
        /// Gets all donors with User and Profile navigation properties included
        /// </summary>
        /// <returns>A list of donors with related data</returns>
        Task<List<Donor>> GetAllWithUserDataAsync();

        /// <summary>
        /// Gets paginated donors with User and Profile navigation properties included
        /// </summary>
        /// <param name="paginationParameters">Pagination and search parameters</param>
        /// <returns>A paginated result of donors with related data</returns>
        Task<PagedResult<Donor>> GetPaginatedWithUserDataAsync(PaginationParameters paginationParameters);
    }
}
