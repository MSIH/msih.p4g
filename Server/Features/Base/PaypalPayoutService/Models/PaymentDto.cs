using System;

namespace msih.p4g.Shared.Models.PaymentService
{
    public class PaymentDto
    {
        public string Id { get; set; }
        public string FundraiserId { get; set; }
        public string PaypalEmail { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string Status { get; set; }
        public string PaypalBatchId { get; set; }
        public string PaypalPayoutItemId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string Notes { get; set; }
        public string ErrorMessage { get; set; }
    }
}
