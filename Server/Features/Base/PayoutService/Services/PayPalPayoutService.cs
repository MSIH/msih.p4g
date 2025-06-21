/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using msih.p4g.Server.Features.Base.PayoutService.Interfaces;
using msih.p4g.Server.Features.Base.PayoutService.Models;
using msih.p4g.Server.Features.Base.PayoutService.Models.Configuration;
using msih.p4g.Shared.Models.PayoutService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace msih.p4g.Server.Features.Base.PayoutService.Services
{
    /// <summary>
    /// Service for processing payouts to fundraisers via PayPal
    /// </summary>
    public class PayPalPayoutService : IPayoutService
    {
        private readonly IPayoutRepository _PayoutRepository;
        private readonly ILogger<PayPalPayoutService> _logger;
        private readonly PayPalOptions _payPalOptions;
        private readonly HttpClient _httpClient;
        private string _accessToken;
        private DateTime _tokenExpiration = DateTime.MinValue;

        /// <summary>
        /// Maximum number of Payouts in a single batch (PayPal limit is 15,000, but we're being conservative)
        /// </summary>
        private const int MAX_BATCH_SIZE = 1000;

        public PayPalPayoutService(
            IPayoutRepository PayoutRepository,
            IOptions<PayPalOptions> payPalOptions,
            ILogger<PayPalPayoutService> logger,
            HttpClient httpClient)
        {
            _PayoutRepository = PayoutRepository ?? throw new ArgumentNullException(nameof(PayoutRepository));
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

            var Payout = new Payout
            {
                FundraiserId = fundraiserId,
                PaypalEmail = paypalEmail,
                Amount = amount,
                Currency = currency,
                Status = PayoutStatus.Pending,
                Notes = notes,
                CreatedBy = "System"
            };

            await _PayoutRepository.AddAsync(Payout);

            _logger.LogInformation("Created new Payout record {PayoutId} for fundraiser {FundraiserId}", Payout.Id, fundraiserId);

            return Payout;
        }

        /// <summary>
        /// Process a pending Payout through PayPal
        /// </summary>
        public async Task<Payout> ProcessPayoutAsync(string PayoutId)
        {
            var Payout = await _PayoutRepository.GetByIdAsync(PayoutId);

            if (Payout == null)
                throw new KeyNotFoundException($"Payout with ID {PayoutId} not found");

            if (Payout.Status != PayoutStatus.Pending)
                throw new InvalidOperationException($"Payout {PayoutId} has already been processed with status {Payout.Status}");

            try
            {
                _logger.LogInformation("Processing Payout {PayoutId} for fundraiser {FundraiserId}", Payout.Id, Payout.FundraiserId);
                
                // Update Payout status to processing
                Payout.Status = PayoutStatus.Processing;
                await _PayoutRepository.UpdateAsync(Payout);

                // Ensure we have a valid access token
                await EnsureAccessTokenAsync();

                // Create the payout
                var payoutResponse = await CreatePayPalPayoutAsync(Payout);

                // Update Payout with PayPal response data
                Payout.PaypalBatchId = payoutResponse.BatchId;
                Payout.Status = PayoutStatus.Completed;
                Payout.ProcessedAt = DateTime.UtcNow;
                
                await _PayoutRepository.UpdateAsync(Payout);

                _logger.LogInformation("Successfully processed Payout {PayoutId} with PayPal batch ID {BatchId}", Payout.Id, Payout.PaypalBatchId);
                
                return Payout;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Payout {PayoutId}: {ErrorMessage}", Payout.Id, ex.Message);
                
                // Update Payout with error
                Payout.Status = PayoutStatus.Failed;
                Payout.ErrorMessage = ex.Message;
                await _PayoutRepository.UpdateAsync(Payout);
                
                return Payout;
            }
        }

        /// <summary>
        /// Process multiple Payouts as a batch payout
        /// </summary>
        public async Task<List<Payout>> ProcessBatchPayoutsAsync(List<string> PayoutIds)
        {
            if (PayoutIds == null || !PayoutIds.Any())
                throw new ArgumentException("At least one Payout ID must be provided", nameof(PayoutIds));

            if (PayoutIds.Count > MAX_BATCH_SIZE)
                throw new ArgumentException($"Batch size exceeds maximum allowed ({MAX_BATCH_SIZE})", nameof(PayoutIds));

            var Payouts = await _PayoutRepository.GetPayoutsByIdsAsync(PayoutIds);
            
            if (Payouts.Count == 0)
                throw new KeyNotFoundException("None of the specified Payout IDs were found");

            // Validate all Payouts
            var invalidPayouts = Payouts.Where(p => p.Status != PayoutStatus.Pending).ToList();
            if (invalidPayouts.Any())
            {
                var invalidIds = string.Join(", ", invalidPayouts.Select(p => p.Id));
                throw new InvalidOperationException($"Some Payouts have already been processed: {invalidIds}");
            }

            try
            {
                _logger.LogInformation("Processing batch of {Count} Payouts", Payouts.Count);
                
                // Update Payout statuses to processing
                foreach (var Payout in Payouts)
                {
                    Payout.Status = PayoutStatus.Processing;
                    Payout.IsBatchPayout = true;
                }
                
                await _PayoutRepository.UpdateRangeAsync(Payouts);

                // Ensure we have a valid access token
                await EnsureAccessTokenAsync();

                // Create the batch payout
                var payoutResponse = await CreatePayPalBatchPayoutAsync(Payouts);

                // Update Payouts with PayPal response data
                foreach (var Payout in Payouts)
                {
                    Payout.PaypalBatchId = payoutResponse.BatchId;
                    Payout.Status = PayoutStatus.Completed;
                    Payout.ProcessedAt = DateTime.UtcNow;
                }
                
                await _PayoutRepository.UpdateRangeAsync(Payouts);

                _logger.LogInformation("Successfully processed batch of {Count} Payouts with PayPal batch ID {BatchId}", 
                    Payouts.Count, payoutResponse.BatchId);
                
                return Payouts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing batch of {Count} Payouts: {ErrorMessage}", Payouts.Count, ex.Message);
                
                // Update Payouts with error
                foreach (var Payout in Payouts)
                {
                    Payout.Status = PayoutStatus.Failed;
                    Payout.ErrorMessage = ex.Message;
                }
                
                await _PayoutRepository.UpdateRangeAsync(Payouts);
                
                return Payouts;
            }
        }

        /// <summary>
        /// Get a Payout by ID
        /// </summary>
        public async Task<Payout> GetPayoutAsync(string PayoutId)
        {
            var Payout = await _PayoutRepository.GetByIdAsync(PayoutId);

            if (Payout == null)
                throw new KeyNotFoundException($"Payout with ID {PayoutId} not found");

            return Payout;
        }

        /// <summary>
        /// Get Payout history for a fundraiser
        /// </summary>
        public async Task<List<Payout>> GetFundraiserPayoutHistoryAsync(string fundraiserId, int page = 1, int pageSize = 20)
        {
            return await _PayoutRepository.GetPayoutsByFundraiserIdAsync(fundraiserId, page, pageSize);
        }

        /// <summary>
        /// Get all Payouts with a specific status
        /// </summary>
        public async Task<List<Payout>> GetPayoutsByStatusAsync(PayoutStatus status, int page = 1, int pageSize = 20)
        {
            return await _PayoutRepository.GetPayoutsByStatusAsync(status, page, pageSize);
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
        private async Task<PayPalPayoutResponse> CreatePayPalPayoutAsync(Payout Payout)
        {
            _logger.LogInformation("Creating PayPal payout for Payout {PayoutId}", Payout.Id);

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
                            value = Payout.Amount.ToString("F2"),
                            currency = Payout.Currency
                        },
                        note = string.IsNullOrEmpty(Payout.Notes) ? "Fundraiser payout" : Payout.Notes,
                        receiver = Payout.PaypalEmail,
                        sender_item_id = Payout.Id
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
        private async Task<PayPalPayoutResponse> CreatePayPalBatchPayoutAsync(List<Payout> Payouts)
        {
            _logger.LogInformation("Creating PayPal batch payout for {Count} Payouts", Payouts.Count);

            // Create unique sender_batch_id for this batch payout
            var senderBatchId = $"MSIH_BATCH_PAYOUT_{Guid.NewGuid().ToString("N")}";

            var items = Payouts.Select(Payout => new
            {
                recipient_type = "EMAIL",
                amount = new
                {
                    value = Payout.Amount.ToString("F2"),
                    currency = Payout.Currency
                },
                note = string.IsNullOrEmpty(Payout.Notes) ? "Fundraiser payout" : Payout.Notes,
                receiver = Payout.PaypalEmail,
                sender_item_id = Payout.Id
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
