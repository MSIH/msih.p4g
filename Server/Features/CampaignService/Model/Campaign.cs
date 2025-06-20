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
using msih.p4g.Server.Features.DonationService.Models;
using System.ComponentModel.DataAnnotations;

namespace msih.p4g.Server.Features.CampaignService.Model
{
    /// <summary>
    /// Represents a donation campaign/service to which donors can contribute.
    /// </summary>
    public class Campaign : BaseEntity
    {
        /// <summary>
        /// Gets or sets the title of the campaign (single line).
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the campaign (multiline).
        /// </summary>
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Collection of donations associated with this campaign.
        /// </summary>
        public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();

        /// <summary>
        /// Indicates if this campaign is the default campaign.
        /// </summary>
        public bool IsDefault { get; set; }
    }

}
