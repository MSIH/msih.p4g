/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using System;

namespace msih.p4g.Server.Common.Models
{
    /// <summary>
    /// Interface for entities that support active status and audit functionality
    /// </summary>
    public interface IAuditableEntity
    {
        /// <summary>
        /// Gets or sets whether the entity is active
        /// </summary>
        bool IsActive { get; set; }
        
        /// <summary>
        /// Gets or sets when the entity was created
        /// </summary>
        DateTime CreatedOn { get; set; }
        
        /// <summary>
        /// Gets or sets who created the entity
        /// </summary>
        string CreatedBy { get; set; }
        
        /// <summary>
        /// Gets or sets when the entity was last modified
        /// </summary>
        DateTime? ModifiedOn { get; set; }
        
        /// <summary>
        /// Gets or sets who last modified the entity
        /// </summary>
        string ModifiedBy { get; set; }
    }
}
