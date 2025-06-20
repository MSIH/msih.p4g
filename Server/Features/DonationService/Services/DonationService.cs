// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

using msih.p4g.Server.Features.Base.PaymentService.Interfaces;
using msih.p4g.Server.Features.Base.PaymentService.Models;
using msih.p4g.Server.Features.Base.ProfileService.Interfaces;
using msih.p4g.Server.Features.Base.ProfileService.Model;
using msih.p4g.Server.Features.Base.UserProfileService.Interfaces;
using msih.p4g.Server.Features.Base.UserService.Interfaces;
using msih.p4g.Server.Features.Base.UserService.Models;
using msih.p4g.Server.Features.DonationService.Interfaces;
using msih.p4g.Server.Features.DonationService.Models;
using msih.p4g.Server.Features.DonorService.Interfaces;
using msih.p4g.Server.Features.DonorService.Model;

namespace msih.p4g.Server.Features.DonationService.Services
{
    /// <summary>
    /// Service to process donations from the client.
    /// </summary>
    public class DonationService : IDonationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IProfileService _profileService;
        private readonly IDonorService _donorService;
        private readonly IDonationRepository _donationRepository;
        private readonly IPaymentService _paymentService;
        private readonly IUserProfileService _userProfileService;

        // Standard transaction fee percentage (can be moved to configuration in the future)
        private const decimal _transactionFeePercentage = 0.029m; // 2.9%
        private const decimal _transactionFeeFlat = 0.30m; // $0.30 flat fee

        public DonationService(
            IUserRepository userRepository,
            IProfileService profileService,
            IDonorService donorService,
            IDonationRepository donationRepository,
            IPaymentService paymentService,
            IUserProfileService userProfileService)
        {
            _userRepository = userRepository;
            _profileService = profileService;
            _donorService = donorService;
            _donationRepository = donationRepository;
            _paymentService = paymentService;
            _userProfileService = userProfileService;
        }

        /// <summary>
        /// Calculates the transaction fee for a donation amount.
        /// </summary>
        private decimal CalculateTransactionFee(decimal amount)
        {
            return Math.Round(amount * _transactionFeePercentage + _transactionFeeFlat, 2);
        }

        /// <summary>
        /// Processes a donation request from the client.
        /// </summary>
        public async Task<Donation> ProcessDonationAsync(DonationRequestDto dto)
        {
            // 1. Find or create user with needed navigation properties
            var user = await _userRepository.GetByEmailAsync(
                email: dto.Email,
                includeProfile: true,
                includeAddress: true,
                includeDonor: true,
                includeFundraiser: false);

            bool isNewUser = false;
            Profile? profile = null;

            if (user == null)
            {
                // Create new user and profile in a single coordinated operation
                user = new User
                {
                    Email = dto.Email,
                    Role = UserRole.Donor
                };

                profile = new Profile
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Address = dto.Address,
                    MobileNumber = dto.Mobile
                };

                // The UserProfileService handles setting the UserId and generating the referral code
                profile = await _userProfileService.CreateUserWithProfileAsync(user, profile, "DonationService");
                isNewUser = true;
            }
            else
            {
                // Use navigation property to get profile if it exists
                profile = user.Profile;

                // If profile doesn't exist (unlikely since we loaded it with includeProfile=true), create it
                if (profile == null)
                {
                    profile = new Profile
                    {
                        UserId = user.Id,
                        FirstName = dto.FirstName,
                        LastName = dto.LastName,
                        Address = dto.Address,
                        MobileNumber = dto.Mobile
                    };
                    profile = await _profileService.AddAsync(profile, "DonationService");
                }
                // Update profile information if needed
                else if (ShouldUpdateProfile(profile, dto))
                {
                    profile.FirstName = dto.FirstName;
                    profile.LastName = dto.LastName;
                    profile.Address = dto.Address;

                    if (!string.IsNullOrEmpty(dto.Mobile) &&
                        (string.IsNullOrEmpty(profile.MobileNumber) || !profile.MobileNumber.Equals(dto.Mobile)))
                    {
                        profile.MobileNumber = dto.Mobile;
                    }

                    profile = await _profileService.UpdateAsync(profile, "DonationService");
                }
            }

            // 2. Find or create donor using navigation property
            Donor? donor = user.Donor;

            if (donor == null)
            {
                // Create a new donor
                donor = new Donor
                {
                    UserId = user.Id,
                    IsActive = true,
                    CreatedBy = "DonationService",
                    CreatedOn = DateTime.UtcNow
                };
                donor = await _donorService.AddAsync(donor);
            }

            // Calculate transaction fee and total amount
            //decimal transactionFee = CalculateTransactionFee(dto.TransactionFee);
            decimal amountToCharge = dto.DonationAmount;

            //// If donor pays the transaction fee, add it to the amount to charge
            //if (dto.PayTransactionFee)
            //{
            //    amountToCharge += transactionFee;
            //}

            // 3. Process payment
            var paymentRequest = new PaymentRequest
            {
                Amount = amountToCharge,
                Currency = "USD", // or from dto if needed
                Description = $"Donation by {dto.FirstName} {dto.LastName}",
                OrderReference = Guid.NewGuid().ToString(),
                CustomerEmail = dto.Email,
                PaymentMethodNonce = dto.PaymentToken
            };
            var paymentResponse = await _paymentService.ProcessPaymentAsync(paymentRequest);
            if (!paymentResponse.Success)
            {
                throw new Exception($"Payment failed: {paymentResponse.ErrorMessage}");
            }

            // 4. Create donation record
            var donation = new Donation
            {
                DonationAmount = dto.DonationAmount,
                PayTransactionFeeAmount = dto.PayTransactionFeeAmount,
                DonorId = donor.Id,
                PayTransactionFee = dto.PayTransactionFee,
                IsMonthly = dto.IsMonthly,
                IsAnnual = dto.IsAnnual,
                DonationMessage = dto.DonationMessage,
                ReferralCode = profile.ReferralCode, // Use the profile's referral code
                CampaignCode = dto.CampaignCode,
                IsActive = true,
                CreatedBy = "DonationService",
                CreatedOn = DateTime.UtcNow
            };

            // Get the PaymentTransaction ID from the payment response
            if (!string.IsNullOrEmpty(paymentResponse.TransactionId))
            {
                var paymentTransaction = await _paymentService.GetTransactionDetailsAsync(paymentResponse.TransactionId);
                if (paymentTransaction != null)
                {
                    donation.PaymentTransactionId = paymentTransaction.Id;
                }
            }

            donation = await _donationRepository.AddAsync(donation, "DonationService");
            return donation;
        }

        /// <summary>
        /// Determines if profile information should be updated based on the donation request
        /// </summary>
        private bool ShouldUpdateProfile(Profile profile, DonationRequestDto dto)
        {
            return (!string.IsNullOrEmpty(dto.FirstName) && !dto.FirstName.Equals(profile.FirstName)) ||
                   (!string.IsNullOrEmpty(dto.LastName) && !dto.LastName.Equals(profile.LastName)) ||
                   (dto.Address != null && !dto.Address.Equals(profile.Address)) ||
                   (!string.IsNullOrEmpty(dto.Mobile) && !dto.Mobile.Equals(profile.MobileNumber));
        }

        /// <summary>
        /// Gets all donations.
        /// </summary>
        public async Task<List<Donation>> GetAllAsync()
        {
            var donations = await _donationRepository.GetAllAsync();
            return donations.ToList();
        }

        /// <summary>
        /// Gets a donation by its ID.
        /// </summary>
        public async Task<Donation?> GetByIdAsync(int id)
        {
            return await _donationRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Gets donations by donor ID.
        /// </summary>
        public async Task<List<Donation>> GetByDonorIdAsync(int donorId)
        {
            var donations = await _donationRepository.FindAsync(d => d.DonorId == donorId);
            return donations.ToList();
        }

        /// <summary>
        /// Gets donations by campaign ID.
        /// </summary>
        public async Task<List<Donation>> GetByCampaignIdAsync(int campaignId)
        {
            var donations = await _donationRepository.FindAsync(d => d.CampaignId == campaignId);
            return donations.ToList();
        }

        /// <summary>
        /// Gets donations by campaign code.
        /// </summary>
        public async Task<List<Donation>> GetByCampaignCodeAsync(string campaignCode)
        {
            if (string.IsNullOrEmpty(campaignCode))
                return new List<Donation>();

            var donations = await _donationRepository.FindAsync(d => d.CampaignCode == campaignCode);
            return donations.ToList();
        }

        /// <summary>
        /// Gets donations by referral code.
        /// </summary>
        public async Task<List<Donation>> GetByReferralCodeAsync(string referralCode)
        {
            if (string.IsNullOrEmpty(referralCode))
                return new List<Donation>();

            var donations = await _donationRepository.FindAsync(d => d.ReferralCode == referralCode);
            return donations.ToList();
        }

        /// <summary>
        /// Searches for donations matching the specified search term.
        /// </summary>
        public async Task<List<Donation>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return await GetAllAsync();

            // Simple search implementation - can be expanded based on requirements
            var donations = await _donationRepository.FindAsync(d =>
                (d.DonationMessage != null && d.DonationMessage.Contains(searchTerm)) ||
                (d.ReferralCode != null && d.ReferralCode.Contains(searchTerm)) ||
                (d.CampaignCode != null && d.CampaignCode.Contains(searchTerm))
            );

            return donations.ToList();
        }

        /// <summary>
        /// Adds a new donation.
        /// </summary>
        public async Task<Donation> AddAsync(Donation donation)
        {
            donation.CreatedOn = DateTime.UtcNow;
            donation.CreatedBy = "DonationService";
            donation.IsActive = true;

            return await _donationRepository.AddAsync(donation, "DonationService");
        }

        /// <summary>
        /// Updates an existing donation.
        /// </summary>
        public async Task<bool> UpdateAsync(Donation donation)
        {
            donation.ModifiedOn = DateTime.UtcNow;
            donation.ModifiedBy = "DonationService";

            await _donationRepository.UpdateAsync(donation, "DonationService");
            return true;
        }

        /// <summary>
        /// Sets the active status of a donation.
        /// </summary>
        public async Task<bool> SetActiveAsync(int id, bool isActive, string modifiedBy = "System")
        {
            return await _donationRepository.SetActiveStatusAsync(id, isActive, modifiedBy);
        }

        /// <summary>
        /// Gets the total donation amount for a specific campaign.
        /// </summary>
        public async Task<decimal> GetTotalAmountByCampaignIdAsync(int campaignId)
        {
            var donations = await GetByCampaignIdAsync(campaignId);
            return donations.Sum(d => d.DonationAmount);
        }

        /// <summary>
        /// Gets the total donation amount for a specific donor.
        /// </summary>
        public async Task<decimal> GetTotalAmountByDonorIdAsync(int donorId)
        {
            var donations = await GetByDonorIdAsync(donorId);
            return donations.Sum(d => d.DonationAmount);
        }
    }
}
