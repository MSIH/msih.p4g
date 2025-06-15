using System.ComponentModel.DataAnnotations;
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
