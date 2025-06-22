/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.Extensions.Logging;
using Moq;
using msih.p4g.Client.Features.Base.Payment.Services;
using msih.p4g.Shared.Models.PaymentService;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace msih.p4g.Tests.Client.Tests.Features.Base.Payment
{
    public class PayPalPayoutServiceTests
    {
        private readonly Mock<ILogger<PayPalPayoutService>> _loggerMock;
        private readonly Mock<msih.p4g.Server.Features.Base.PaymentService.Interfaces.IPayPalPayoutService> _serverServiceMock;
        private readonly PayPalPayoutService _service;

        public PayPalPayoutServiceTests()
        {
            _loggerMock = new Mock<ILogger<PayPalPayoutService>>();
            _serverServiceMock = new Mock<msih.p4g.Server.Features.Base.PaymentService.Interfaces.IPayPalPayoutService>();
            _service = new PayPalPayoutService(_serverServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetFundraiserPaymentHistoryAsync_ShouldCallServerService()
        {
            // Arrange
            var fundraiserId = "test-fundraiser-id";
            var page = 1;
            var pageSize = 20;
            var expectedPayments = new List<PaymentDto> { new PaymentDto { Id = "payment1" } };
            
            _serverServiceMock.Setup(s => s.GetFundraiserPaymentHistoryAsync(fundraiserId, page, pageSize))
                .ReturnsAsync(expectedPayments);

            // Act
            var result = await _service.GetFundraiserPaymentHistoryAsync(fundraiserId, page, pageSize);

            // Assert
            Assert.Equal(expectedPayments, result);
            _serverServiceMock.Verify(s => s.GetFundraiserPaymentHistoryAsync(fundraiserId, page, pageSize), Times.Once);
        }

        [Fact]
        public async Task GetPaymentAsync_ShouldCallServerService()
        {
            // Arrange
            var paymentId = "test-payment-id";
            var expectedPayment = new PaymentDto { Id = paymentId };
            
            _serverServiceMock.Setup(s => s.GetPaymentAsync(paymentId))
                .ReturnsAsync(expectedPayment);

            // Act
            var result = await _service.GetPaymentAsync(paymentId);

            // Assert
            Assert.Equal(expectedPayment, result);
            _serverServiceMock.Verify(s => s.GetPaymentAsync(paymentId), Times.Once);
        }

        [Fact]
        public async Task CreatePaymentAsync_ShouldCallServerService()
        {
            // Arrange
            var fundraiserId = "test-fundraiser-id";
            var paypalEmail = "test@example.com";
            var amount = 100.00m;
            var currency = "USD";
            var notes = "Test payment";
            var expectedPayment = new PaymentDto { 
                Id = "new-payment-id",
                FundraiserId = fundraiserId,
                PaypalEmail = paypalEmail,
                Amount = amount,
                Currency = currency,
                Notes = notes
            };
            
            _serverServiceMock.Setup(s => s.CreatePaymentAsync(fundraiserId, paypalEmail, amount, currency, notes))
                .ReturnsAsync(expectedPayment);

            // Act
            var result = await _service.CreatePaymentAsync(fundraiserId, paypalEmail, amount, currency, notes);

            // Assert
            Assert.Equal(expectedPayment, result);
            _serverServiceMock.Verify(s => s.CreatePaymentAsync(fundraiserId, paypalEmail, amount, currency, notes), Times.Once);
        }

        [Fact]
        public async Task ProcessPaymentAsync_ShouldCallServerService()
        {
            // Arrange
            var paymentId = "test-payment-id";
            var expectedPayment = new PaymentDto { 
                Id = paymentId,
                Status = PaymentStatus.Processed
            };
            
            _serverServiceMock.Setup(s => s.ProcessPaymentAsync(paymentId))
                .ReturnsAsync(expectedPayment);

            // Act
            var result = await _service.ProcessPaymentAsync(paymentId);

            // Assert
            Assert.Equal(expectedPayment, result);
            _serverServiceMock.Verify(s => s.ProcessPaymentAsync(paymentId), Times.Once);
        }
    }
}
