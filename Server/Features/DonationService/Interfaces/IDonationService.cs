// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

using msih.p4g.Server.Features.DonationService.Models;
using msih.p4g.Shared.Models;

namespace msih.p4g.Server.Features.DonationService.Interfaces
{
    /// <summary>
    /// Interface for the Donation Service
    /// </summary>
    public interface IDonationService
    {
        /// <summary>
        /// Processes a donation request from the client.
        /// If PayTransactionFee is true in the request, the transaction fee will be added to the total charged amount.
        /// The donation record will include base amount, transaction fee amount, and total amount charged.
        /// </summary>
        Task<Donation> ProcessDonationAsync(DonationRequestDto dto);

        Task<DonorRegistrationResultType> ProcessDonorRegistrationAsync(DonationRequestDto dto);

        /// <summary>
        /// Gets all donations.
        /// </summary>
        Task<List<Donation>> GetAllAsync();

        /// <summary>
        /// Gets a donation by its ID.
        /// </summary>
        Task<Donation?> GetByIdAsync(int id);

        /// <summary>
        /// Gets donations by donor ID.
        /// </summary>
        Task<List<Donation>> GetByDonorIdAsync(int donorId);

        /// <summary>
        /// Gets donations for a specific user by email.
        /// </summary>
        Task<List<Donation>> GetByUserEmailAsync(string email);

        /// <summary>
        /// Gets donations by campaign ID.
        /// </summary>
        Task<List<Donation>> GetByCampaignIdAsync(int campaignId);

        /// <summary>
        /// Gets donations by campaign code.
        /// </summary>
        Task<List<Donation>> GetByCampaignCodeAsync(string campaignCode);

        /// <summary>
        /// Gets donations by referral code.
        /// </summary>
        Task<List<Donation>> GetByReferralCodeAsync(string referralCode);

        /// <summary>
        /// Searches for donations matching the specified search term.
        /// </summary>
        Task<List<Donation>> SearchAsync(string searchTerm);

        /// <summary>
        /// Searches for donations for a specific user by email.
        /// </summary>
        Task<List<Donation>> SearchByUserEmailAsync(string email, string searchTerm);

        /// <summary>
        /// Adds a new donation.
        /// Transaction fee and total amount charged will be automatically calculated if not already set.
        /// </summary>
        Task<Donation> AddAsync(Donation donation);

        /// <summary>
        /// Updates an existing donation.
        /// Transaction fee and total amount charged will be recalculated based on the updated amount.
        /// </summary>
        Task<bool> UpdateAsync(Donation donation);

        /// <summary>
        /// Updates a recurring donation for a specific user.
        /// </summary>
        Task<bool> UpdateRecurringDonationAsync(string userEmail, int donationId, decimal newAmount, bool isActive);

        /// <summary>
        /// Cancels a recurring donation for a specific user.
        /// </summary>
        Task<bool> CancelRecurringDonationAsync(string userEmail, int donationId);

        /// <summary>
        /// Sets the active status of a donation.
        /// </summary>
        Task<bool> SetActiveAsync(int id, bool isActive, string modifiedBy = "DonationService");

        /// <summary>
        /// Gets the total donation amount for a specific campaign.
        /// This returns the base donation amount (excluding transaction fees).
        /// </summary>
        Task<decimal> GetTotalAmountByCampaignIdAsync(int campaignId);

        /// <summary>
        /// Gets the total donation amount for a specific donor.
        /// This returns the base donation amount (excluding transaction fees).
        /// </summary>
        Task<decimal> GetTotalAmountByDonorIdAsync(int donorId);

        /// <summary>
        /// Gets paginated donations for a specific user by email.
        /// </summary>
        Task<PagedResult<Donation>> GetPagedByUserEmailAsync(string email, PaginationParameters parameters);

        /// <summary>
        /// Searches for paginated donations for a specific user by email.
        /// </summary>
        Task<PagedResult<Donation>> SearchPagedByUserEmailAsync(string email, PaginationParameters parameters);

        /// <summary>
        /// Gets paginated donations for a specific referral code.
        /// </summary>
        Task<PagedResult<Donation>> GetPagedByReferralCodeAsync(string referralCode, PaginationParameters parameters);

        /// <summary>
        /// Gets active recurring donations for a user by email.
        /// </summary>
        Task<List<Donation>> GetActiveRecurringDonationsByUserEmailAsync(string email);

        /// <summary>
        /// Updates the payment method for a recurring donation.
        /// </summary>
        Task<bool> UpdateRecurringPaymentMethodAsync(string userEmail, int donationId, string newPaymentToken);

        /// <summary>
        /// Gets recurring donation setup records (parent donations) for a user.
        /// These are the donations users can manage (cancel, update payment method, etc.).
        /// </summary>
        Task<List<Donation>> GetRecurringDonationSetupsAsync(string email);

        /// <summary>
        /// Gets all payment records for a specific recurring donation setup.
        /// </summary>
        Task<List<Donation>> GetRecurringDonationPaymentsAsync(int recurringDonationSetupId);

        /// <summary>
        /// Determines if a donation is a recurring setup donation (manageable by user).
        /// </summary>
        bool IsRecurringSetupDonation(Donation donation);

        /// <summary>
        /// Determines if a donation is a recurring payment record.
        /// </summary>
        bool IsRecurringPaymentRecord(Donation donation);
    }
}
