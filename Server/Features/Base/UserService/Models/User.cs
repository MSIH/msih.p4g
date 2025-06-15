using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using msih.p4g.Shared.Models;

namespace msih.p4g.Server.Features.Base.UserService.Models
{
    public enum UserRole
    {
        Donor,
        Fundraiser,
        Admin
    }

    public class User : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public new int Id { get; set; } // Ensure auto-generated key

        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public UserRole Role { get; set; }

        public virtual msih.p4g.Shared.Models.Profile Profile { get; set; }
        public virtual msih.p4g.Shared.Models.Donor Donor { get; set; }

        public void ChangeRole(UserRole newRole)
        {
            Role = newRole;
        }
    }
}
