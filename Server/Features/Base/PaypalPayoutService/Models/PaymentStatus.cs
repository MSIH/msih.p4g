using System;

namespace msih.p4g.Shared.Models.PaymentService
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
