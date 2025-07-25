/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using MSIH.Core.Services.Payout.Models.Configuration;
using MSIH.Core.Services.Payout.Models.PayPal;
using MSIH.Core.Services.Payout.Services;
using Xunit;

namespace msih.p4g.Tests.Server.Features.Base.PayoutService
{
    public class PayPalApiClientTests
    {
        private readonly Mock<IOptions<PayPalOptions>> _mockOptions;
        private readonly Mock<ILogger<PayPalApiClient>> _mockLogger;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly PayPalOptions _payPalOptions;

        public PayPalApiClientTests()
        {
            _payPalOptions = new PayPalOptions
            {
                ClientId = "test-client-id",
                Secret = "test-secret",
                Environment = "sandbox"
            };

            _mockOptions = new Mock<IOptions<PayPalOptions>>();
            _mockOptions.Setup(x => x.Value).Returns(_payPalOptions);

            _mockLogger = new Mock<ILogger<PayPalApiClient>>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        }

        [Fact]
        public async Task GetAccessTokenAsync_ShouldReturnToken_WhenApiCallSucceeds()
        {
            // Arrange
            var tokenResponse = new PayPalTokenResponse
            {
                AccessToken = "test-access-token",
                TokenType = "Bearer",
                ExpiresIn = 32400
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri!.AbsolutePath == "/v1/oauth2/token"),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(tokenResponse))
                });

            var payPalApiClient = new PayPalApiClient(_httpClient, _mockOptions.Object, _mockLogger.Object);

            // Act
            var result = await payPalApiClient.GetAccessTokenAsync();

            // Assert
            Assert.Equal(tokenResponse.AccessToken, result.AccessToken);
            Assert.Equal(tokenResponse.TokenType, result.TokenType);
            Assert.Equal(tokenResponse.ExpiresIn, result.ExpiresIn);
        }

        [Fact]
        public async Task CreatePayoutAsync_ShouldReturnResponse_WhenApiCallSucceeds()
        {
            // Arrange
            var tokenResponse = new PayPalTokenResponse
            {
                AccessToken = "test-access-token",
                TokenType = "Bearer",
                ExpiresIn = 32400
            };

            var payoutResponse = new PayPalPayoutResponse
            {
                BatchHeader = new PayPalBatchHeader
                {
                    PayoutBatchId = "test-batch-id",
                    BatchStatus = "PENDING",
                    SenderBatchHeader = new PayPalSenderBatchHeaderResponse
                    {
                        SenderBatchId = "test-sender-batch-id",
                        EmailSubject = "Test Payout"
                    }
                }
            };

            _mockHttpMessageHandler
                .Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(tokenResponse))
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(payoutResponse))
                });

            var payPalApiClient = new PayPalApiClient(_httpClient, _mockOptions.Object, _mockLogger.Object);
            var request = new PayPalPayoutRequest
            {
                SenderBatchHeader = new PayPalSenderBatchHeader
                {
                    SenderBatchId = "test-sender-batch-id",
                    EmailSubject = "Test Payout"
                },
                Items = new System.Collections.Generic.List<PayPalPayoutItem>
                {
                    new PayPalPayoutItem
                    {
                        RecipientType = "EMAIL",
                        Amount = new PayPalAmount
                        {
                            Currency = "USD",
                            Value = "100.00"
                        },
                        Receiver = "test@example.com",
                        SenderItemId = "test-sender-item-id"
                    }
                }
            };

            // Act
            var result = await payPalApiClient.CreatePayoutAsync(request);

            // Assert
            Assert.Equal(payoutResponse.BatchHeader.PayoutBatchId, result.BatchHeader.PayoutBatchId);
            Assert.Equal(payoutResponse.BatchHeader.BatchStatus, result.BatchHeader.BatchStatus);
        }

        [Fact]
        public async Task GetBatchPayoutStatusAsync_ShouldReturnStatus_WhenApiCallSucceeds()
        {
            // Arrange
            var tokenResponse = new PayPalTokenResponse
            {
                AccessToken = "test-access-token",
                TokenType = "Bearer",
                ExpiresIn = 32400
            };

            var batchStatus = new PayPalBatchStatus
            {
                BatchHeader = new PayPalBatchStatusHeader
                {
                    PayoutBatchId = "test-batch-id",
                    BatchStatus = "SUCCESS",
                    SenderBatchHeader = new PayPalSenderBatchHeaderResponse
                    {
                        SenderBatchId = "test-sender-batch-id"
                    }
                }
            };

            _mockHttpMessageHandler
                .Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(tokenResponse))
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(batchStatus))
                });

            var payPalApiClient = new PayPalApiClient(_httpClient, _mockOptions.Object, _mockLogger.Object);

            // Act
            var result = await payPalApiClient.GetBatchPayoutStatusAsync("test-batch-id");

            // Assert
            Assert.Equal(batchStatus.BatchHeader.PayoutBatchId, result.BatchHeader.PayoutBatchId);
            Assert.Equal(batchStatus.BatchHeader.BatchStatus, result.BatchHeader.BatchStatus);
        }
    }
}
