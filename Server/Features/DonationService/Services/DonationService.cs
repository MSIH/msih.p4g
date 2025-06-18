using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using msih.p4g.Server.Features.Base.UserService.Interfaces;
using msih.p4g.Server.Features.Base.ProfileService.Interfaces;
using msih.p4g.Server.Features.DonorService.Interfaces;
using msih.p4g.Server.Features.DonationService.Models;
using msih.p4g.Server.Features.Base.PaymentService.Interfaces;
using msih.p4g.Server.Features.Base.PaymentService.Models;
using msih.p4g.Server.Features.Base.UserService.Models;
using msih.p4g.Server.Features.Base.ProfileService.Model;
using msih.p4g.Server.Features.DonorService.Model;
using msih.p4g.Server.Features.Base.UserProfileService.Interfaces;
using msih.p4g.Server.Features.DonationService.Interfaces;

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
        /// Processes a donation request from the client.
        /// </summary>
        public async Task<Donation> ProcessDonationAsync(DonationRequestDto dto)
        {
            // 1. Find or create user
            var user = await _userRepository.GetByEmailAsync(dto.Email);
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
                    Address = dto.Address
                };
                
                // The UserProfileService handles setting the UserId and generating the referral code
                profile = await _userProfileService.CreateUserWithProfileAsync(user, profile, "DonationService");
                isNewUser = true;
            }
            else
            {
                // User exists, get their profile
                profile = await _profileService.GetByIdAsync(user.Id);
                
                // If profile doesn't exist, create it
                if (profile == null)
                {
                    profile = new Profile
                    {
                        UserId = user.Id,
                        FirstName = dto.FirstName,
                        LastName = dto.LastName,
                        Address = dto.Address
                    };
                    profile = await _profileService.AddAsync(profile, "DonationService");
                }
            }

            // 3. Find or create donor using navigation property if user exists
            Donor? donor = null;
            if (!isNewUser && user.Donor != null)
            {
                donor = user.Donor;
            }
            
            if (donor == null)
            {
                // If not found through navigation, try to get by user ID
                if (!isNewUser)
                {
                    donor = await _donorService.GetByIdAsync(user.Id);
                }
                
                // If still not found, create a new donor
                if (donor == null)
                {
                    donor = new Donor
                    {
                        UserId = user.Id,
                        IsActive = true,
                        CreatedBy = "DonationService",
                        CreatedOn = DateTime.UtcNow
                    };
                    donor = await _donorService.AddAsync(donor);
                }
            }

            // 4. Process payment
            var paymentRequest = new PaymentRequest
            {
                Amount = dto.Amount,
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

            // 5. Create donation record
            var donation = new Donation
            {
                Amount = dto.Amount,
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

            // Now we have the PaymentTransaction ID from the payment response
            // We need to get the PaymentTransaction from the DB to get its ID
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
            return donations.Sum(d => d.Amount);
        }

        /// <summary>
        /// Gets the total donation amount for a specific donor.
        /// </summary>
        public async Task<decimal> GetTotalAmountByDonorIdAsync(int donorId)
        {
            var donations = await GetByDonorIdAsync(donorId);
            return donations.Sum(d => d.Amount);
        }
    }
}
