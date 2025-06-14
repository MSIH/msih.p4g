/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using System.Collections.Generic;

namespace msih.p4g.Server.Features.Base.PaymentService.Interfaces
{
    /// <summary>
    /// Interface for a factory that provides payment services
    /// </summary>
    public interface IPaymentServiceFactory
    {
        /// <summary>
        /// Gets a payment service by provider name
        /// </summary>
        /// <param name="providerName">The name of the payment provider</param>
        /// <returns>The payment service instance</returns>
        IPaymentService GetPaymentService(string providerName);
        
        /// <summary>
        /// Gets the default payment service based on configuration
        /// </summary>
        /// <returns>The default payment service</returns>
        IPaymentService GetDefaultPaymentService();
        
        /// <summary>
        /// Gets all available payment services
        /// </summary>
        /// <returns>A collection of all payment services</returns>
        IEnumerable<IPaymentService> GetAllPaymentServices();
    }
}