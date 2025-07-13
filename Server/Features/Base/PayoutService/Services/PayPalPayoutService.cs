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
using Microsoft.Extensions.Options;
using msih.p4g.Server.Features.Base.PayoutService.Interfaces;
using msih.p4g.Server.Features.Base.PayoutService.Models;
using msih.p4g.Server.Features.Base.PayoutService.Models.Configuration;
using msih.p4g.Server.Features.Base.PayoutService.Models.PayPal;
using System.Text;
using System.Text.Json;

namespace msih.p4g.Server.Features.Base.PayoutService.Services
{
    /// <summary>
    /// Service for processing payouts to fundraisers via PayPal
    /// </summary>
    public class PayPalPayoutService : IPayoutService
    {
        private readonly IPayoutRepository _payoutRepository;
        private readonly ILogger<PayPalPayoutService> _logger;
        private readonly PayPalOptions _payPalOptions;
        private readonly HttpClient _httpClient;
        private string _accessToken;
        private DateTime _tokenExpiration = DateTime.MinValue;

        /// <summary>
        /// Maximum number of Payouts in a single batch (PayPal limit is 15,000, but we're being conservative)
        /// </summary>
        private const int _maxBatchSize = 1000;

        public PayPalPayoutService(
            IPayoutRepository payoutRepository,
            IOptions<PayPalOptions> payPalOptions,
            ILogger<PayPalPayoutService> logger,
            HttpClient httpClient)
        {
            _payoutRepository = payoutRepository ?? throw new ArgumentNullException(nameof(payoutRepository));
            _payPalOptions = payPalOptions?.Value ?? throw new ArgumentNullException(nameof(payPalOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <summary>
        /// Create a new Payout record
        /// </summary>
        public async Task<Payout> CreatePayoutAsync(
            string fundraiserId,
            string paypalEmail,
            decimal amount,
            string currency = "USD",
            string? notes = null,
            msih.p4g.Server.Features.FundraiserService.Model.AccountType accountType = msih.p4g.Server.Features.FundraiserService.Model.AccountType.PayPal,
            msih.p4g.Server.Features.FundraiserService.Model.AccountFormat accountFormat = msih.p4g.Server.Features.FundraiserService.Model.AccountFormat.Email)
        {
            if (string.IsNullOrEmpty(fundraiserId))
                throw new ArgumentException("Fundraiser ID is required", nameof(fundraiserId));

            if (string.IsNullOrEmpty(paypalEmail))
                throw new ArgumentException("PayPal email is required", nameof(paypalEmail));

            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero", nameof(amount));

            var payout = new Payout
            {
                FundraiserId = fundraiserId,
                PayoutAccount = paypalEmail,
                PayoutAccountType = accountType,
                PayoutAccountFormat = accountFormat,
                Amount = amount,
                Currency = currency,
                ErrorMessage = null,
                CreatedBy = "PayoutService",
                IsBatchPayout = false
            };

            await _payoutRepository.AddAsync(payout);

            _logger.LogInformation("Created new Payout record {PayoutId} for fundraiser {FundraiserId}", payout.Id, fundraiserId);

            return payout;
        }

        /// <summary>
        /// Process multiple Payouts as a batch payout
        /// </summary>
        public async Task<List<Payout>> ProcessBatchPayoutsAsync(List<string> payoutIds)
        {
            if (payoutIds == null || !payoutIds.Any())
                throw new ArgumentException("At least one Payout ID must be provided", nameof(payoutIds));

            if (payoutIds.Count > _maxBatchSize)
                throw new ArgumentException($"Batch size exceeds maximum allowed ({_maxBatchSize})", nameof(payoutIds));

            // 1. Get Payouts from list of Ids
            var payouts = await _payoutRepository.GetPayoutsByIdsAsync(payoutIds);

            if (payouts.Count == 0)
                throw new KeyNotFoundException("None of the specified Payout IDs were found");

            // 2. Validate all Payouts do not have senderid, should be null - todo getpayouts method should return payment with null batch status
            var invalidPayouts = payouts.Where(p => p.PaypalSenderId != null).ToList();
            if (invalidPayouts.Any())
            {
                var invalidIds = string.Join(", ", invalidPayouts.Select(p => p.Id));
                throw new InvalidOperationException($"Some Payouts have already been processed: {invalidIds}");
            }

            // Declare payoutResponse outside the try-catch blocks so it's available in both
            PayPalPayoutResponse payoutResponse = null;


            try
            {
                _logger.LogInformation("Processing batch of {Count} Payouts", payouts.Count);

                // Ensure we have a valid access token
                await EnsureAccessTokenAsync();

                // 3. Create the batch payout
                payoutResponse = await CreatePayPalBatchPayoutAsync(payouts);

                // 4. Update Payouts with PayPal response data
                foreach (var payout in payouts)
                {
                    payout.PaypalBatchId = payoutResponse.BatchId;
                    payout.ProcessedAt = DateTime.UtcNow;
                    payout.BatchStatus = payoutResponse.BatchStatus; // Updated to get this from the response
                    payout.IsBatchPayout = true;
                    payout.PaypalSenderId = payoutResponse.SenderBatchId; // Use the sender batch ID for tracking
                }

                await _payoutRepository.UpdateRangeAsync(payouts);

                _logger.LogInformation("Successfully processed batch of {Count} Payouts with PayPal batch ID {BatchId}",
                    payouts.Count, payoutResponse.BatchId);

                return payouts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing batch of {Count} Payouts: {ErrorMessage}", payouts.Count, ex.Message);

                // Update Payouts with error
                foreach (var payout in payouts)
                {

                    payout.ErrorMessage = ex.Message;
                    payout.PaypalBatchId = payoutResponse.BatchId;
                    payout.ProcessedAt = DateTime.UtcNow;
                    payout.BatchStatus = PayPalBatchStatusEnum.ERROR; // Updated to get this from the response
                    payout.IsBatchPayout = true;
                    payout.PaypalSenderId = payoutResponse.SenderBatchId; // Use the sender batch ID for tracking
                }

                await _payoutRepository.UpdateRangeAsync(payouts);

                return payouts;
            }
        }

        /// <summary>
        /// Get a Payout by ID
        /// </summary>
        public async Task<Payout> GetPayoutAsync(string payoutId)
        {
            // Convert string ID to int
            if (!int.TryParse(payoutId, out int id))
            {
                throw new ArgumentException($"Invalid payout ID format: {payoutId}", nameof(payoutId));
            }

            var payout = await _payoutRepository.GetByIdAsync(id);

            if (payout == null)
                throw new KeyNotFoundException($"Payout with ID {payoutId} not found");

            return payout;
        }

        /// <summary>
        /// Get Payout history for a fundraiser
        /// </summary>
        public async Task<List<Payout>> GetFundraiserPayoutHistoryAsync(string fundraiserId, int page = 1, int pageSize = 20)
        {
            return await _payoutRepository.GetPayoutsByFundraiserIdAsync(fundraiserId, page, pageSize);
        }

        /// <summary>
        /// Get all Payouts with a specific status
        /// </summary>
        public async Task<List<Payout>> GetPayoutsByStatusAsync(PayPalTransactionStatusEnum status, int page = 1, int pageSize = 20)
        {
            return await _payoutRepository.GetPayoutsByStatusAsync(status, page, pageSize);
        }

        /// <summary>
        /// Get the status of a PayPal batch payout
        /// </summary>
        public async Task<PayPalBatchStatus> GetBatchPayoutStatusAsync(string batchId)
        {
            if (string.IsNullOrEmpty(batchId))
                throw new ArgumentException("Batch ID is required", nameof(batchId));

            try
            {
                _logger.LogInformation("Getting status for batch {BatchId}", batchId);

                // Ensure we have a valid access token
                await EnsureAccessTokenAsync();

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"{_payPalOptions.ApiUrl}/v1/Payouts/payouts/{batchId}"),
                    Headers =
                    {
                        { "Authorization", $"Bearer {_accessToken}" }
                    }
                };

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("PayPal API error: {StatusCode} - {Response}", response.StatusCode, content);
                    throw new Exception($"PayPal API error: {response.StatusCode} - {content}");
                }

                // https://developer.paypal.com/docs/api/payments.payouts-batch/v1/#definition-payout_batch
                var batchResponse = JsonSerializer.Deserialize<PayPalBatchStatus>(content);
                return batchResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting batch status for {BatchId}: {ErrorMessage}", batchId, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get all payouts for a fundraiser
        /// </summary>
        public async Task<List<Payout>> GetPayoutsByFundraiserIdAsync(string fundraiserId)
        {
            return await _payoutRepository.GetPayoutsByFundraiserIdAsync(fundraiserId);
        }

        /// <summary>
        /// Updates the status of all payouts in a batch by querying PayPal for the current status
        /// </summary>
        /// <param name="batchId">The PayPal batch ID</param>
        /// <returns>List of updated Payout records</returns>
        public async Task<List<Payout>> UpdateBatchPayoutStatusAsync(string batchId)
        {
            if (string.IsNullOrEmpty(batchId))
                throw new ArgumentException("Batch ID is required", nameof(batchId));

            try
            {
                _logger.LogInformation("Updating status for all payouts in batch {BatchId}", batchId);

                // 1. Get the current batch status from PayPal
                PayPalBatchStatus batchStatus = await GetBatchPayoutStatusAsync(batchId);

                // 2. Get all payouts that belong to this batch
                var payouts = await _payoutRepository.GetPayoutsByBatchIdAsync(batchId);

                if (!payouts.Any())
                {
                    _logger.LogWarning("No payouts found for batch {BatchId}", batchId);
                    return new List<Payout>();
                }

                _logger.LogInformation("Found {Count} payouts for batch {BatchId}", payouts.Count, batchId);

                // 3. Update each payout with the current status
                foreach (var payout in payouts)
                {
                    // Update the batch-level status
                    payout.BatchStatus = Enum.Parse<PayPalBatchStatusEnum>(batchStatus.BatchHeader.BatchStatus);

                    // Find the matching item in the batch details (if available)
                    var payoutItem = batchStatus.Items?.FirstOrDefault(i => i.SenderItemId == payout.Id.ToString());
                    if (payoutItem != null)
                    {
                        // Update item-level details
                        payout.PaypalPayoutItemId = payoutItem.PayoutItemId;
                        payout.PaypalTransactionId = payoutItem.TransactionId;
                        payout.TransactionStatus = Enum.Parse<PayPalTransactionStatusEnum>(payoutItem.TransactionStatus);

                        // If the item has an error, update the error message
                        if (payoutItem.Errors != null)
                        {
                            payout.ErrorMessage = payoutItem.Errors.Message;
                        }

                        if (payoutItem.PayoutItemFee != null)
                        {
                            // Convert fee value to decimal for storage in the Payout entity
                            if (decimal.TryParse(payoutItem.PayoutItemFee.Value, out decimal feeAmount))
                            {
                                payout.FeeAmount = feeAmount;
                            }
                            else
                            {
                                _logger.LogWarning("Could not parse fee amount '{FeeValue}' for payout {PayoutId}",
                                    payoutItem.PayoutItemFee.Value, payout.Id);
                            }
                        }
                    }
                }

                // 4. Save updates to the database
                await _payoutRepository.UpdateRangeAsync(payouts);

                _logger.LogInformation("Successfully updated status for {Count} payouts in batch {BatchId}", payouts.Count, batchId);

                return payouts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating batch status for {BatchId}: {ErrorMessage}", batchId, ex.Message);
                throw;
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Ensure we have a valid access token for PayPal API
        /// </summary>
        private async Task EnsureAccessTokenAsync()
        {
            if (_accessToken != null && _tokenExpiration > DateTime.UtcNow)
                return;

            _logger.LogInformation("Getting new PayPal access token");

            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_payPalOptions.ClientId}:{_payPalOptions.Secret}"));

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{_payPalOptions.ApiUrl}/v1/oauth2/token"),
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" }
                }),
                Headers =
                {
                    { "Authorization", $"Basic {auth}" }
                }
            };

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(content);

            _accessToken = tokenResponse.GetProperty("access_token").GetString();
            var expiresIn = tokenResponse.GetProperty("expires_in").GetInt32();

            // Set expiration with a safety margin
            _tokenExpiration = DateTime.UtcNow.AddSeconds(expiresIn - 60);

            _logger.LogInformation("Received new PayPal access token, expires in {ExpiresIn} seconds", expiresIn);
        }

        /// <summary>
        /// Create a PayPal payout for a single Payout
        /// </summary>
        private async Task<PayPalPayoutResponse> CreatePayPalPayoutAsync(Payout payout)
        {
            _logger.LogInformation("Creating PayPal payout for Payout {PayoutId}", payout.Id);

            // Create unique sender_batch_id for this payout
            var senderBatchId = $"MSIH_PAYOUT_{Guid.NewGuid().ToString("N")}";

            var payoutItem = CreatePayoutItem(payout);

            var payoutRequest = new
            {
                sender_batch_header = new
                {
                    sender_batch_id = senderBatchId,
                    email_subject = "You have a payout from your fundraising campaign",
                    email_message = "Thank you for your fundraising efforts. This is your payout."
                },
                items = new[] { payoutItem }
            };

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{_payPalOptions.ApiUrl}/v1/Payouts/payouts"),
                Content = new StringContent(JsonSerializer.Serialize(payoutRequest), Encoding.UTF8, "application/json"),
                Headers =
                {
                    { "Authorization", $"Bearer {_accessToken}" }
                }
            };

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PayPal API error: {StatusCode} - {Response}", response.StatusCode, content);
                throw new Exception($"PayPal API error: {response.StatusCode} - {content}");
            }

            var payoutResponse = JsonSerializer.Deserialize<JsonElement>(content);

            return new PayPalPayoutResponse
            {
                BatchId = payoutResponse.GetProperty("batch_header").GetProperty("payout_batch_id").GetString(),
                BatchStatus = Enum.Parse<PayPalBatchStatusEnum>(payoutResponse.GetProperty("batch_header").GetProperty("batch_status").GetString()),
                SenderBatchId = senderBatchId
            };
        }

        /// <summary>
        /// Create a PayPal batch payout for multiple Payouts
        /// </summary>
        private async Task<PayPalPayoutResponse> CreatePayPalBatchPayoutAsync(List<Payout> payouts)
        {
            _logger.LogInformation("Creating PayPal batch payout for {Count} Payouts", payouts.Count);

            // Create unique sender_batch_id for this batch payout from datestamp yyyymmddMMss        

            var senderBatchId = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

            var items = payouts.Select(payout => CreatePayoutItem(payout)).ToArray();

            var payoutRequest = new
            {
                sender_batch_header = new
                {
                    sender_batch_id = senderBatchId,
                    email_subject = "You have a payout from your fundraising campaign",
                    email_message = "Thank you for your fundraising efforts. This is your payout."
                },
                items
            };

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{_payPalOptions.ApiUrl}/v1/Payouts/payouts"),
                Content = new StringContent(JsonSerializer.Serialize(payoutRequest), Encoding.UTF8, "application/json"),
                Headers =
                {
                    { "Authorization", $"Bearer {_accessToken}" }
                }
            };

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PayPal API error: {StatusCode} - {Response}", response.StatusCode, content);
                throw new Exception($"PayPal API error: {response.StatusCode} - {content}");
            }

            var payoutResponse = JsonSerializer.Deserialize<JsonElement>(content);

            return new PayPalPayoutResponse
            {
                BatchId = payoutResponse.GetProperty("batch_header").GetProperty("payout_batch_id").GetString(),
                BatchStatus = Enum.Parse<PayPalBatchStatusEnum>(payoutResponse.GetProperty("batch_header").GetProperty("batch_status").GetString()),
                SenderBatchId = senderBatchId
            };
        }

        /// <summary>
        /// Creates a payout item based on the payout account type and format
        /// </summary>
        private object CreatePayoutItem(Payout payout)
        {
            var item = new Dictionary<string, object>
            {
                ["amount"] = new
                {
                    value = payout.Amount.ToString("F2"),
                    currency = payout.Currency
                },
                ["note"] = string.IsNullOrEmpty(payout.Notes) ? "Fundraiser payout" : payout.Notes,
                ["sender_item_id"] = payout.Id.ToString(),
                ["receiver"] = payout.PayoutAccount
            };

            // Set recipient_type based on account format
            switch (payout.PayoutAccountFormat)
            {
                case msih.p4g.Server.Features.FundraiserService.Model.AccountFormat.Mobile:
                    item["recipient_type"] = "PHONE";
                    break;
                case msih.p4g.Server.Features.FundraiserService.Model.AccountFormat.Email:
                    item["recipient_type"] = "EMAIL";
                    break;
                case msih.p4g.Server.Features.FundraiserService.Model.AccountFormat.Handle:
                    // User handle (or) Username associated with Venmo account
                    item["recipient_type"] = "USER_HANDLE";
                    break;
                default:
                    item["recipient_type"] = "EMAIL"; // Default to email if not specified
                    break;
            }

            // Set wallet type based on account type
            switch (payout.PayoutAccountType)
            {
                case msih.p4g.Server.Features.FundraiserService.Model.AccountType.PayPal:
                    item["recipient_wallet"] = "PAYPAL";
                    break;
                case msih.p4g.Server.Features.FundraiserService.Model.AccountType.Venmo:
                    item["recipient_wallet"] = "VENMO";
                    break;
            }

            return item;
        }

        /// <summary>
        /// Simple response class for PayPal payout response
        /// </summary>
        private class PayPalPayoutResponse
        {
            public string BatchId { get; set; }
            public PayPalBatchStatusEnum BatchStatus { get; set; }
            public string SenderBatchId { get; set; }
        }

        #endregion
    }
}
