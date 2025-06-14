/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using msih.p4g.Server.Features.Base.PaymentService.Interfaces;

namespace msih.p4g.Server.Features.Base.PaymentService.Services
{
    /// <summary>
    /// Factory service that provides access to different payment service implementations
    /// </summary>
    public class PaymentServiceFactory : IPaymentServiceFactory
    {
        private readonly ILogger<PaymentServiceFactory> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, IPaymentService> _paymentServices = new();
        
        public PaymentServiceFactory(
            ILogger<PaymentServiceFactory> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }
        
        /// <summary>
        /// Gets a payment service by provider name
        /// </summary>
        public IPaymentService GetPaymentService(string providerName)
        {
            if (string.IsNullOrWhiteSpace(providerName))
            {
                throw new ArgumentException("Provider name cannot be null or empty", nameof(providerName));
            }
            
            // Check if we've already created an instance for this provider
            if (_paymentServices.TryGetValue(providerName, out var service))
            {
                return service;
            }
            
            // Create a new instance based on the provider name
            IPaymentService paymentService = providerName.ToLower() switch
            {
                "braintree" => _serviceProvider.GetRequiredService<BraintreePaymentService>(),
                // Add other providers here when implemented
                _ => throw new ArgumentException($"Unsupported payment provider: {providerName}", nameof(providerName))
            };
            
            // Initialize the service
            Task.Run(async () =>
            {
                try
                {
                    await paymentService.InitializeAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to initialize payment service for provider {providerName}");
                }
            }).Wait();
            
            // Cache the instance
            _paymentServices[providerName] = paymentService;
            
            return paymentService;
        }
        
        /// <summary>
        /// Gets the default payment service based on configuration
        /// </summary>
        public IPaymentService GetDefaultPaymentService()
        {
            try
            {
                // For now, Braintree is the default
                return GetPaymentService("Braintree");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get default payment service");
                throw;
            }
        }
        
        /// <summary>
        /// Gets all available payment services
        /// </summary>
        public IEnumerable<IPaymentService> GetAllPaymentServices()
        {
            var services = new List<IPaymentService>();
            
            try
            {
                // Add Braintree
                services.Add(GetPaymentService("Braintree"));
                
                // Add other providers as they are implemented
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all payment services");
            }
            
            return services;
        }
    }
}