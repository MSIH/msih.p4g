/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using System;

namespace msih.p4g.Shared.Models.PayoutService
{
    /// <summary>
    /// Represents the status of a payout
    /// </summary>
    public enum PayoutStatus
    {
        /// <summary>
        /// Payout is pending processing
        /// </summary>
        Pending,
        
        /// <summary>
        /// Payout is currently being processed
        /// </summary>
        Processing,
        
        /// <summary>
        /// Payout has been successfully completed
        /// </summary>
        Completed,
        
        /// <summary>
        /// Payout processing failed
        /// </summary>
        Failed,
        
        /// <summary>
        /// Payout was cancelled
        /// </summary>
        Cancelled
    }
}
