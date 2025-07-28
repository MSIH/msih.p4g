/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

using msih.p4g.Server.Features.Base.PaymentService.Interfaces;
using msih.p4g.Server.Features.Base.PaymentService.Models;
using msih.p4g.Server.Features.Base.MessageService.Interfaces;
using msih.p4g.Server.Features.Base.UserService.Interfaces;
using msih.p4g.Server.Features.DonationService.Interfaces;
using msih.p4g.Server.Features.DonationService.Models;
using msih.p4g.Server.Features.RecurringDonationService.Interfaces;
using msih.p4g.Server.Features.RecurringDonationService.Models;
using msih.p4g.Shared.Models;

namespace msih.p4g.Server.Features.RecurringDonationService.Services
{
    /// <summary>
    /// Service for managing recurring donations.
    /// </summary>
    public class RecurringDonationService : IRecurringDonationService
    {
        private readonly IRecurringDonationRepository _recurringDonationRepository;
        private readonly IPaymentService _paymentService;
        private readonly IDonationService _donationService;
        private readonly IMessageService _messageService;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<RecurringDonationService> _logger;

        // Maximum failed attempts before marking as failed
        private const int MaxFailedAttempts = 3;

        public RecurringDonationService(
            IRecurringDonationRepository recurringDonationRepository,
            IPaymentService paymentService,
            IDonationService donationService,
            IMessageService messageService,
            IUserRepository userRepository,
            ILogger<RecurringDonationService> logger)
        {
            _recurringDonationRepository = recurringDonationRepository;
            _paymentService = paymentService;
            _donationService = donationService;
            _messageService = messageService;
            _userRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new recurring donation subscription.
        /// </summary>
        public async Task<RecurringDonation> CreateRecurringDonationAsync(RecurringDonation recurringDonation, string createdBy = "RecurringDonationService")
        {
            // Set initial values
            recurringDonation.Status = RecurringDonationStatus.Active;
            recurringDonation.NextProcessDate = CalculateNextProcessDate(recurringDonation.StartDate, recurringDonation.Frequency);
            recurringDonation.SuccessfulDonationsCount = 0;
            recurringDonation.FailedAttemptsCount = 0;
            recurringDonation.CreatedBy = createdBy;
            recurringDonation.CreatedOn = DateTime.UtcNow;
            recurringDonation.IsActive = true;

            var result = await _recurringDonationRepository.AddAsync(recurringDonation, createdBy);

            _logger.LogInformation("Created recurring donation {Id} for donor {DonorId} with amount {Amount} and frequency {Frequency}",
                result.Id, result.DonorId, result.Amount, result.Frequency);

            return result;
        }

        /// <summary>
        /// Gets a recurring donation by ID.
        /// </summary>
        public async Task<RecurringDonation?> GetByIdAsync(int id)
        {
            return await _recurringDonationRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Gets all recurring donations for a specific donor.
        /// </summary>
        public async Task<List<RecurringDonation>> GetByDonorIdAsync(int donorId)
        {
            var recurringDonations = await _recurringDonationRepository.GetByDonorIdAsync(donorId);
            return recurringDonations.ToList();
        }

        /// <summary>
        /// Gets recurring donations that are due for processing.
        /// </summary>
        public async Task<List<RecurringDonation>> GetDueForProcessingAsync()
        {
            var recurringDonations = await _recurringDonationRepository.GetDueForProcessingAsync();
            return recurringDonations.ToList();
        }

        /// <summary>
        /// Gets recurring donations by status.
        /// </summary>
        public async Task<List<RecurringDonation>> GetByStatusAsync(RecurringDonationStatus status)
        {
            var recurringDonations = await _recurringDonationRepository.GetByStatusAsync(status);
            return recurringDonations.ToList();
        }

        /// <summary>
        /// Processes a single recurring donation.
        /// </summary>
        public async Task<bool> ProcessRecurringDonationAsync(int recurringDonationId)
        {
            var recurringDonation = await _recurringDonationRepository.GetByIdAsync(recurringDonationId);
            if (recurringDonation == null)
            {
                _logger.LogError("Recurring donation {Id} not found", recurringDonationId);
                return false;
            }

            if (recurringDonation.Status != RecurringDonationStatus.Active)
            {
                _logger.LogWarning("Recurring donation {Id} is not active (Status: {Status})", recurringDonationId, recurringDonation.Status);
                return false;
            }

            try
            {
                // Calculate total amount to charge
                decimal amountToCharge = recurringDonation.Amount;
                if (recurringDonation.PayTransactionFee)
                {
                    amountToCharge += recurringDonation.PayTransactionFeeAmount;
                }

                // Process payment
                var paymentRequest = new PaymentRequest
                {
                    Amount = amountToCharge,
                    Currency = recurringDonation.Currency,
                    Description = $"Recurring donation - {recurringDonation.Frequency}",
                    OrderReference = $"RecurringDonation-{recurringDonation.Id}-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    CustomerEmail = recurringDonation.Donor?.User?.Email ?? "unknown@email.com",
                    PaymentMethodNonce = recurringDonation.PaymentMethodToken
                };

                var paymentResponse = await _paymentService.ProcessPaymentAsync(paymentRequest);

                if (paymentResponse.Success)
                {
                    // Create donation record
                    var donation = new Donation
                    {
                        DonationAmount = recurringDonation.Amount,
                        PayTransactionFeeAmount = recurringDonation.PayTransactionFeeAmount,
                        DonorId = recurringDonation.DonorId,
                        PayTransactionFee = recurringDonation.PayTransactionFee,
                        IsMonthly = recurringDonation.Frequency == RecurringFrequency.Monthly,
                        IsAnnual = recurringDonation.Frequency == RecurringFrequency.Annually,
                        DonationMessage = recurringDonation.DonationMessage,
                        ReferralCode = recurringDonation.ReferralCode,
                        CampaignCode = recurringDonation.CampaignCode,
                        CampaignId = recurringDonation.CampaignId,
                        PaymentTransactionId = !string.IsNullOrEmpty(paymentResponse.TransactionId) ? 
                            (await _paymentService.GetTransactionDetailsAsync(paymentResponse.TransactionId))?.Id : null,
                        IsActive = true,
                        CreatedBy = "RecurringDonationService",
                        CreatedOn = DateTime.UtcNow
                    };

                    await _donationService.AddAsync(donation);

                    // Update recurring donation
                    await _recurringDonationRepository.IncrementSuccessfulDonationsCountAsync(recurringDonation.Id, "RecurringDonationService");
                    var nextProcessDate = CalculateNextProcessDate(DateTime.UtcNow, recurringDonation.Frequency);
                    await _recurringDonationRepository.UpdateNextProcessDateAsync(recurringDonation.Id, nextProcessDate, "RecurringDonationService");

                    // Clear any previous error messages
                    recurringDonation.LastErrorMessage = null;
                    await _recurringDonationRepository.UpdateAsync(recurringDonation, "RecurringDonationService");

                    // Send thank you email
                    await SendRecurringDonationThankYouEmailAsync(recurringDonation, donation);

                    _logger.LogInformation("Successfully processed recurring donation {Id} for amount {Amount}", 
                        recurringDonation.Id, recurringDonation.Amount);

                    return true;
                }
                else
                {
                    // Payment failed
                    await HandlePaymentFailureAsync(recurringDonation, paymentResponse.ErrorMessage ?? "Payment failed");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing recurring donation {Id}: {ErrorMessage}", 
                    recurringDonation.Id, ex.Message);
                await HandlePaymentFailureAsync(recurringDonation, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Processes all recurring donations that are due.
        /// </summary>
        public async Task<int> ProcessDueRecurringDonationsAsync()
        {
            var dueRecurringDonations = await GetDueForProcessingAsync();
            int successCount = 0;

            _logger.LogInformation("Processing {Count} recurring donations that are due", dueRecurringDonations.Count);

            foreach (var recurringDonation in dueRecurringDonations)
            {
                try
                {
                    if (await ProcessRecurringDonationAsync(recurringDonation.Id))
                    {
                        successCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing recurring donation {Id} in batch: {ErrorMessage}", 
                        recurringDonation.Id, ex.Message);
                }
            }

            _logger.LogInformation("Successfully processed {SuccessCount}/{TotalCount} recurring donations", 
                successCount, dueRecurringDonations.Count);

            return successCount;
        }

        /// <summary>
        /// Updates a recurring donation.
        /// </summary>
        public async Task<bool> UpdateRecurringDonationAsync(RecurringDonation recurringDonation, string modifiedBy = "RecurringDonationService")
        {
            recurringDonation.ModifiedBy = modifiedBy;
            recurringDonation.ModifiedOn = DateTime.UtcNow;

            await _recurringDonationRepository.UpdateAsync(recurringDonation, modifiedBy);
            return true;
        }

        /// <summary>
        /// Cancels a recurring donation.
        /// </summary>
        public async Task<bool> CancelRecurringDonationAsync(int id, string cancelledBy, string? reason = null)
        {
            var recurringDonation = await _recurringDonationRepository.GetByIdAsync(id);
            if (recurringDonation == null)
                return false;

            recurringDonation.Status = RecurringDonationStatus.Cancelled;
            recurringDonation.CancelledDate = DateTime.UtcNow;
            recurringDonation.CancelledBy = cancelledBy;
            recurringDonation.CancellationReason = reason;
            recurringDonation.ModifiedBy = cancelledBy;
            recurringDonation.ModifiedOn = DateTime.UtcNow;

            await _recurringDonationRepository.UpdateAsync(recurringDonation, cancelledBy);

            _logger.LogInformation("Cancelled recurring donation {Id} by {CancelledBy}. Reason: {Reason}", 
                id, cancelledBy, reason ?? "No reason provided");

            return true;
        }

        /// <summary>
        /// Pauses a recurring donation.
        /// </summary>
        public async Task<bool> PauseRecurringDonationAsync(int id, string modifiedBy)
        {
            return await _recurringDonationRepository.UpdateStatusAsync(id, RecurringDonationStatus.Paused, modifiedBy);
        }

        /// <summary>
        /// Resumes a paused recurring donation.
        /// </summary>
        public async Task<bool> ResumeRecurringDonationAsync(int id, string modifiedBy)
        {
            var recurringDonation = await _recurringDonationRepository.GetByIdAsync(id);
            if (recurringDonation == null || recurringDonation.Status != RecurringDonationStatus.Paused)
                return false;

            // Update next process date to now if it's in the past
            if (recurringDonation.NextProcessDate <= DateTime.UtcNow)
            {
                var nextProcessDate = CalculateNextProcessDate(DateTime.UtcNow, recurringDonation.Frequency);
                await _recurringDonationRepository.UpdateNextProcessDateAsync(id, nextProcessDate, modifiedBy);
            }

            return await _recurringDonationRepository.UpdateStatusAsync(id, RecurringDonationStatus.Active, modifiedBy);
        }

        /// <summary>
        /// Updates the amount for a recurring donation.
        /// </summary>
        public async Task<bool> UpdateAmountAsync(int id, decimal newAmount, string modifiedBy)
        {
            var recurringDonation = await _recurringDonationRepository.GetByIdAsync(id);
            if (recurringDonation == null)
                return false;

            recurringDonation.Amount = newAmount;
            recurringDonation.ModifiedBy = modifiedBy;
            recurringDonation.ModifiedOn = DateTime.UtcNow;

            await _recurringDonationRepository.UpdateAsync(recurringDonation, modifiedBy);
            return true;
        }

        /// <summary>
        /// Updates the payment method for a recurring donation.
        /// </summary>
        public async Task<bool> UpdatePaymentMethodAsync(int id, string newPaymentMethodToken, string modifiedBy)
        {
            var recurringDonation = await _recurringDonationRepository.GetByIdAsync(id);
            if (recurringDonation == null)
                return false;

            recurringDonation.PaymentMethodToken = newPaymentMethodToken;
            recurringDonation.ModifiedBy = modifiedBy;
            recurringDonation.ModifiedOn = DateTime.UtcNow;

            // Reset failed attempts since we have a new payment method
            recurringDonation.FailedAttemptsCount = 0;
            recurringDonation.LastErrorMessage = null;

            // If it was in failed status, reactivate it
            if (recurringDonation.Status == RecurringDonationStatus.Failed)
            {
                recurringDonation.Status = RecurringDonationStatus.Active;
            }

            await _recurringDonationRepository.UpdateAsync(recurringDonation, modifiedBy);
            return true;
        }

        /// <summary>
        /// Gets paginated recurring donations for a donor.
        /// </summary>
        public async Task<PagedResult<RecurringDonation>> GetPagedByDonorIdAsync(int donorId, PaginationParameters parameters)
        {
            return await _recurringDonationRepository.GetPagedByDonorIdAsync(donorId, parameters);
        }

        /// <summary>
        /// Gets paginated recurring donations by status.
        /// </summary>
        public async Task<PagedResult<RecurringDonation>> GetPagedByStatusAsync(RecurringDonationStatus status, PaginationParameters parameters)
        {
            return await _recurringDonationRepository.GetPagedByStatusAsync(status, parameters);
        }

        /// <summary>
        /// Gets recurring donations by user email.
        /// </summary>
        public async Task<List<RecurringDonation>> GetByUserEmailAsync(string email)
        {
            var recurringDonations = await _recurringDonationRepository.GetByUserEmailAsync(email);
            return recurringDonations.ToList();
        }

        /// <summary>
        /// Gets paginated recurring donations by user email.
        /// </summary>
        public async Task<PagedResult<RecurringDonation>> GetPagedByUserEmailAsync(string email, PaginationParameters parameters)
        {
            return await _recurringDonationRepository.GetPagedByUserEmailAsync(email, parameters);
        }

        #region Private Helper Methods

        /// <summary>
        /// Calculates the next process date based on the frequency.
        /// </summary>
        private DateTime CalculateNextProcessDate(DateTime fromDate, RecurringFrequency frequency)
        {
            return frequency switch
            {
                RecurringFrequency.Monthly => fromDate.AddMonths(1),
                RecurringFrequency.Annually => fromDate.AddYears(1),
                _ => fromDate.AddMonths(1)
            };
        }

        /// <summary>
        /// Handles payment failure for recurring donations.
        /// </summary>
        private async Task HandlePaymentFailureAsync(RecurringDonation recurringDonation, string errorMessage)
        {
            await _recurringDonationRepository.IncrementFailedAttemptsCountAsync(recurringDonation.Id, "RecurringDonationService");

            recurringDonation.LastErrorMessage = errorMessage;

            if (recurringDonation.FailedAttemptsCount >= MaxFailedAttempts)
            {
                // Mark as failed after max attempts
                recurringDonation.Status = RecurringDonationStatus.Failed;
                _logger.LogWarning("Recurring donation {Id} marked as failed after {FailedAttempts} attempts. Last error: {ErrorMessage}",
                    recurringDonation.Id, recurringDonation.FailedAttemptsCount, errorMessage);
            }
            else
            {
                // Schedule retry for next processing cycle
                var nextProcessDate = CalculateNextProcessDate(DateTime.UtcNow, recurringDonation.Frequency);
                recurringDonation.NextProcessDate = nextProcessDate;
                _logger.LogWarning("Recurring donation {Id} failed (attempt {FailedAttempts}/{MaxAttempts}). Scheduled for retry on {NextProcessDate}. Error: {ErrorMessage}",
                    recurringDonation.Id, recurringDonation.FailedAttemptsCount, MaxFailedAttempts, nextProcessDate, errorMessage);
            }

            await _recurringDonationRepository.UpdateAsync(recurringDonation, "RecurringDonationService");
        }

        /// <summary>
        /// Sends a thank you email for recurring donations.
        /// </summary>
        private async Task SendRecurringDonationThankYouEmailAsync(RecurringDonation recurringDonation, Donation donation)
        {
            try
            {
                if (recurringDonation.Donor?.User == null)
                    return;

                var placeholders = new Dictionary<string, string>
                {
                    ["donorName"] = recurringDonation.Donor.User.Profile?.FullName ?? "Valued Donor",
                    ["donationAmountInDollars"] = donation.DonationAmount.ToString("C"),
                    ["frequency"] = recurringDonation.Frequency.ToString().ToLower(),
                    ["nextDonationDate"] = recurringDonation.NextProcessDate.ToString("MMMM d, yyyy")
                };

                bool emailSent = await _messageService.SendTemplatedMessageByNameAsync(
                    templateName: "Recurring Donation Thank You Email",
                    to: recurringDonation.Donor.User.Email,
                    placeholderValues: placeholders
                );

                if (emailSent)
                {
                    _logger.LogInformation("Thank you email sent successfully for recurring donation {Id}", recurringDonation.Id);
                }
                else
                {
                    _logger.LogWarning("Failed to send thank you email for recurring donation {Id}", recurringDonation.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending thank you email for recurring donation {Id}: {ErrorMessage}", 
                    recurringDonation.Id, ex.Message);
            }
        }

        #endregion
    }
}