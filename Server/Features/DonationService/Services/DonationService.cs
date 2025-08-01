// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

using msih.p4g.Server.Features.Base.AffiliateMonitoringService.Interfaces;
using msih.p4g.Server.Features.Base.MessageService.Interfaces;
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
using msih.p4g.Shared.Models;

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
        private readonly IMessageService _messageService;
        private readonly IAffiliateMonitoringService _affiliateMonitoringService;
        private readonly ILogger<DonationService> _logger;

        // Standard transaction fee percentage (can be moved to configuration in the future)
        private const decimal _transactionFeePercentage = 0.029m; // 2.9%
        private const decimal _transactionFeeFlat = 0.30m; // $0.30 flat fee

        // Ensure the constructor initializes _messageService and _logger
        public DonationService(
            IUserRepository userRepository,
            IProfileService profileService,
            IDonorService donorService,
            IDonationRepository donationRepository,
            IPaymentService paymentService,
            IUserProfileService userProfileService,
            IMessageService messageService,
            IAffiliateMonitoringService affiliateMonitoringService,
            ILogger<DonationService> logger)
        {
            _userRepository = userRepository;
            _profileService = profileService;
            _donorService = donorService;
            _donationRepository = donationRepository;
            _paymentService = paymentService;
            _userProfileService = userProfileService;
            _messageService = messageService;
            _affiliateMonitoringService = affiliateMonitoringService;
            _logger = logger;
        }

        /// <summary>
        /// Calculates the transaction fee for a donation amount.
        /// </summary>
        private decimal CalculateTransactionFee(decimal amount)
        {
            return Math.Round(amount * _transactionFeePercentage + _transactionFeeFlat, 2);
        }

        public async Task<DonorRegistrationResultType> ProcessDonorRegistrationAsync(DonationRequestDto dto)
        {
            bool donorCreated = false;
            bool error = false;
            try
            {
                // 1. Find user with needed navigation properties
                var user = await _userRepository.GetByEmailAsync(
                    email: dto.Email,
                    includeProfile: true,
                    includeAddress: true,
                    includeDonor: true,
                    includeFundraiser: false);

                if (user == null)
                {
                    // Create new user and profile in a single coordinated operation
                    user = new User
                    {
                        Email = dto.Email,
                        Role = UserRole.Donor
                    };

                    var profile = new Profile
                    {
                        FirstName = dto.FirstName,
                        LastName = dto.LastName,
                        Address = dto.Address,
                        MobileNumber = dto.Mobile
                    };

                    // The UserProfileService handles setting the UserId and generating the referral code
                    profile = await _userProfileService.CreateUserWithProfileAsync(user, profile, "DonationService");

                    // Check if the referral code belongs to the user making the donation (prevent self-referral)
                    string? referralCodeToUse = dto.ReferralCode;
                    if (!string.IsNullOrEmpty(dto.ReferralCode) && profile?.ReferralCode == dto.ReferralCode)
                    {
                        // User is trying to use their own referral code - silently ignore it
                        referralCodeToUse = null;
                        _logger.LogInformation("User {Email} attempted to use their own referral code {ReferralCode} during donor registration - ignoring referral code",
                            dto.Email, dto.ReferralCode);
                    }

                    // Create donor for the new user
                    var donor = new Donor
                    {
                        UserId = user.Id,
                        IsActive = true,
                        CreatedBy = "DonationService",
                        CreatedOn = DateTime.UtcNow,
                        ReferralCode = referralCodeToUse
                    };
                    donor = await _donorService.AddAsync(donor);

                    donorCreated = true;

                    // Check affiliate suspension after donor creation
                    if (!string.IsNullOrEmpty(dto.ReferralCode))
                    {
                        try
                        {
                            await _affiliateMonitoringService.CheckAffiliateAfterDonorCreationAsync(dto.ReferralCode);
                        }
                        catch (Exception affiliateEx)
                        {
                            _logger.LogError(affiliateEx,
                                "Error checking affiliate suspension for referral code {ReferralCode}",
                                dto.ReferralCode);
                            // Don't fail the entire donor creation process for affiliate monitoring errors
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error processing donor registration for email {Email}. Error: {ErrorMessage}",
                    dto.Email, ex.Message);
                error = true;

                // You might want to return a specific error result type or rethrow
                // depending on your error handling strategy

            }

            // Return appropriate result based on what was done
            if (donorCreated)
            {
                return DonorRegistrationResultType.NewCreated;
            }
            else if (error)
            {
                return DonorRegistrationResultType.Error;
            }
            else
            {
                return DonorRegistrationResultType.ExistingFound;
            }
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
            bool newDonorCreated = false;

            if (donor == null)
            {
                // Check if the referral code belongs to the user making the donation (prevent self-referral)
                string? donorReferralCodeToUse = dto.ReferralCode;
                if (!string.IsNullOrEmpty(dto.ReferralCode) && profile?.ReferralCode == dto.ReferralCode)
                {
                    // User is trying to use their own referral code - silently ignore it
                    donorReferralCodeToUse = null;
                    _logger.LogInformation("User {Email} attempted to use their own referral code {ReferralCode} during donor creation - ignoring referral code",
                        dto.Email, dto.ReferralCode);
                }

                // Create a new donor
                donor = new Donor
                {
                    UserId = user.Id,
                    IsActive = true,
                    CreatedBy = "DonationService",
                    CreatedOn = DateTime.UtcNow,
                    ReferralCode = donorReferralCodeToUse
                };
                donor = await _donorService.AddAsync(donor);
                newDonorCreated = true;
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
            // Check if the referral code belongs to the user making the donation (prevent self-referral)
            string? referralCodeToUse = dto.ReferralCode;
            if (!string.IsNullOrEmpty(dto.ReferralCode) && profile?.ReferralCode == dto.ReferralCode)
            {
                // User is trying to use their own referral code - silently ignore it
                referralCodeToUse = null;
                _logger.LogInformation("User {Email} attempted to use their own referral code {ReferralCode} for donation - ignoring referral code",
                    dto.Email, dto.ReferralCode);
            }

            var donation = new Donation
            {
                DonationAmount = dto.DonationAmount,
                PayTransactionFeeAmount = dto.PayTransactionFeeAmount,
                DonorId = donor.Id,
                PayTransactionFee = dto.PayTransactionFee,
                IsMonthly = dto.IsMonthly,
                IsAnnual = dto.IsAnnual,
                DonationMessage = dto.DonationMessage,
                ReferralCode = referralCodeToUse,
                CampaignCode = dto.CampaignCode,
                CampaignId = dto.CampaignId,
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

            // 4.5. Set up recurring donation processing if this is a recurring donation
            if ((dto.IsMonthly || dto.IsAnnual) && !string.IsNullOrEmpty(dto.PaymentToken))
            {
                // Store payment token for recurring processing
                donation.RecurringPaymentToken = dto.PaymentToken;
                
                // Set next process date
                donation.NextProcessDate = dto.IsMonthly 
                    ? DateTime.UtcNow.AddMonths(1) 
                    : DateTime.UtcNow.AddYears(1);
                
                await _donationRepository.UpdateAsync(donation, "DonationService");
                
                _logger.LogInformation("Set up recurring donation {DonationId} for {Frequency} processing. Next process date: {NextProcessDate}",
                    donation.Id, dto.IsMonthly ? "monthly" : "annual", donation.NextProcessDate);
            }

            // 5. Send email notification to donor with error handling
            try
            {
                var placeholders = new Dictionary<string, string>
                {
                    ["donorName"] = profile?.FullName ?? $"{dto.FirstName} {dto.LastName}",
                    ["donationAmountInDollars"] = donation.DonationAmount.ToString("C")
                };

                bool emailSent = await _messageService.SendTemplatedMessageByNameAsync(
                    templateName: "MVP Donor Thank You Email",
                    to: user.Email,
                    placeholderValues: placeholders
                );

                if (emailSent)
                {
                    _logger.LogInformation("Thank you email sent successfully to donor {Email} for donation ID {DonationId}",
                        user.Email, donation.Id);
                }
                else
                {
                    _logger.LogWarning("Failed to send thank you email to donor {Email} for donation ID {DonationId}. Email service returned false.",
                        user.Email, donation.Id);
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the donation process
                _logger.LogError(ex,
                    "Error sending thank you email to donor {Email} for donation ID {DonationId}. Donation was processed successfully but email notification failed.",
                    user.Email, donation.Id);

                // Optionally, you could store this failure for retry later
                // The donation itself was successful, so we don't want to throw an exception here
            }

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
            var donations = await _donationRepository.GetAllWithIncludesAsync();
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
        public async Task<bool> SetActiveAsync(int id, bool isActive, string modifiedBy = "DonationService")
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

        /// <summary>
        /// Gets donations for a specific user by email.
        /// </summary>
        public async Task<List<Donation>> GetByUserEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
                return new List<Donation>();

            try
            {
                // Get user and their donor information
                var user = await _userRepository.GetByEmailAsync(email, includeDonor: true);
                if (user?.Donor == null)
                    return new List<Donation>();

                // Get donations for this donor
                return await GetByDonorIdAsync(user.Donor.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting donations for user {Email}", email);
                return new List<Donation>();
            }
        }

        /// <summary>
        /// Searches for donations for a specific user by email.
        /// </summary>
        public async Task<List<Donation>> SearchByUserEmailAsync(string email, string searchTerm)
        {
            if (string.IsNullOrEmpty(email))
                return new List<Donation>();

            try
            {
                // Get all user donations first
                var userDonations = await GetByUserEmailAsync(email);

                if (string.IsNullOrWhiteSpace(searchTerm))
                    return userDonations;

                // Filter donations based on search term
                return userDonations.Where(d =>
                    (d.DonationMessage?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (d.ReferralCode?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (d.CampaignCode?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    d.DonationAmount.ToString().Contains(searchTerm)
                ).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching donations for user {Email} with search term '{SearchTerm}'", email, searchTerm);
                return new List<Donation>();
            }
        }

        /// <summary>
        /// Updates a recurring donation for a specific user.
        /// </summary>
        public async Task<bool> UpdateRecurringDonationAsync(string userEmail, int donationId, decimal newAmount, bool isActive)
        {
            if (string.IsNullOrEmpty(userEmail))
                return false;

            try
            {
                // Verify the donation belongs to the current user
                var user = await _userRepository.GetByEmailAsync(userEmail, includeDonor: true);
                if (user?.Donor == null)
                    return false;

                var donation = await GetByIdAsync(donationId);
                if (donation == null || donation.DonorId != user.Donor.Id)
                    return false;

                // Only allow updating recurring donations
                if (!donation.IsMonthly && !donation.IsAnnual)
                    return false;

                // Update the donation
                donation.DonationAmount = newAmount;
                donation.IsActive = isActive;
                donation.ModifiedBy = userEmail;
                donation.ModifiedOn = DateTime.UtcNow;

                return await UpdateAsync(donation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating recurring donation {DonationId} for user {UserEmail}", donationId, userEmail);
                return false;
            }
        }

        /// <summary>
        /// Cancels a recurring donation for a specific user.
        /// </summary>
        public async Task<bool> CancelRecurringDonationAsync(string userEmail, int donationId)
        {
            if (string.IsNullOrEmpty(userEmail))
                return false;

            try
            {
                // Verify the donation belongs to the current user
                var user = await _userRepository.GetByEmailAsync(userEmail, includeDonor: true);
                if (user?.Donor == null)
                    return false;

                var donation = await GetByIdAsync(donationId);
                if (donation == null || donation.DonorId != user.Donor.Id)
                    return false;

                // Only allow canceling recurring donations
                if (!donation.IsMonthly && !donation.IsAnnual)
                    return false;

                // Set the donation as inactive
                return await SetActiveAsync(donationId, false, userEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling recurring donation {DonationId} for user {UserEmail}", donationId, userEmail);
                return false;
            }
        }

        /// <summary>
        /// Gets paginated donations for a specific user by email.
        /// </summary>
        public async Task<PagedResult<Donation>> GetPagedByUserEmailAsync(string email, PaginationParameters parameters)
        {
            if (string.IsNullOrEmpty(email))
                return new PagedResult<Donation> { Items = new List<Donation>(), TotalCount = 0, PageNumber = parameters.PageNumber, PageSize = parameters.PageSize };

            try
            {
                return await _donationRepository.GetPagedByUserEmailAsync(email, parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated donations for user {Email}", email);
                return new PagedResult<Donation> { Items = new List<Donation>(), TotalCount = 0, PageNumber = parameters.PageNumber, PageSize = parameters.PageSize };
            }
        }

        /// <summary>
        /// Searches for paginated donations for a specific user by email.
        /// </summary>
        public async Task<PagedResult<Donation>> SearchPagedByUserEmailAsync(string email, PaginationParameters parameters)
        {
            if (string.IsNullOrEmpty(email))
                return new PagedResult<Donation> { Items = new List<Donation>(), TotalCount = 0, PageNumber = parameters.PageNumber, PageSize = parameters.PageSize };

            try
            {
                return await _donationRepository.SearchPagedByUserEmailAsync(email, parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching paginated donations for user {Email}", email);
                return new PagedResult<Donation> { Items = new List<Donation>(), TotalCount = 0, PageNumber = parameters.PageNumber, PageSize = parameters.PageSize };
            }
        }

        /// <summary>
        /// Gets paginated donations for a specific referral code.
        /// </summary>
        public async Task<PagedResult<Donation>> GetPagedByReferralCodeAsync(string referralCode, PaginationParameters parameters)
        {
            return await _donationRepository.GetPagedByReferralCodeAsync(referralCode, parameters);
        }

        /// <summary>
        /// Gets active recurring donations for a user by email.
        /// </summary>
        public async Task<List<Donation>> GetActiveRecurringDonationsByUserEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
                return new List<Donation>();

            try
            {
                var userDonations = await GetByUserEmailAsync(email);
                return userDonations.Where(d => d.IsActive && (d.IsMonthly || d.IsAnnual)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active recurring donations for user {Email}", email);
                return new List<Donation>();
            }
        }

        /// <summary>
        /// Updates the payment method for a recurring donation.
        /// </summary>
        public async Task<bool> UpdateRecurringPaymentMethodAsync(string userEmail, int donationId, string newPaymentToken)
        {
            if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(newPaymentToken))
                return false;

            try
            {
                // Verify the donation belongs to the current user
                var user = await _userRepository.GetByEmailAsync(userEmail, includeDonor: true);
                if (user?.Donor == null)
                    return false;

                var donation = await GetByIdAsync(donationId);
                if (donation == null || donation.DonorId != user.Donor.Id)
                    return false;

                // Only allow updating recurring setup donations (parent donations)
                if (!donation.IsMonthly && !donation.IsAnnual)
                    return false;

                // Make sure this is a setup donation, not a payment record
                if (donation.ParentRecurringDonationId.HasValue)
                    return false;

                // Update the payment method
                donation.RecurringPaymentToken = newPaymentToken;
                donation.ModifiedBy = userEmail;
                donation.ModifiedOn = DateTime.UtcNow;

                return await UpdateAsync(donation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment method for recurring donation {DonationId} for user {UserEmail}", donationId, userEmail);
                return false;
            }
        }

        /// <summary>
        /// Gets recurring donation setup records (parent donations) for a user.
        /// These are the donations users can manage (cancel, update payment method, etc.).
        /// </summary>
        public async Task<List<Donation>> GetRecurringDonationSetupsAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
                return new List<Donation>();

            try
            {
                var userDonations = await GetByUserEmailAsync(email);
                return userDonations.Where(d => 
                    d.IsActive && 
                    (d.IsMonthly || d.IsAnnual) && 
                    !d.ParentRecurringDonationId.HasValue // Only setup donations, not payments
                ).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recurring donation setups for user {Email}", email);
                return new List<Donation>();
            }
        }

        /// <summary>
        /// Gets all payment records for a specific recurring donation setup.
        /// </summary>
        public async Task<List<Donation>> GetRecurringDonationPaymentsAsync(int recurringDonationSetupId)
        {
            try
            {
                var allDonations = await GetAllAsync();
                return allDonations.Where(d => 
                    d.ParentRecurringDonationId == recurringDonationSetupId && 
                    d.IsActive
                ).OrderByDescending(d => d.CreatedOn).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment records for recurring donation setup {SetupId}", recurringDonationSetupId);
                return new List<Donation>();
            }
        }

        /// <summary>
        /// Determines if a donation is a recurring setup donation (manageable by user).
        /// </summary>
        public bool IsRecurringSetupDonation(Donation donation)
        {
            return donation != null && 
                   (donation.IsMonthly || donation.IsAnnual) && 
                   !donation.ParentRecurringDonationId.HasValue;
        }

        /// <summary>
        /// Determines if a donation is a recurring payment record.
        /// </summary>
        public bool IsRecurringPaymentRecord(Donation donation)
        {
            return donation != null && donation.ParentRecurringDonationId.HasValue;
        }
    }
}
