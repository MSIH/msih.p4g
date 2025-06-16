using System;
using System.Threading.Tasks;
using msih.p4g.Server.Features.Base.UserService.Interfaces;
using msih.p4g.Server.Features.Base.ProfileService.Interfaces;
using msih.p4g.Server.Features.DonorService.Interfaces;
using msih.p4g.Server.Features.DonationService.Data;
using msih.p4g.Server.Features.DonationService.Models;
using msih.p4g.Server.Features.Base.PaymentService.Interfaces;
using msih.p4g.Server.Features.Base.PaymentService.Models;
using msih.p4g.Server.Features.Base.UserService.Models;
using msih.p4g.Server.Features.Base.ProfileService.Model;
using msih.p4g.Server.Features.DonorService.Model;
using msih.p4g.Server.Features.Base.UserProfileService.Interfaces;

namespace msih.p4g.Server.Features.DonationService.Services
{
    /// <summary>
    /// Service to process donations from the client.
    /// </summary>
    public class DonationService
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
    }
}
