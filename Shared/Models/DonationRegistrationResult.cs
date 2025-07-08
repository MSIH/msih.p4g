// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

namespace msih.p4g.Shared.Models
{
    /// <summary>
    /// Enumeration of possible donor registration result types
    /// </summary>
    public enum DonorRegistrationResultType
    {
        /// <summary>
        /// An existing donor was found for the email address
        /// </summary>
        ExistingFound,

        /// <summary>
        /// A new donor was successfully created
        /// </summary>
        NewCreated,

        /// <summary>
        /// An error occurred during the registration process
        /// </summary>
        Error
    }
}
}
