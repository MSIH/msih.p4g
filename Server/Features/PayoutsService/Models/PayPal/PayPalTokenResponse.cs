/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using System.Text.Json.Serialization;

namespace msih.p4g.Server.Features.PayoutService.Models.PayPal
{
    /// <summary>
    /// PayPal OAuth token response
    /// </summary>
    public class PayPalTokenResponse
    {
        /// <summary>
        /// The OAuth 2.0 access token
        /// </summary>
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = null!;

        /// <summary>
        /// The token type (typically "Bearer")
        /// </summary>
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = null!;

        /// <summary>
        /// The time until the token expires, in seconds
        /// </summary>
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        /// <summary>
        /// The application client ID
        /// </summary>
        [JsonPropertyName("app_id")]
        public string? AppId { get; set; }

        /// <summary>
        /// The nonce returned in the response
        /// </summary>
        [JsonPropertyName("nonce")]
        public string? Nonce { get; set; }
    }
}
