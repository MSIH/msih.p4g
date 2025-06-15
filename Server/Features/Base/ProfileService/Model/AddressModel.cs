// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using msih.p4g.Server.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace msih.p4g.Server.Features.Base.ProfileService.Model
{
    /// <summary>
    /// Represents a postal address for a donor or contact.
    /// </summary>
    public class AddressModel : BaseEntity
    {
        [MaxLength(100)]
        public required string Street { get; set; }
        [MaxLength(100)]
        public required string City { get; set; }
        [MaxLength(100)]
        public required string State { get; set; }
        [MaxLength(20)]
        public required string PostalCode { get; set; }
        [MaxLength(100)]
        public string? Country { get; set; }
    }
}
