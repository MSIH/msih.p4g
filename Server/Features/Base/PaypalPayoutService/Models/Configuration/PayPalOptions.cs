namespace msih.p4g.Server.Features.Base.PaypalPayoutService.Models.Configuration
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
        public string ClientId { get; set; }

        /// <summary>
        /// PayPal Secret
        /// </summary>
        public string Secret { get; set; }

        /// <summary>
        /// PayPal Environment (sandbox or live)
        /// </summary>
        public string Environment { get; set; }

        /// <summary>
        /// PayPal API URL (changes based on environment)
        /// </summary>
        public string ApiUrl => Environment.ToLower() == "live" 
            ? "https://api.paypal.com" 
            : "https://api.sandbox.paypal.com";
    }
}
