using System;
using System.ComponentModel.DataAnnotations;

namespace msih.p4g.Shared.Models
{
    /// <summary>
    /// Base class for entities that support soft delete and active status functionality
    /// </summary>
    public abstract class BaseEntity : IAuditableEntity
    {
        /// <summary>
        /// Gets or sets the primary key for the entity
        /// </summary>
        [Key]
        public int Id { get; set; }
        
        /// <summary>
        /// Gets or sets whether the entity is active
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether the entity is deleted (for soft delete)
        /// </summary>
        public bool IsDeleted { get; set; } = false;
        
        /// <summary>
        /// Gets or sets when the entity was created
        /// </summary>
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Gets or sets who created the entity
        /// </summary>
        [MaxLength(100)]
        public string CreatedBy { get; set; } = "System";
        
        /// <summary>
        /// Gets or sets when the entity was last modified
        /// </summary>
        public DateTime? ModifiedOn { get; set; }
        
        /// <summary>
        /// Gets or sets who last modified the entity
        /// </summary>
        [MaxLength(100)]
        public string ModifiedBy { get; set; }
    }
}
