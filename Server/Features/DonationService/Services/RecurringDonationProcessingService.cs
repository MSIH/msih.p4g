/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

using msih.p4g.Server.Features.Base.PaymentService.Interfaces;
using msih.p4g.Server.Features.Base.PaymentService.Models;
using msih.p4g.Server.Features.DonationService.Interfaces;
using msih.p4g.Server.Features.DonationService.Models;

namespace msih.p4g.Server.Features.DonationService.Services
{
    /// <summary>
    /// Background service that processes recurring donations automatically.
    /// </summary>
    public class RecurringDonationProcessingService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RecurringDonationProcessingService> _logger;
        private readonly TimeSpan _processInterval = TimeSpan.FromHours(1); // Process every hour

        public RecurringDonationProcessingService(
            IServiceProvider serviceProvider,
            ILogger<RecurringDonationProcessingService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Recurring Donation Processing Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessRecurringDonationsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing recurring donations: {ErrorMessage}", ex.Message);
                }

                // Wait for the next processing interval
                try
                {
                    await Task.Delay(_processInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected when the service is being stopped
                    break;
                }
            }

            _logger.LogInformation("Recurring Donation Processing Service stopped");
        }

        private async Task ProcessRecurringDonationsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var donationService = scope.ServiceProvider.GetRequiredService<IDonationService>();
            var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();

            try
            {
                _logger.LogDebug("Starting recurring donation processing cycle");

                // Get all active recurring donations that are due for processing
                var donations = await donationService.GetAllAsync();
                var recurringDonations = donations.Where(d => 
                    d.IsActive && 
                    (d.IsMonthly || d.IsAnnual) && 
                    d.NextProcessDate.HasValue && 
                    d.NextProcessDate <= DateTime.UtcNow &&
                    !string.IsNullOrEmpty(d.RecurringPaymentToken)
                ).ToList();

                if (!recurringDonations.Any())
                {
                    _logger.LogDebug("No recurring donations due for processing");
                    return;
                }

                _logger.LogInformation("Processing {Count} recurring donations", recurringDonations.Count);

                int successCount = 0;
                foreach (var recurringDonation in recurringDonations)
                {
                    try
                    {
                        if (await ProcessSingleRecurringDonationAsync(recurringDonation, paymentService, donationService))
                        {
                            successCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing recurring donation {DonationId}: {ErrorMessage}", 
                            recurringDonation.Id, ex.Message);
                    }
                }

                _logger.LogInformation("Successfully processed {SuccessCount}/{TotalCount} recurring donations", 
                    successCount, recurringDonations.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in recurring donation processing cycle: {ErrorMessage}", ex.Message);
            }
        }

        private async Task<bool> ProcessSingleRecurringDonationAsync(
            Donation recurringDonation,
            IPaymentService paymentService,
            IDonationService donationService)
        {
            try
            {
                // Calculate total amount to charge
                decimal amountToCharge = recurringDonation.DonationAmount;
                if (recurringDonation.PayTransactionFee)
                {
                    amountToCharge += recurringDonation.PayTransactionFeeAmount;
                }

                // Process payment using stored payment token
                var paymentRequest = new PaymentRequest
                {
                    Amount = amountToCharge,
                    Currency = "USD",
                    Description = $"Recurring donation - {(recurringDonation.IsMonthly ? "Monthly" : "Annual")}",
                    OrderReference = $"Recurring-{recurringDonation.Id}-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    CustomerEmail = recurringDonation.Donor?.User?.Email ?? "unknown@email.com",
                    PaymentMethodNonce = recurringDonation.RecurringPaymentToken
                };

                var paymentResponse = await paymentService.ProcessPaymentAsync(paymentRequest);

                if (paymentResponse.Success)
                {
                    // Create child donation record for this recurring payment
                    var paymentDonation = new Donation
                    {
                        DonationAmount = recurringDonation.DonationAmount,
                        PayTransactionFeeAmount = recurringDonation.PayTransactionFeeAmount,
                        DonorId = recurringDonation.DonorId,
                        PayTransactionFee = recurringDonation.PayTransactionFee,
                        // Mark as payment records, not recurring setups
                        IsMonthly = false,
                        IsAnnual = false,
                        DonationMessage = recurringDonation.DonationMessage,
                        ReferralCode = recurringDonation.ReferralCode,
                        CampaignCode = recurringDonation.CampaignCode,
                        CampaignId = recurringDonation.CampaignId,
                        PaymentTransactionId = !string.IsNullOrEmpty(paymentResponse.TransactionId) ?
                            (await paymentService.GetTransactionDetailsAsync(paymentResponse.TransactionId))?.Id : null,
                        // Link to parent recurring donation setup
                        ParentRecurringDonationId = recurringDonation.Id,
                        IsActive = true,
                        CreatedBy = "RecurringDonationProcessingService",
                        CreatedOn = DateTime.UtcNow
                    };

                    await donationService.AddAsync(paymentDonation);

                    // Update the original recurring donation's next process date
                    var nextProcessDate = recurringDonation.IsMonthly 
                        ? DateTime.UtcNow.AddMonths(1) 
                        : DateTime.UtcNow.AddYears(1);
                    
                    recurringDonation.NextProcessDate = nextProcessDate;
                    recurringDonation.ModifiedBy = "RecurringDonationProcessingService";
                    recurringDonation.ModifiedOn = DateTime.UtcNow;

                    await donationService.UpdateAsync(recurringDonation);

                    _logger.LogInformation("Successfully processed recurring donation {DonationId} for amount {Amount}. Next process date: {NextProcessDate}", 
                        recurringDonation.Id, recurringDonation.DonationAmount, nextProcessDate);

                    return true;
                }
                else
                {
                    _logger.LogWarning("Payment failed for recurring donation {DonationId}: {ErrorMessage}", 
                        recurringDonation.Id, paymentResponse.ErrorMessage);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing recurring donation {DonationId}: {ErrorMessage}", 
                    recurringDonation.Id, ex.Message);
                return false;
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Stopping Recurring Donation Processing Service...");
            await base.StopAsync(stoppingToken);
        }
    }
}