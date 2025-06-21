using System;
using msih.p4g.Shared.Models.PaymentService;
using msih.p4g.Server.Common.Models;

namespace msih.p4g.Server.Features.Base.PaypalPayoutService.Models
{
    public class Payment : BaseEntity
    {
        public string FundraiserId { get; set; } = null!;
        public string PaypalEmail { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string? PaypalBatchId { get; set; }
        public string? PaypalPayoutItemId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
        public string? Notes { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
