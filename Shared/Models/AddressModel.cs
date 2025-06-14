/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using System.ComponentModel.DataAnnotations;

namespace msih.p4g.Shared.Models
{
    /// <summary>
    /// Represents a postal address for a donor or contact.
    /// </summary>
    public class AddressModel
    {
        [MaxLength(100)]
        public string Street { get; set; }
        [MaxLength(100)]
        public string City { get; set; }
        [MaxLength(100)]
        public string State { get; set; }
        [MaxLength(20)]
        public string PostalCode { get; set; }
        [MaxLength(100)]
        public string Country { get; set; }
    }
}
