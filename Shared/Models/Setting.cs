using System.ComponentModel.DataAnnotations;

namespace msih.p4g.Shared.Models
{
    /// <summary>
    /// Represents a key-value application setting (for Email, SMS, etc.)
    /// </summary>
    public class Setting : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Key { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Value { get; set; }
    }
}
