/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Features.Base.PaymentService.Models;
using msih.p4g.Server.Features.Base.PaymentService.Models.Configuration;
using msih.p4g.Server.Features.Base.PaymentService.Services;
using msih.p4g.Shared.Models.PaymentService;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace msih.p4g.Tests.Server.Tests.Features.Base.PaymentService
{
    public class PayPalPayoutServiceTests
    {
        private readonly Mock<IOptions<PayPalOptions>> _mockOptions;
        private readonly Mock<ILogger<PayPalPayoutService>> _mockLogger;
        private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;

        public PayPalPayoutServiceTests()
        {
            // Setup mock PayPal options
            _mockOptions = new Mock<IOptions<PayPalOptions>>();
            _mockOptions.Setup(o => o.Value).Returns(new PayPalOptions
            {
                ClientId = "test-client-id",
                Secret = "test-secret",
                Environment = "sandbox"
            });

            // Setup mock logger
            _mockLogger = new Mock<ILogger<PayPalPayoutService>>();

            // Setup in-memory database
            _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            // Setup mock HTTP handler
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://api.sandbox.paypal.com")
            };
        }

        [Fact]
        public async Task CreatePaymentAsync_ShouldCreateNewPayment()
        {
            // Arrange
            using var dbContext = new ApplicationDbContext(_dbContextOptions);
            var service = new PayPalPayoutService(dbContext, _mockOptions.Object, _mockLogger.Object, _httpClient);

            // Act
            var result = await service.CreatePaymentAsync(
                fundraiserId: "test-fundraiser-id",
                paypalEmail: "test@example.com",
                amount: 100.50m,
                currency: "USD",
                notes: "Test payment");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test-fundraiser-id", result.FundraiserId);
            Assert.Equal("test@example.com", result.PaypalEmail);
            Assert.Equal(100.50m, result.Amount);
            Assert.Equal("USD", result.Currency);
            Assert.Equal("Pending", result.Status);
            Assert.Equal("Test payment", result.Notes);

            // Verify it was saved to the database
            var savedPayment = await dbContext.Payments.FirstOrDefaultAsync();
            Assert.NotNull(savedPayment);
            Assert.Equal("test-fundraiser-id", savedPayment.FundraiserId);
        }

        [Fact]
        public async Task ProcessPaymentAsync_ShouldProcessPaymentSuccessfully()
        {
            // Arrange
            using var dbContext = new ApplicationDbContext(_dbContextOptions);
            
            // Add a payment to process
            var payment = new Payment
            {
                Id = Guid.NewGuid().ToString(),
                FundraiserId = "test-fundraiser-id",
                PaypalEmail = "test@example.com",
                Amount = 100.50m,
                Currency = "USD",
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                Notes = "Test payment"
            };
            dbContext.Payments.Add(payment);
            await dbContext.SaveChangesAsync();

            // Setup mock HTTP responses
            // 1. Token response
            SetupMockHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(new
            {
                access_token = "test-access-token",
                expires_in = 3600
            }));

            // 2. Payout response
            SetupMockHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(new
            {
                batch_header = new
                {
                    payout_batch_id = "test-batch-id"
                }
            }));

            var service = new PayPalPayoutService(dbContext, _mockOptions.Object, _mockLogger.Object, _httpClient);

            // Act
            var result = await service.ProcessPaymentAsync(payment.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Completed", result.Status);
            Assert.Equal("test-batch-id", result.PaypalBatchId);
            Assert.NotNull(result.ProcessedAt);

            // Verify the payment was updated in the database
            var updatedPayment = await dbContext.Payments.FindAsync(payment.Id);
            Assert.NotNull(updatedPayment);
            Assert.Equal(PaymentStatus.Completed, updatedPayment.Status);
            Assert.Equal("test-batch-id", updatedPayment.PaypalBatchId);
        }

        [Fact]
        public async Task ProcessPaymentAsync_ShouldHandleApiErrors()
        {
            // Arrange
            using var dbContext = new ApplicationDbContext(_dbContextOptions);
            
            // Add a payment to process
            var payment = new Payment
            {
                Id = Guid.NewGuid().ToString(),
                FundraiserId = "test-fundraiser-id",
                PaypalEmail = "test@example.com",
                Amount = 100.50m,
                Currency = "USD",
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                Notes = "Test payment"
            };
            dbContext.Payments.Add(payment);
            await dbContext.SaveChangesAsync();

            // Setup mock HTTP responses
            // 1. Token response
            SetupMockHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(new
            {
                access_token = "test-access-token",
                expires_in = 3600
            }));

            // 2. Error response from PayPal
            SetupMockHttpResponse(HttpStatusCode.BadRequest, JsonSerializer.Serialize(new
            {
                name = "VALIDATION_ERROR",
                message = "Invalid request - see details",
                details = new[]
                {
                    new { field = "amount", issue = "Invalid amount" }
                }
            }));

            var service = new PayPalPayoutService(dbContext, _mockOptions.Object, _mockLogger.Object, _httpClient);

            // Act
            var result = await service.ProcessPaymentAsync(payment.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Failed", result.Status);
            Assert.NotNull(result.ErrorMessage);
            Assert.Contains("PayPal API error", result.ErrorMessage);

            // Verify the payment was updated in the database
            var updatedPayment = await dbContext.Payments.FindAsync(payment.Id);
            Assert.NotNull(updatedPayment);
            Assert.Equal(PaymentStatus.Failed, updatedPayment.Status);
            Assert.NotNull(updatedPayment.ErrorMessage);
        }

        private void SetupMockHttpResponse(HttpStatusCode statusCode, string content)
        {
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(content, Encoding.UTF8, "application/json")
                });
        }
    }
}
