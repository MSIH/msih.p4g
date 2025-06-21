using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Features.Base.PaymentService.Interfaces;
using msih.p4g.Server.Features.Base.PaymentService.Models;
using msih.p4g.Server.Features.Base.PaymentService.Models.Configuration;
using msih.p4g.Shared.Models.PaymentService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace msih.p4g.Server.Features.Base.PaymentService.Services
{
    /// <summary>
    /// Service for processing payouts to fundraisers via PayPal
    /// </summary>
    public class PayPalPayoutService : IPayPalPayoutService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<PayPalPayoutService> _logger;
        private readonly PayPalOptions _payPalOptions;
        private readonly HttpClient _httpClient;
        private string _accessToken;
        private DateTime _tokenExpiration = DateTime.MinValue;

        public PayPalPayoutService(
            ApplicationDbContext dbContext,
            IOptions<PayPalOptions> payPalOptions,
            ILogger<PayPalPayoutService> logger,
            HttpClient httpClient)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _payPalOptions = payPalOptions?.Value ?? throw new ArgumentNullException(nameof(payPalOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <summary>
        /// Create a new payment record
        /// </summary>
        public async Task<PaymentDto> CreatePaymentAsync(string fundraiserId, string paypalEmail, decimal amount, string currency = "USD", string? notes = null)
        {
            if (string.IsNullOrEmpty(fundraiserId))
                throw new ArgumentException("Fundraiser ID is required", nameof(fundraiserId));
            
            if (string.IsNullOrEmpty(paypalEmail))
                throw new ArgumentException("PayPal email is required", nameof(paypalEmail));
            
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero", nameof(amount));

            var payment = new Payment
            {
                FundraiserId = fundraiserId,
                PaypalEmail = paypalEmail,
                Amount = amount,
                Currency = currency,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                Notes = notes
            };

            _dbContext.Payments.Add(payment);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Created new payment record {PaymentId} for fundraiser {FundraiserId}", payment.Id, fundraiserId);

            return MapToDto(payment);
        }

        /// <summary>
        /// Process a pending payment through PayPal
        /// </summary>
        public async Task<PaymentDto> ProcessPaymentAsync(string paymentId)
        {
            var payment = await _dbContext.Payments
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
                throw new KeyNotFoundException($"Payment with ID {paymentId} not found");

            if (payment.Status != PaymentStatus.Pending)
                throw new InvalidOperationException($"Payment {paymentId} has already been processed with status {payment.Status}");

            try
            {
                _logger.LogInformation("Processing payment {PaymentId} for fundraiser {FundraiserId}", payment.Id, payment.FundraiserId);
                
                // Update payment status to processing
                payment.Status = PaymentStatus.Processing;
                await _dbContext.SaveChangesAsync();

                // Ensure we have a valid access token
                await EnsureAccessTokenAsync();

                // Create the payout
                var payoutResponse = await CreatePayPalPayoutAsync(payment);

                // Update payment with PayPal response data
                payment.PaypalBatchId = payoutResponse.BatchId;
                payment.Status = PaymentStatus.Completed;
                payment.ProcessedAt = DateTime.UtcNow;
                
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Successfully processed payment {PaymentId} with PayPal batch ID {BatchId}", payment.Id, payment.PaypalBatchId);
                
                return MapToDto(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment {PaymentId}: {ErrorMessage}", payment.Id, ex.Message);
                
                // Update payment with error
                payment.Status = PaymentStatus.Failed;
                payment.ErrorMessage = ex.Message;
                await _dbContext.SaveChangesAsync();
                
                return MapToDto(payment);
            }
        }

        /// <summary>
        /// Get a payment by ID
        /// </summary>
        public async Task<PaymentDto> GetPaymentAsync(string paymentId)
        {
            var payment = await _dbContext.Payments
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
                throw new KeyNotFoundException($"Payment with ID {paymentId} not found");

            return MapToDto(payment);
        }

        /// <summary>
        /// Get payment history for a fundraiser
        /// </summary>
        public async Task<List<PaymentDto>> GetFundraiserPaymentHistoryAsync(string fundraiserId, int page = 1, int pageSize = 20)
        {
            var payments = await _dbContext.Payments
                .Where(p => p.FundraiserId == fundraiserId)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return payments.Select(MapToDto).ToList();
        }

        /// <summary>
        /// Get all payments with a specific status
        /// </summary>
        public async Task<List<PaymentDto>> GetPaymentsByStatusAsync(PaymentStatus status, int page = 1, int pageSize = 20)
        {
            var payments = await _dbContext.Payments
                .Where(p => p.Status == status)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return payments.Select(MapToDto).ToList();
        }

        #region Private Helper Methods

        /// <summary>
        /// Map from Payment entity to PaymentDto
        /// </summary>
        private static PaymentDto MapToDto(Payment payment)
        {
            return new PaymentDto
            {
                Id = payment.Id,
                FundraiserId = payment.FundraiserId,
                PaypalEmail = payment.PaypalEmail,
                Amount = payment.Amount,
                Currency = payment.Currency,
                Status = payment.Status.ToString(),
                PaypalBatchId = payment.PaypalBatchId,
                PaypalPayoutItemId = payment.PaypalPayoutItemId,
                CreatedAt = payment.CreatedAt,
                ProcessedAt = payment.ProcessedAt,
                Notes = payment.Notes,
                ErrorMessage = payment.ErrorMessage
            };
        }

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
        /// Create a PayPal payout for a payment
        /// </summary>
        private async Task<PayPalPayoutResponse> CreatePayPalPayoutAsync(Payment payment)
        {
            _logger.LogInformation("Creating PayPal payout for payment {PaymentId}", payment.Id);

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
                            value = payment.Amount.ToString("F2"),
                            currency = payment.Currency
                        },
                        note = string.IsNullOrEmpty(payment.Notes) ? "Fundraiser payout" : payment.Notes,
                        receiver = payment.PaypalEmail,
                        sender_item_id = payment.Id
                    }
                }
            };

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{_payPalOptions.ApiUrl}/v1/payments/payouts"),
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
        /// Simple response class for PayPal payout response
        /// </summary>
        private class PayPalPayoutResponse
        {
            public string BatchId { get; set; }
        }

        #endregion
    }
}
