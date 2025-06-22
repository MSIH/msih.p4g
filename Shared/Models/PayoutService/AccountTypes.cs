/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
namespace msih.p4g.Shared.Models.PayoutService
{
    /// <summary>
    /// Represents the type of account used for payouts
    /// </summary>
    public enum AccountType
    {
        /// <summary>
        /// PayPal account
        /// </summary>
        PayPal,
        
        /// <summary>
        /// Bank account
        /// </summary>
        Bank,
        
        /// <summary>
        /// Venmo account
        /// </summary>
        Venmo,
        
        /// <summary>
        /// Other account type
        /// </summary>
        Other
    }

    /// <summary>
    /// Represents the format of the account identifier
    /// </summary>
    public enum AccountFormat
    {
        /// <summary>
        /// Email address format
        /// </summary>
        Email,
        
        /// <summary>
        /// Phone number format
        /// </summary>
        Phone,
        
        /// <summary>
        /// Account number format
        /// </summary>
        AccountNumber,
        
        /// <summary>
        /// Username format
        /// </summary>
        Username,
        
        /// <summary>
        /// Other format
        /// </summary>
        Other
    }
}
