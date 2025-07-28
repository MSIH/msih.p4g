/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

using Microsoft.EntityFrameworkCore;
using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.RecurringDonationService.Interfaces;
using msih.p4g.Server.Features.RecurringDonationService.Models;
using msih.p4g.Shared.Models;

namespace msih.p4g.Server.Features.RecurringDonationService.Repositories
{
    /// <summary>
    /// Repository for managing recurring donations.
    /// </summary>
    public class RecurringDonationRepository : GenericRepository<RecurringDonation>, IRecurringDonationRepository
    {
        private readonly ApplicationDbContext _context;

        public RecurringDonationRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets recurring donations by donor ID.
        /// </summary>
        public async Task<IEnumerable<RecurringDonation>> GetByDonorIdAsync(int donorId)
        {
            return await _context.RecurringDonations
                .Include(rd => rd.Donor)
                .Include(rd => rd.Campaign)
                .Where(rd => rd.DonorId == donorId && rd.IsActive && !rd.IsDeleted)
                .OrderByDescending(rd => rd.CreatedOn)
                .ToListAsync();
        }

        /// <summary>
        /// Gets recurring donations that are due for processing.
        /// </summary>
        public async Task<IEnumerable<RecurringDonation>> GetDueForProcessingAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.RecurringDonations
                .Include(rd => rd.Donor)
                .Include(rd => rd.Campaign)
                .Where(rd => rd.Status == RecurringDonationStatus.Active &&
                           rd.NextProcessDate <= now &&
                           rd.IsActive &&
                           !rd.IsDeleted &&
                           (rd.EndDate == null || rd.EndDate > now))
                .OrderBy(rd => rd.NextProcessDate)
                .ToListAsync();
        }

        /// <summary>
        /// Gets recurring donations by status.
        /// </summary>
        public async Task<IEnumerable<RecurringDonation>> GetByStatusAsync(RecurringDonationStatus status)
        {
            return await _context.RecurringDonations
                .Include(rd => rd.Donor)
                .Include(rd => rd.Campaign)
                .Where(rd => rd.Status == status && rd.IsActive && !rd.IsDeleted)
                .OrderByDescending(rd => rd.CreatedOn)
                .ToListAsync();
        }

        /// <summary>
        /// Gets paginated recurring donations by donor ID.
        /// </summary>
        public async Task<PagedResult<RecurringDonation>> GetPagedByDonorIdAsync(int donorId, PaginationParameters parameters)
        {
            var query = _context.RecurringDonations
                .Include(rd => rd.Donor)
                .Include(rd => rd.Campaign)
                .Where(rd => rd.DonorId == donorId && rd.IsActive && !rd.IsDeleted);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(rd => rd.CreatedOn)
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return new PagedResult<RecurringDonation>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }

        /// <summary>
        /// Gets paginated recurring donations by status.
        /// </summary>
        public async Task<PagedResult<RecurringDonation>> GetPagedByStatusAsync(RecurringDonationStatus status, PaginationParameters parameters)
        {
            var query = _context.RecurringDonations
                .Include(rd => rd.Donor)
                .Include(rd => rd.Campaign)
                .Where(rd => rd.Status == status && rd.IsActive && !rd.IsDeleted);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(rd => rd.CreatedOn)
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return new PagedResult<RecurringDonation>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }

        /// <summary>
        /// Gets recurring donations by user email.
        /// </summary>
        public async Task<IEnumerable<RecurringDonation>> GetByUserEmailAsync(string email)
        {
            return await _context.RecurringDonations
                .Include(rd => rd.Donor)
                    .ThenInclude(d => d.User)
                .Include(rd => rd.Campaign)
                .Where(rd => rd.Donor.User.Email == email && rd.IsActive && !rd.IsDeleted)
                .OrderByDescending(rd => rd.CreatedOn)
                .ToListAsync();
        }

        /// <summary>
        /// Gets paginated recurring donations by user email.
        /// </summary>
        public async Task<PagedResult<RecurringDonation>> GetPagedByUserEmailAsync(string email, PaginationParameters parameters)
        {
            var query = _context.RecurringDonations
                .Include(rd => rd.Donor)
                    .ThenInclude(d => d.User)
                .Include(rd => rd.Campaign)
                .Where(rd => rd.Donor.User.Email == email && rd.IsActive && !rd.IsDeleted);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(rd => rd.CreatedOn)
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return new PagedResult<RecurringDonation>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }

        /// <summary>
        /// Updates the next process date for a recurring donation.
        /// </summary>
        public async Task<bool> UpdateNextProcessDateAsync(int id, DateTime nextProcessDate, string modifiedBy)
        {
            var recurringDonation = await GetByIdAsync(id);
            if (recurringDonation == null)
                return false;

            recurringDonation.NextProcessDate = nextProcessDate;
            recurringDonation.ModifiedBy = modifiedBy;
            recurringDonation.ModifiedOn = DateTime.UtcNow;

            await UpdateAsync(recurringDonation, modifiedBy);
            return true;
        }

        /// <summary>
        /// Updates the status of a recurring donation.
        /// </summary>
        public async Task<bool> UpdateStatusAsync(int id, RecurringDonationStatus status, string modifiedBy)
        {
            var recurringDonation = await GetByIdAsync(id);
            if (recurringDonation == null)
                return false;

            recurringDonation.Status = status;
            recurringDonation.ModifiedBy = modifiedBy;
            recurringDonation.ModifiedOn = DateTime.UtcNow;

            if (status == RecurringDonationStatus.Cancelled)
            {
                recurringDonation.CancelledDate = DateTime.UtcNow;
                recurringDonation.CancelledBy = modifiedBy;
            }

            await UpdateAsync(recurringDonation, modifiedBy);
            return true;
        }

        /// <summary>
        /// Increments the successful donations count.
        /// </summary>
        public async Task<bool> IncrementSuccessfulDonationsCountAsync(int id, string modifiedBy)
        {
            var recurringDonation = await GetByIdAsync(id);
            if (recurringDonation == null)
                return false;

            recurringDonation.SuccessfulDonationsCount++;
            recurringDonation.LastProcessedDate = DateTime.UtcNow;
            recurringDonation.ModifiedBy = modifiedBy;
            recurringDonation.ModifiedOn = DateTime.UtcNow;

            await UpdateAsync(recurringDonation, modifiedBy);
            return true;
        }

        /// <summary>
        /// Increments the failed attempts count.
        /// </summary>
        public async Task<bool> IncrementFailedAttemptsCountAsync(int id, string modifiedBy)
        {
            var recurringDonation = await GetByIdAsync(id);
            if (recurringDonation == null)
                return false;

            recurringDonation.FailedAttemptsCount++;
            recurringDonation.ModifiedBy = modifiedBy;
            recurringDonation.ModifiedOn = DateTime.UtcNow;

            await UpdateAsync(recurringDonation, modifiedBy);
            return true;
        }

        /// <summary>
        /// Gets all with includes for navigation properties.
        /// </summary>
        public override async Task<IEnumerable<RecurringDonation>> GetAllAsync()
        {
            return await _context.RecurringDonations
                .Include(rd => rd.Donor)
                .Include(rd => rd.Campaign)
                .Where(rd => rd.IsActive && !rd.IsDeleted)
                .OrderByDescending(rd => rd.CreatedOn)
                .ToListAsync();
        }

        /// <summary>
        /// Gets by ID with includes for navigation properties.
        /// </summary>
        public override async Task<RecurringDonation?> GetByIdAsync(int id)
        {
            return await _context.RecurringDonations
                .Include(rd => rd.Donor)
                .Include(rd => rd.Campaign)
                .FirstOrDefaultAsync(rd => rd.Id == id && rd.IsActive && !rd.IsDeleted);
        }
    }
}