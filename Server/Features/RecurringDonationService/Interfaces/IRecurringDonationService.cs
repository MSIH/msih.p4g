/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

using msih.p4g.Server.Features.RecurringDonationService.Models;
using msih.p4g.Shared.Models;

namespace msih.p4g.Server.Features.RecurringDonationService.Interfaces
{
    /// <summary>
    /// Interface for managing recurring donations.
    /// </summary>
    public interface IRecurringDonationService
    {
        /// <summary>
        /// Creates a new recurring donation subscription.
        /// </summary>
        Task<RecurringDonation> CreateRecurringDonationAsync(RecurringDonation recurringDonation, string createdBy = "RecurringDonationService");

        /// <summary>
        /// Gets a recurring donation by ID.
        /// </summary>
        Task<RecurringDonation?> GetByIdAsync(int id);

        /// <summary>
        /// Gets all recurring donations for a specific donor.
        /// </summary>
        Task<List<RecurringDonation>> GetByDonorIdAsync(int donorId);

        /// <summary>
        /// Gets recurring donations that are due for processing.
        /// </summary>
        Task<List<RecurringDonation>> GetDueForProcessingAsync();

        /// <summary>
        /// Gets recurring donations by status.
        /// </summary>
        Task<List<RecurringDonation>> GetByStatusAsync(RecurringDonationStatus status);

        /// <summary>
        /// Processes a single recurring donation.
        /// </summary>
        Task<bool> ProcessRecurringDonationAsync(int recurringDonationId);

        /// <summary>
        /// Processes all recurring donations that are due.
        /// </summary>
        Task<int> ProcessDueRecurringDonationsAsync();

        /// <summary>
        /// Updates a recurring donation.
        /// </summary>
        Task<bool> UpdateRecurringDonationAsync(RecurringDonation recurringDonation, string modifiedBy = "RecurringDonationService");

        /// <summary>
        /// Cancels a recurring donation.
        /// </summary>
        Task<bool> CancelRecurringDonationAsync(int id, string cancelledBy, string? reason = null);

        /// <summary>
        /// Pauses a recurring donation.
        /// </summary>
        Task<bool> PauseRecurringDonationAsync(int id, string modifiedBy);

        /// <summary>
        /// Resumes a paused recurring donation.
        /// </summary>
        Task<bool> ResumeRecurringDonationAsync(int id, string modifiedBy);

        /// <summary>
        /// Updates the amount for a recurring donation.
        /// </summary>
        Task<bool> UpdateAmountAsync(int id, decimal newAmount, string modifiedBy);

        /// <summary>
        /// Updates the payment method for a recurring donation.
        /// </summary>
        Task<bool> UpdatePaymentMethodAsync(int id, string newPaymentMethodToken, string modifiedBy);

        /// <summary>
        /// Gets paginated recurring donations for a donor.
        /// </summary>
        Task<PagedResult<RecurringDonation>> GetPagedByDonorIdAsync(int donorId, PaginationParameters parameters);

        /// <summary>
        /// Gets paginated recurring donations by status.
        /// </summary>
        Task<PagedResult<RecurringDonation>> GetPagedByStatusAsync(RecurringDonationStatus status, PaginationParameters parameters);

        /// <summary>
        /// Gets recurring donations by user email.
        /// </summary>
        Task<List<RecurringDonation>> GetByUserEmailAsync(string email);

        /// <summary>
        /// Gets paginated recurring donations by user email.
        /// </summary>
        Task<PagedResult<RecurringDonation>> GetPagedByUserEmailAsync(string email, PaginationParameters parameters);
    }
}