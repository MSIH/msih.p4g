/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using msih.p4g.Server.Features.Base.PayoutService.Interfaces;
using msih.p4g.Server.Features.Base.PayoutService.Models.Configuration;
using msih.p4g.Server.Features.Base.PayoutService.Models.PayPal;

namespace msih.p4g.Server.Features.Base.PayoutService.Services
{
    /// <summary>
    /// Service for direct PayPal API interactions
    /// </summary>
    public class PayPalApiClient : IPayPalApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PayPalApiClient> _logger;
        private readonly PayPalOptions _payPalOptions;
        private string? _accessToken;
        private DateTime _tokenExpiration = DateTime.MinValue;

        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        /// <summary>
        /// Constructor for PayPalApiClient
        /// </summary>
        public PayPalApiClient(
            HttpClient httpClient,
            IOptions<PayPalOptions> payPalOptions,
            ILogger<PayPalApiClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _payPalOptions = payPalOptions?.Value ?? throw new ArgumentNullException(nameof(payPalOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get an access token from PayPal OAuth
        /// </summary>
        public async Task<PayPalTokenResponse> GetAccessTokenAsync()
        {
            // Check if we already have a valid token
            if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiration)
            {
                return new PayPalTokenResponse
                {
                    AccessToken = _accessToken,
                    TokenType = "Bearer",
                    ExpiresIn = (int)(_tokenExpiration - DateTime.UtcNow).TotalSeconds
                };
            }

            try
            {
                _logger.LogInformation("Requesting new PayPal access token");

                // Prepare the request
                var authString = $"{_payPalOptions.ClientId}:{_payPalOptions.Secret}";
                var encodedAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes(authString));

                using var request = new HttpRequestMessage(HttpMethod.Post, $"{_payPalOptions.ApiUrl}/v1/oauth2/token");
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", encodedAuth);
                request.Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials")
                });

                // Send the request
                using var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                // Parse the response
                var content = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<PayPalTokenResponse>(content, _jsonOptions);

                if (tokenResponse == null)
                {
                    throw new InvalidOperationException("Failed to deserialize PayPal token response");
                }

                // Cache the token
                _accessToken = tokenResponse.AccessToken;
                _tokenExpiration = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60); // Expire 1 minute early to be safe

                _logger.LogInformation("Received new PayPal access token, expires in {ExpiresIn} seconds", tokenResponse.ExpiresIn);

                return tokenResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting PayPal access token");
                throw;
            }
        }

        /// <summary>
        /// Create a payout (single or batch) through PayPal API
        /// </summary>
        public async Task<PayPalPayoutResponse> CreatePayoutAsync(PayPalPayoutRequest request)
        {
            try
            {
                _logger.LogInformation("Creating PayPal payout with {ItemCount} items", request.Items.Count);

                // Ensure we have a valid token
                var tokenResponse = await GetAccessTokenAsync();

                // Prepare the request
                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_payPalOptions.ApiUrl}/v1/payments/payouts");
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);
                
                // Serialize the request body
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

                // Send the request
                using var response = await _httpClient.SendAsync(httpRequest);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("PayPal payout creation failed with status {StatusCode}: {Content}", 
                        response.StatusCode, content);
                    throw new HttpRequestException($"PayPal API error: {response.StatusCode}, {content}");
                }

                // Parse the response
                var payoutResponse = JsonSerializer.Deserialize<PayPalPayoutResponse>(content, _jsonOptions);

                if (payoutResponse == null)
                {
                    throw new InvalidOperationException("Failed to deserialize PayPal payout response");
                }

                _logger.LogInformation("PayPal payout created successfully with batch ID {BatchId}", 
                    payoutResponse.BatchHeader.PayoutBatchId);

                return payoutResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PayPal payout");
                throw;
            }
        }

        /// <summary>
        /// Get the status of a batch payout
        /// </summary>
        public async Task<PayPalBatchStatus> GetBatchPayoutStatusAsync(string batchId)
        {
            try
            {
                _logger.LogInformation("Getting status for PayPal batch payout {BatchId}", batchId);

                // Ensure we have a valid token
                var tokenResponse = await GetAccessTokenAsync();

                // Prepare the request
                using var request = new HttpRequestMessage(HttpMethod.Get, $"{_payPalOptions.ApiUrl}/v1/payments/payouts/{batchId}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

                // Send the request
                using var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("PayPal batch status request failed with status {StatusCode}: {Content}", 
                        response.StatusCode, content);
                    throw new HttpRequestException($"PayPal API error: {response.StatusCode}, {content}");
                }

                // Parse the response
                var batchStatus = JsonSerializer.Deserialize<PayPalBatchStatus>(content, _jsonOptions);

                if (batchStatus == null)
                {
                    throw new InvalidOperationException("Failed to deserialize PayPal batch status response");
                }

                _logger.LogInformation("Retrieved PayPal batch status for {BatchId}: {Status}", 
                    batchId, batchStatus.BatchHeader.BatchStatus);

                return batchStatus;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting PayPal batch payout status for {BatchId}", batchId);
                throw;
            }
        }

        /// <summary>
        /// Get the details of a payout item
        /// </summary>
        public async Task<PayPalBatchStatusItem> GetPayoutItemDetailsAsync(string payoutItemId)
        {
            try
            {
                _logger.LogInformation("Getting details for PayPal payout item {PayoutItemId}", payoutItemId);

                // Ensure we have a valid token
                var tokenResponse = await GetAccessTokenAsync();

                // Prepare the request
                using var request = new HttpRequestMessage(HttpMethod.Get, $"{_payPalOptions.ApiUrl}/v1/payments/payouts-item/{payoutItemId}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

                // Send the request
                using var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("PayPal payout item details request failed with status {StatusCode}: {Content}", 
                        response.StatusCode, content);
                    throw new HttpRequestException($"PayPal API error: {response.StatusCode}, {content}");
                }

                // Parse the response
                var itemDetails = JsonSerializer.Deserialize<PayPalBatchStatusItem>(content, _jsonOptions);

                if (itemDetails == null)
                {
                    throw new InvalidOperationException("Failed to deserialize PayPal payout item details response");
                }

                _logger.LogInformation("Retrieved PayPal payout item details for {PayoutItemId}: {Status}", 
                    payoutItemId, itemDetails.TransactionStatus);

                return itemDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting PayPal payout item details for {PayoutItemId}", payoutItemId);
                throw;
            }
        }
    }
}
