/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.RecurringDonationService.Models;
using msih.p4g.Shared.Models;

namespace msih.p4g.Server.Features.RecurringDonationService.Interfaces
{
    /// <summary>
    /// Repository interface for recurring donations.
    /// </summary>
    public interface IRecurringDonationRepository : IGenericRepository<RecurringDonation>
    {
        /// <summary>
        /// Gets recurring donations by donor ID.
        /// </summary>
        Task<IEnumerable<RecurringDonation>> GetByDonorIdAsync(int donorId);

        /// <summary>
        /// Gets recurring donations that are due for processing.
        /// </summary>
        Task<IEnumerable<RecurringDonation>> GetDueForProcessingAsync();

        /// <summary>
        /// Gets recurring donations by status.
        /// </summary>
        Task<IEnumerable<RecurringDonation>> GetByStatusAsync(RecurringDonationStatus status);

        /// <summary>
        /// Gets paginated recurring donations by donor ID.
        /// </summary>
        Task<PagedResult<RecurringDonation>> GetPagedByDonorIdAsync(int donorId, PaginationParameters parameters);

        /// <summary>
        /// Gets paginated recurring donations by status.
        /// </summary>
        Task<PagedResult<RecurringDonation>> GetPagedByStatusAsync(RecurringDonationStatus status, PaginationParameters parameters);

        /// <summary>
        /// Gets recurring donations by user email.
        /// </summary>
        Task<IEnumerable<RecurringDonation>> GetByUserEmailAsync(string email);

        /// <summary>
        /// Gets paginated recurring donations by user email.
        /// </summary>
        Task<PagedResult<RecurringDonation>> GetPagedByUserEmailAsync(string email, PaginationParameters parameters);

        /// <summary>
        /// Updates the next process date for a recurring donation.
        /// </summary>
        Task<bool> UpdateNextProcessDateAsync(int id, DateTime nextProcessDate, string modifiedBy);

        /// <summary>
        /// Updates the status of a recurring donation.
        /// </summary>
        Task<bool> UpdateStatusAsync(int id, RecurringDonationStatus status, string modifiedBy);

        /// <summary>
        /// Increments the successful donations count.
        /// </summary>
        Task<bool> IncrementSuccessfulDonationsCountAsync(int id, string modifiedBy);

        /// <summary>
        /// Increments the failed attempts count.
        /// </summary>
        Task<bool> IncrementFailedAttemptsCountAsync(int id, string modifiedBy);
    }
}