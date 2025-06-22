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
using msih.p4g.Shared.Models.PayoutService;
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
        public async Task<Payout> CreatePayoutAsync(string fundraiserId, string paypalEmail, decimal amount, string currency = "USD", string? notes = null)
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
                PayoutAccountType = msih.p4g.Server.Features.FundraiserService.Model.AccountType.PayPal,
                PayoutAccountFormat = msih.p4g.Server.Features.FundraiserService.Model.AccountFormat.Email,
                Amount = amount,
                Currency = currency,
                Status = PayoutStatus.Pending,
                ErrorMessage = null,
                CreatedBy = "System",
                IsBatchPayout = false
            };

            await _payoutRepository.AddAsync(payout);

            _logger.LogInformation("Created new Payout record {PayoutId} for fundraiser {FundraiserId}", payout.Id, fundraiserId);

            return payout;
        }

        /// <summary>
        /// Process a pending Payout through PayPal
        /// </summary>
        public async Task<Payout> ProcessPayoutAsync(string payoutId)
        {
            // Convert string ID to int
            if (!int.TryParse(payoutId, out int id))
            {
                throw new ArgumentException($"Invalid payout ID format: {payoutId}", nameof(payoutId));
            }

            var payout = await _payoutRepository.GetByIdAsync(id);

            if (payout == null)
                throw new KeyNotFoundException($"Payout with ID {payoutId} not found");

            if (payout.Status != PayoutStatus.Pending)
                throw new InvalidOperationException($"Payout {payoutId} has already been processed with status {payout.Status}");

            try
            {
                _logger.LogInformation("Processing Payout {PayoutId} for fundraiser {FundraiserId}", payout.Id, payout.FundraiserId);

                // Update Payout status to processing
                payout.Status = PayoutStatus.Processing;
                await _payoutRepository.UpdateAsync(payout);

                // Ensure we have a valid access token
                await EnsureAccessTokenAsync();

                // Create the payout
                var payoutResponse = await CreatePayPalPayoutAsync(payout);

                // Update Payout with PayPal response data
                payout.PaypalBatchId = payoutResponse.BatchId;
                payout.Status = PayoutStatus.Completed;
                payout.ProcessedAt = DateTime.UtcNow;

                await _payoutRepository.UpdateAsync(payout);

                _logger.LogInformation("Successfully processed Payout {PayoutId} with PayPal batch ID {BatchId}", payout.Id, payout.PaypalBatchId);

                return payout;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Payout {PayoutId}: {ErrorMessage}", payout.Id, ex.Message);

                // Update Payout with error
                payout.Status = PayoutStatus.Failed;
                payout.ErrorMessage = ex.Message;
                await _payoutRepository.UpdateAsync(payout);

                return payout;
            }
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

            var payouts = await _payoutRepository.GetPayoutsByIdsAsync(payoutIds);

            if (payouts.Count == 0)
                throw new KeyNotFoundException("None of the specified Payout IDs were found");

            // Validate all Payouts
            var invalidPayouts = payouts.Where(p => p.Status != PayoutStatus.Pending).ToList();
            if (invalidPayouts.Any())
            {
                var invalidIds = string.Join(", ", invalidPayouts.Select(p => p.Id));
                throw new InvalidOperationException($"Some Payouts have already been processed: {invalidIds}");
            }

            try
            {
                _logger.LogInformation("Processing batch of {Count} Payouts", payouts.Count);

                // Update Payout statuses to processing
                foreach (var payout in payouts)
                {
                    payout.Status = PayoutStatus.Processing;
                    payout.IsBatchPayout = true;
                }

                await _payoutRepository.UpdateRangeAsync(payouts);

                // Ensure we have a valid access token
                await EnsureAccessTokenAsync();

                // Create the batch payout
                var payoutResponse = await CreatePayPalBatchPayoutAsync(payouts);

                // Update Payouts with PayPal response data
                foreach (var payout in payouts)
                {
                    payout.PaypalBatchId = payoutResponse.BatchId;
                    payout.Status = PayoutStatus.Completed;
                    payout.ProcessedAt = DateTime.UtcNow;
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
                    payout.Status = PayoutStatus.Failed;
                    payout.ErrorMessage = ex.Message;
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
        public async Task<List<Payout>> GetPayoutsByStatusAsync(PayoutStatus status, int page = 1, int pageSize = 20)
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

                var batchResponse = JsonSerializer.Deserialize<JsonElement>(content);
                return ParseBatchStatus(batchResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting batch status for {BatchId}: {ErrorMessage}", batchId, ex.Message);
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

            var payoutRequest = new
            {
                sender_batch_header = new
                {
                    sender_batch_id = senderBatchId,
                    email_subject = "You have a payout from your fundraising campaign",
                    email_message = "Thank you for your fundraising efforts. This is your payout."
                },
                items = new[]
                {
                    new
                    {
                        recipient_type = "EMAIL",
                        amount = new
                        {
                            value = payout.Amount.ToString("F2"),
                            currency = payout.Currency
                        },
                        note = string.IsNullOrEmpty(payout.Notes) ? "Fundraiser payout" : payout.Notes,
                        receiver = payout.PayoutAccount,
                        sender_item_id = payout.Id.ToString()
                    }
                }
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
                BatchId = payoutResponse.GetProperty("batch_header").GetProperty("payout_batch_id").GetString()
            };
        }

        /// <summary>
        /// Create a PayPal batch payout for multiple Payouts
        /// </summary>
        private async Task<PayPalPayoutResponse> CreatePayPalBatchPayoutAsync(List<Payout> payouts)
        {
            _logger.LogInformation("Creating PayPal batch payout for {Count} Payouts", payouts.Count);

            // Create unique sender_batch_id for this batch payout
            var senderBatchId = $"MSIH_BATCH_PAYOUT_{Guid.NewGuid().ToString("N")}";

            var items = payouts.Select(payout => new
            {
                recipient_type = "EMAIL",
                amount = new
                {
                    value = payout.Amount.ToString("F2"),
                    currency = payout.Currency
                },
                note = string.IsNullOrEmpty(payout.Notes) ? "Fundraiser payout" : payout.Notes,
                receiver = payout.PayoutAccount,
                sender_item_id = payout.Id.ToString()
            }).ToArray();

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
                BatchId = payoutResponse.GetProperty("batch_header").GetProperty("payout_batch_id").GetString()
            };
        }

        /// <summary>
        /// Parse the batch status response from PayPal
        /// </summary>
        private PayPalBatchStatus ParseBatchStatus(JsonElement batchResponse)
        {
            var batchHeader = batchResponse.GetProperty("batch_header");
            var batchStatus = new PayPalBatchStatus
            {
                BatchId = batchHeader.GetProperty("payout_batch_id").GetString(),
                Status = batchHeader.GetProperty("batch_status").GetString(),
                TotalAmount = decimal.Parse(batchHeader.GetProperty("amount").GetProperty("value").GetString()),
                Currency = batchHeader.GetProperty("amount").GetProperty("currency").GetString()
            };

            if (batchHeader.TryGetProperty("time_created", out var timeCreated))
            {
                batchStatus.BatchCreationTime = DateTime.Parse(timeCreated.GetString());
            }

            if (batchHeader.TryGetProperty("time_completed", out var timeCompleted))
            {
                batchStatus.BatchCompletionTime = DateTime.Parse(timeCompleted.GetString());
            }

            // Parse item details if available
            if (batchResponse.TryGetProperty("items", out var items))
            {
                foreach (var item in items.EnumerateArray())
                {
                    var payoutItem = new PayPalBatchItemStatus
                    {
                        PayoutItemId = item.GetProperty("payout_item_id").GetString(),
                        Status = item.GetProperty("transaction_status").GetString(),
                        SenderItemId = item.GetProperty("payout_item").GetProperty("sender_item_id").GetString(),
                        Amount = decimal.Parse(item.GetProperty("payout_item").GetProperty("amount").GetProperty("value").GetString()),
                        Currency = item.GetProperty("payout_item").GetProperty("amount").GetProperty("currency").GetString(),
                        ReceiverEmail = item.GetProperty("payout_item").GetProperty("receiver").GetString()
                    };

                    if (item.TryGetProperty("transaction_id", out var transactionId))
                    {
                        payoutItem.TransactionId = transactionId.GetString();
                    }

                    if (item.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                    {
                        payoutItem.ErrorMessage = errors.GetProperty("message").GetString();
                    }

                    batchStatus.Items.Add(payoutItem);
                }
            }

            // Calculate success and error counts
            batchStatus.SuccessCount = batchStatus.Items.Count(i => i.Status == "SUCCESS");
            batchStatus.ErrorCount = batchStatus.Items.Count(i => i.Status == "FAILED" || i.Status == "BLOCKED" || i.Status == "RETURNED");

            return batchStatus;
        }

        /// <summary>
        /// Simple response class for PayPal payout response
        /// </summary>
        private class PayPalPayoutResponse
        {
            public string BatchId { get; set; }
        }

        #endregion
    }
}
