using System;

namespace msih.p4g.Shared.Models
{
    /// <summary>
    /// Interface for entities that support soft delete and active status functionality
    /// </summary>
    public interface IAuditableEntity
    {
        /// <summary>
        /// Gets or sets whether the entity is active
        /// </summary>
        bool IsActive { get; set; }
        
        /// <summary>
        /// Gets or sets whether the entity is deleted (for soft delete)
        /// </summary>
        bool IsDeleted { get; set; }
        
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
