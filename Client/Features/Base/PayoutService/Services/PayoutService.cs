/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using msih.p4g.Client.Features.Base.PayoutService.Interfaces;
using msih.p4g.Shared.Models.PayoutService;

namespace msih.p4g.Client.Features.Base.PayoutService.Services
{
    /// <summary>
    /// Client-side service for interacting with the PayPal payout API
    /// </summary>
    public class PayoutService : IPayoutService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PayoutService> _logger;
        private const string BaseApiUrl = "api/payout";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpClient">HTTP client for API requests</param>
        /// <param name="logger">Logger</param>
        public PayoutService(HttpClient httpClient, ILogger<PayoutService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Create a new Payout record
        /// </summary>
        public async Task<PayoutDto> CreatePayoutAsync(string fundraiserId, string paypalEmail, decimal amount, string currency = "USD", string? notes = null)
        {
            try
            {
                var request = new
                {
                    FundraiserId = fundraiserId,
                    PaypalEmail = paypalEmail,
                    Amount = amount,
                    Currency = currency,
                    Notes = notes
                };

                var response = await _httpClient.PostAsJsonAsync($"{BaseApiUrl}/create", request);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<PayoutDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payout for fundraiser {FundraiserId}", fundraiserId);
                throw;
            }
        }

        /// <summary>
        /// Process a pending Payout through PayPal
        /// </summary>
        public async Task<PayoutDto> ProcessPayoutAsync(string payoutId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"{BaseApiUrl}/process/{payoutId}", null);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<PayoutDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payout {PayoutId}", payoutId);
                throw;
            }
        }

        /// <summary>
        /// Process multiple Payouts as a batch payout
        /// </summary>
        public async Task<List<PayoutDto>> ProcessBatchPayoutsAsync(List<string> payoutIds)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{BaseApiUrl}/process-batch", payoutIds);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<List<PayoutDto>>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing batch payouts");
                throw;
            }
        }

        /// <summary>
        /// Get a Payout by ID
        /// </summary>
        public async Task<PayoutDto> GetPayoutAsync(string payoutId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<PayoutDto>($"{BaseApiUrl}/{payoutId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payout {PayoutId}", payoutId);
                throw;
            }
        }

        /// <summary>
        /// Get Payout history for a fundraiser
        /// </summary>
        public async Task<List<PayoutDto>> GetFundraiserPayoutHistoryAsync(string fundraiserId, int page = 1, int pageSize = 20)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<PayoutDto>>($"{BaseApiUrl}/fundraiser/{fundraiserId}?page={page}&pageSize={pageSize}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payout history for fundraiser {FundraiserId}", fundraiserId);
                throw;
            }
        }

        /// <summary>
        /// Get all Payouts with a specific status
        /// </summary>
        public async Task<List<PayoutDto>> GetPayoutsByStatusAsync(PayoutStatus status, int page = 1, int pageSize = 20)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<PayoutDto>>($"{BaseApiUrl}/status/{status}?page={page}&pageSize={pageSize}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payouts with status {Status}", status);
                throw;
            }
        }

        /// <summary>
        /// Get the status of a PayPal batch payout
        /// </summary>
        public async Task<PayPalBatchStatusDto> GetBatchPayoutStatusAsync(string batchId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<PayPalBatchStatusDto>($"{BaseApiUrl}/batch-status/{batchId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting batch payout status for batch {BatchId}", batchId);
                throw;
            }
        }
    }
}
