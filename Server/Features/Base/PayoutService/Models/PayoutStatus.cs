using System;

namespace msih.p4g.Shared.Models.PayoutService
{
    public enum PaymentStatus
    {
        Pending,
        Processing,
        Completed,
        Failed,
        Cancelled
    }
}
