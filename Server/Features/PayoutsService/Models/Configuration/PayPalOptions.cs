/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

namespace msih.p4g.Server.Features.PayoutService.Models.Configuration
{
    /// <summary>
    /// PayPal API configuration options
    /// </summary>
    public class PayPalOptions
    {
        public const string SectionName = "PayPal";

        /// <summary>
        /// PayPal Client ID
        /// </summary>
        public string ClientId { get; set; } = null!;

        /// <summary>
        /// PayPal Secret
        /// </summary>
        public string Secret { get; set; } = null!;

        /// <summary>
        /// PayPal Environment (sandbox or live)
        /// </summary>
        public string Environment { get; set; } = null!;

        /// <summary>
        /// PayPal API URL (changes based on environment)
        /// </summary>
        public string ApiUrl => Environment.ToLower() == "live"
            ? "https://api.paypal.com"
            : "https://api.sandbox.paypal.com";
    }
}
