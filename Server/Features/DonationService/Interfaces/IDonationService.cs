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
        Task<bool> SetActiveAsync(int id, bool isActive, string modifiedBy = "System");

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
    }
}
